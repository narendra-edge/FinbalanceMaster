# Etl/nav_fetcher.py (FIXED VERSION)
"""
NAV Data Fetcher - FIXED COLUMN MAPPINGS
-----------------------------------------
Extracts NAV data from:
1. CAMS Historical (ZIP → DBF)
2. KFIN Historical (ZIP → DBF)
3. AMFI Daily (TXT download)
"""

import logging
import re
import zipfile
from pathlib import Path
from datetime import datetime
from typing import List, Optional
import requests
import pandas as pd
from dbfread import DBF

from Etl.utils import ensure_dir, build_retry, settings
from Etl.unzip_utils import extract_zip

logger = logging.getLogger("mf.etl.nav_fetcher")


class NavFetcher:
    """
    Unified NAV data fetcher for CAMS, KFIN historical DBF files
    and AMFI daily TXT feed
    """
    
    def __init__(self, config: dict, paths: dict):
        self.config = config
        self.paths = paths
        
        # Historical NAV directories
        self.cams_historical_dir = Path(config.get("cams_nav_historical_dir", 
                                        "C:/Data_MF/Cams_Data_HistoricalNav"))
        self.kfin_historical_dir = Path(config.get("kfin_nav_historical_dir",
                                        "C:/Data_MF/Kfin_Data_HistoricalNav"))
        
        # Output directories
        self.output_dir = Path(paths.get("nav_output_dir", 
                               paths.get("downloads_dir") + "/nav"))
        ensure_dir(self.output_dir)
        
        # AMFI daily NAV URL
        self.amfi_nav_url = config.get("amfi_nav_url", 
                                       "https://portal.amfiindia.com/spages/NAVAll.txt")
    
    # ================================================================
    # CAMS HISTORICAL NAV (ZIP → DBF → CSV)
    # ================================================================
    
    def extract_cams_historical(self) -> List[Path]:
        """
        Extract CAMS historical NAV from ZIP files containing DBF
        Returns list of extracted CSV files
        """
        logger.info("📦 Extracting CAMS historical NAV data...")
        
        if not self.cams_historical_dir.exists():
            logger.warning(f"⚠️  CAMS historical directory not found: {self.cams_historical_dir}")
            return []
        
        zip_files = list(self.cams_historical_dir.glob("*.zip"))
        if not zip_files:
            logger.warning("⚠️  No ZIP files found in CAMS historical directory")
            return []
        
        logger.info(f"Found {len(zip_files)} CAMS ZIP files")
        
        extracted_csvs = []
        temp_extract_dir = self.output_dir / "cams_temp"
        ensure_dir(temp_extract_dir)
        
        for zip_path in zip_files:
            try:
                logger.info(f"📂 Processing: {zip_path.name}")
                
                # Get password from config
                password = self.config.get("cams_zip_password", 
                                          self.config.get("gmail", {}).get("zip_password"))
                
                # Extract ZIP
                extracted_files = extract_zip(str(zip_path), str(temp_extract_dir), password)
                
                # Process DBF files
                for file_path in extracted_files:
                    file_path = Path(file_path)
                    if file_path.suffix.lower() == '.dbf':
                        csv_path = self._convert_cams_dbf_to_csv(file_path)
                        if csv_path:
                            extracted_csvs.append(csv_path)
                
            except Exception as e:
                logger.exception(f"❌ Failed to process {zip_path.name}: {e}")
                continue
        
        logger.info(f"✅ Extracted {len(extracted_csvs)} CAMS NAV files")
        return extracted_csvs
    
    def _convert_cams_dbf_to_csv(self, dbf_path: Path) -> Optional[Path]:
        """
        Convert CAMS DBF file to CSV with proper column mapping
        
        ACTUAL CAMS NAV COLUMNS (from your CSV):
        - product_code
        - product_name
        - NAV_DATE (UPPERCASE!)
        - NAV_VALUE (UPPERCASE!)
        - DIV_PERUNI
        - CORPDIVRAT
        - SCHEME_TYP
        - isin_no
        - SWING_NAV
        """
        try:
            logger.info(f"🔄 Converting CAMS DBF: {dbf_path.name}")
            
            # Read DBF
            table = DBF(str(dbf_path), encoding='latin1', char_decode_errors='ignore')
            records = list(table)
            
            if not records:
                logger.warning(f"⚠️  Empty DBF file: {dbf_path.name}")
                return None
            
            df = pd.DataFrame(records)
            
            # Log actual columns for debugging
            logger.info(f"📋 Actual DBF columns: {list(df.columns)}")
            
            # Normalize to uppercase for mapping
            df.columns = [col.upper().strip() for col in df.columns]
            
            # FIXED: Column mapping based on ACTUAL CAMS format
            column_mapping = {
                # Product identification
                'PRODCODE': 'product_code',
                'PRODUCT_CODE': 'product_code',
                'PCODE': 'product_code',
                'CODE': 'product_code',
                
                'PRODNAME': 'product_name',
                'PRODUCT_NAME': 'product_name',
                'PNAME': 'product_name',
                'NAME': 'product_name',
                
                # Date - UPPERCASE in CAMS!
                'NAV_DATE': 'nav_date',  # This is the actual column name
                'NAVDATE': 'nav_date',
                'DATE': 'nav_date',
                
                # NAV Value - UPPERCASE in CAMS!
                'NAV_VALUE': 'nav_value',  # This is the actual column name
                'NAV': 'nav_value',
                'NAVVALUE': 'nav_value',
                
                # Dividend fields
                'DIV_PERUNI': 'dividend_per_unit',
                'DIVUNIT': 'dividend_per_unit',
                'DIVIDEND': 'dividend_per_unit',
                
                'CORPDIVRAT': 'corp_div_rate',
                'CORPDIV': 'corp_div_rate',
                'CORPRATE': 'corp_div_rate',
                
                # Scheme info
                'SCHEME_TYP': 'scheme_type',
                'SCHTYPE': 'scheme_type',
                'TYPE': 'scheme_type',
                
                # ISIN
                'ISIN_NO': 'isin_no',
                'ISIN': 'isin_no',
                
                # Swing NAV (new field)
                'SWING_NAV': 'swing_nav',
            }
            
            # Apply mapping
            df.rename(columns=column_mapping, inplace=True)
            
            # Verify required columns exist
            required_cols = ['nav_date', 'nav_value', 'isin_no']
            missing_cols = [col for col in required_cols if col not in df.columns]
            
            if missing_cols:
                logger.error(f"❌ Missing required columns in {dbf_path.name}: {missing_cols}")
                logger.error(f"   Available columns after mapping: {list(df.columns)}")
                return None
            
            # Convert date format (mm/dd/yyyy → yyyy-mm-dd)
            df['nav_date'] = pd.to_datetime(df['nav_date'], errors='coerce')
            
            # Convert numeric fields
            numeric_cols = ['nav_value', 'dividend_per_unit', 'corp_div_rate', 'swing_nav']
            for col in numeric_cols:
                if col in df.columns:
                    df[col] = pd.to_numeric(df[col], errors='coerce')
            
            # Save as CSV
            csv_path = self.output_dir / f"cams_nav_{dbf_path.stem}.csv"
            df.to_csv(csv_path, index=False)
            
            logger.info(f"✅ Converted {len(df)} records → {csv_path.name}")
            return csv_path
            
        except Exception as e:
            logger.exception(f"❌ Failed to convert CAMS DBF {dbf_path.name}: {e}")
            return None
    
    # ================================================================
    # KFIN HISTORICAL NAV (ZIP → DBF → CSV)
    # ================================================================
    
    def extract_kfin_historical(self) -> List[Path]:
        """
        Extract KFIN historical NAV from ZIP files containing DBF
        Returns list of extracted CSV files
        """
        logger.info("📦 Extracting KFIN historical NAV data...")
        
        if not self.kfin_historical_dir.exists():
            logger.warning(f"⚠️  KFIN historical directory not found: {self.kfin_historical_dir}")
            return []
        
        zip_files = list(self.kfin_historical_dir.glob("*.zip"))
        if not zip_files:
            logger.warning("⚠️  No ZIP files found in KFIN historical directory")
            return []
        
        logger.info(f"Found {len(zip_files)} KFIN ZIP files")
        
        extracted_csvs = []
        temp_extract_dir = self.output_dir / "kfin_temp"
        ensure_dir(temp_extract_dir)
        
        for zip_path in zip_files:
            try:
                logger.info(f"📂 Processing: {zip_path.name}")
                
                # Get password from config
                password = self.config.get("kfin_zip_password", 
                                          self.config.get("kfin", {}).get("zip_password"))
                
                # Extract ZIP
                extracted_files = extract_zip(str(zip_path), str(temp_extract_dir), password)
                
                # Process DBF files
                for file_path in extracted_files:
                    file_path = Path(file_path)
                    if file_path.suffix.lower() == '.dbf':
                        csv_path = self._convert_kfin_dbf_to_csv(file_path)
                        if csv_path:
                            extracted_csvs.append(csv_path)
                
            except Exception as e:
                logger.exception(f"❌ Failed to process {zip_path.name}: {e}")
                continue
        
        logger.info(f"✅ Extracted {len(extracted_csvs)} KFIN NAV files")
        return extracted_csvs
    
    def _convert_kfin_dbf_to_csv(self, dbf_path: Path) -> Optional[Path]:
        """
        Convert KFIN DBF file to CSV with proper column mapping
        
        ACTUAL KFIN NAV COLUMNS (from your data):
        - fund (AMC code)
        - scheme (Scheme Code)
        - funddesc (Description)
        - fcode (Product code)
        - navdate (NAV date) - lowercase!
        - nav (NAV value) - lowercase!
        - rpop (Redemption price)
        - ppop (Purchase price)
        - crdate (Report date)
        - crtime (Report time)
        - schemeisin (ISIN)
        """
        try:
            logger.info(f"🔄 Converting KFIN DBF: {dbf_path.name}")
            
            # Read DBF
            table = DBF(str(dbf_path), encoding='latin1', char_decode_errors='ignore')
            records = list(table)
            
            if not records:
                logger.warning(f"⚠️  Empty DBF file: {dbf_path.name}")
                return None
            
            df = pd.DataFrame(records)
            
            # Log actual columns
            logger.info(f"📋 Actual DBF columns: {list(df.columns)}")
            
            # KFIN columns are lowercase - normalize to uppercase for consistent mapping
            df.columns = [col.upper().strip() for col in df.columns]
            
            # FIXED: Column mapping based on ACTUAL KFIN format
            column_mapping = {
                'FUND': 'fund',
                'SCHEME': 'scheme',
                'FUNDDESC': 'funddesc',
                'FCODE': 'fcode',
                'NAVDATE': 'navdate',  # Already correct
                'NAV': 'nav',          # Already correct
                'PPOP': 'ppop',        # Purchase price
                'RPOP': 'rpop',        # Redemption price
                'CRDATE': 'crdate',
                'CRTIME': 'crtime',
                'SCHEMEISIN': 'schemeisin',
                'ISIN': 'schemeisin',
            }
            
            df.rename(columns=column_mapping, inplace=True)
            
            # Verify required columns
            required_cols = ['navdate', 'nav', 'schemeisin']
            missing_cols = [col for col in required_cols if col not in df.columns]
            
            if missing_cols:
                logger.error(f"❌ Missing required columns in {dbf_path.name}: {missing_cols}")
                logger.error(f"   Available columns after mapping: {list(df.columns)}")
                return None
            
            # Convert date format
            df['navdate'] = pd.to_datetime(df['navdate'], errors='coerce')
            if 'crdate' in df.columns:
                df['crdate'] = pd.to_datetime(df['crdate'], errors='coerce')
            
            # Convert numeric fields
            numeric_cols = ['nav', 'ppop', 'rpop']
            for col in numeric_cols:
                if col in df.columns:
                    df[col] = pd.to_numeric(df[col], errors='coerce')
            
            # Save as CSV
            csv_path = self.output_dir / f"kfin_nav_{dbf_path.stem}.csv"
            df.to_csv(csv_path, index=False)
            
            logger.info(f"✅ Converted {len(df)} records → {csv_path.name}")
            return csv_path
            
        except Exception as e:
            logger.exception(f"❌ Failed to convert KFIN DBF {dbf_path.name}: {e}")
            return None
    
    # ================================================================
    # AMFI DAILY NAV (TXT download and parsing)
    # ================================================================
    
    @build_retry(max_attempts=3)
    def fetch_amfi_daily_nav(self) -> Optional[Path]:
        """
        Download and parse AMFI daily NAV TXT file
        
        Format example:
        Open Ended Schemes(Debt Scheme - Banking and PSU Fund)
        
        Aditya Birla Sun Life Mutual Fund
        119551;INF209K01LX5;;Aditya Birla Sun Life Banking & PSU Debt Fund - DIRECT - IDCW;275.729;26-Nov-2025
        119552;INF209K01LY3;;Aditya Birla Sun Life Banking & PSU Debt Fund - DIRECT - Growth;382.661;26-Nov-2025
        
        HDFC Mutual Fund
        ...
        """
        logger.info("📡 Downloading AMFI daily NAV data...")
        
        try:
            response = requests.get(self.amfi_nav_url, timeout=60)
            response.raise_for_status()
            
            # Save raw TXT
            timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
            raw_txt_path = self.output_dir / f"amfi_nav_daily_{timestamp}.txt"
            
            with open(raw_txt_path, 'wb') as f:
                f.write(response.content)
            
            logger.info(f"✅ Downloaded AMFI NAV → {raw_txt_path.name}")
            
            # Parse and clean
            csv_path = self._parse_amfi_nav_txt(raw_txt_path)
            return csv_path
            
        except Exception as e:
            logger.exception(f"❌ Failed to fetch AMFI daily NAV: {e}")
            return None
    
    def _parse_amfi_nav_txt(self, txt_path: Path) -> Optional[Path]:
        """
        Parse AMFI NAV TXT file into structured CSV
        
        Rules:
        - Lines with "Open Ended Schemes(...)" = Category headers
        - Lines with just AMC name = AMC headers
        - Data lines: Scheme Code;ISIN1;ISIN2;Scheme Name;NAV;Date
        """
        logger.info(f"🔄 Parsing AMFI NAV TXT: {txt_path.name}")
        
        try:
            records = []
            current_category = None
            current_amc = None
            
            with open(txt_path, 'r', encoding='utf-8', errors='replace') as f:
                for line in f:
                    line = line.strip()
                    
                    if not line:
                        continue
                    
                    # Category header (contains "Schemes(" or similar)
                    if re.search(r'Schemes?\s*\(', line, re.IGNORECASE):
                        current_category = line
                        logger.debug(f"Category: {current_category}")
                        continue
                    
                    # Check if line is data (contains semicolons)
                    if ';' in line:
                        parts = line.split(';')
                        
                        if len(parts) >= 6:
                            # Data line
                            records.append({
                                'scheme_code': parts[0].strip(),
                                'isin_div_payout_growth': parts[1].strip(),
                                'isin_div_reinvestment': parts[2].strip(),
                                'scheme_name': parts[3].strip(),
                                'net_asset_value': parts[4].strip(),
                                'nav_date': parts[5].strip(),
                                'amc_name': current_amc,
                                'category': current_category,
                            })
                    else:
                        # Assume it's AMC name
                        current_amc = line
                        logger.debug(f"AMC: {current_amc}")
            
            if not records:
                logger.warning("⚠️  No NAV data found in AMFI TXT")
                return None
            
            # Create DataFrame
            df = pd.DataFrame(records)
            
            # Convert date (format: DD-Mon-YYYY)
            df['nav_date'] = pd.to_datetime(df['nav_date'], format='%d-%b-%Y', errors='coerce')
            
            # Convert NAV to numeric
            df['net_asset_value'] = pd.to_numeric(df['net_asset_value'], errors='coerce')
            
            # Save CSV
            csv_path = self.output_dir / f"amfi_nav_daily_{txt_path.stem}.csv"
            df.to_csv(csv_path, index=False)
            
            logger.info(f"✅ Parsed {len(df)} NAV records → {csv_path.name}")
            return csv_path
            
        except Exception as e:
            logger.exception(f"❌ Failed to parse AMFI NAV TXT: {e}")
            return None
    
    # ================================================================
    # MAIN FETCH ORCHESTRATOR
    # ================================================================
    
    def fetch_all_nav_data(self) -> dict:
        """
        Fetch all NAV data sources
        Returns dict with lists of extracted CSV paths
        """
        logger.info("🚀 Starting NAV data extraction...")
        
        results = {
            'cams_historical': [],
            'kfin_historical': [],
            'amfi_daily': None,
        }
        
        # 1. CAMS Historical
        try:
            results['cams_historical'] = self.extract_cams_historical()
        except Exception as e:
            logger.exception(f"❌ CAMS historical extraction failed: {e}")
        
        # 2. KFIN Historical
        try:
            results['kfin_historical'] = self.extract_kfin_historical()
        except Exception as e:
            logger.exception(f"❌ KFIN historical extraction failed: {e}")
        
        # 3. AMFI Daily
        try:
            results['amfi_daily'] = self.fetch_amfi_daily_nav()
        except Exception as e:
            logger.exception(f"❌ AMFI daily extraction failed: {e}")
        
        logger.info("🎉 NAV data extraction complete!")
        logger.info(f"   CAMS Historical: {len(results['cams_historical'])} files")
        logger.info(f"   KFIN Historical: {len(results['kfin_historical'])} files")
        logger.info(f"   AMFI Daily: {'✅' if results['amfi_daily'] else '❌'}")
        
        return results