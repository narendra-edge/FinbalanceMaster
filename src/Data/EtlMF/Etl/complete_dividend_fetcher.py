# Etl/complete_dividend_fetcher.py
"""
Complete Dividend Fetcher
--------------------------
Handles TWO dividend data sources:

1. HISTORICAL: DBF files in password-protected ZIPs
   - CAMS: C:/Data_MF/Cams_Data_HistoricalNav (same location as NAV)
   - KFIN: C:/Data_MF/Kfin_Data_HistoricalNav (same location as NAV)

2. DAILY: Gmail emails with Excel downloads
   - CAMS: Subject "WBR25. IDCW/Bonus Declared"
   - KFIN: From "distributorcare@kfintech.com"
"""

import logging
from pathlib import Path
from typing import List, Optional, Dict
import pandas as pd
from dbfread import DBF

from Etl.utils import ensure_dir
from Etl.unzip_utils import extract_zip
from Etl.gmail_dividend_fetcher import GmailDividendFetcher  # FIXED: Changed import

logger = logging.getLogger("mf.etl.complete_dividend_fetcher")


class CompleteDividendFetcher:
    """
    Unified fetcher for both historical (DBF) and daily (Gmail) dividend data
    """
    
    def __init__(self, config: dict, paths: dict):
        self.config = config
        self.paths = paths
        
        dividend_cfg = config.get("dividend", {})
        
        # Historical dividend directories (same as NAV)
        self.cams_historical_dir = Path(
            dividend_cfg.get("cams_historical_dir",
                           config.get("nav", {}).get("cams_historical_dir",
                           "C:/Data_MF/Cams_Data_HistoricalDividend"))
        )
        self.kfin_historical_dir = Path(
            dividend_cfg.get("kfin_historical_dir",
                           config.get("nav", {}).get("kfin_historical_dir",
                           "C:/Data_MF/Kfin_Data_HistoricalDividend"))
        )
        
        # Output directories
        self.output_dir = Path(paths.get("dividend_output_dir",
                              "C:/Data_MF/downloads/dividend"))
        ensure_dir(self.output_dir)
        
        # Initialize Gmail fetcher for daily dividends
        self.gmail_fetcher = GmailDividendFetcher(config, paths)  # FIXED: Use correct class
        
        logger.info(f"📁 CAMS Historical: {self.cams_historical_dir}")
        logger.info(f"📁 KFIN Historical: {self.kfin_historical_dir}")
        logger.info(f"📁 Output: {self.output_dir}")
    
    # ================================================================
    # HISTORICAL DIVIDENDS (DBF from ZIP files)
    # ================================================================
    
    def extract_cams_historical_dividends(self) -> List[Path]:
        """
        Extract CAMS historical dividend DBF files from ZIPs
        Returns list of extracted CSV files
        """
        logger.info("📦 Extracting CAMS historical dividends (DBF)...")
        
        if not self.cams_historical_dir.exists():
            logger.warning(f"⚠️  CAMS historical directory not found: {self.cams_historical_dir}")
            return []
        
        zip_files = list(self.cams_historical_dir.glob("*.zip"))
        if not zip_files:
            logger.warning("⚠️  No ZIP files found in CAMS historical directory")
            return []
        
        logger.info(f"Found {len(zip_files)} CAMS ZIP files")
        
        extracted_csvs = []
        temp_extract_dir = self.output_dir / "cams_hist_temp"
        ensure_dir(temp_extract_dir)
        
        for zip_path in zip_files:
            try:
                logger.info(f"📂 Processing: {zip_path.name}")
                
                # Get password
                password = self.config.get("nav", {}).get("cams_zip_password",
                          self.config.get("gmail", {}).get("zip_password"))
                
                # Extract ZIP
                extracted_files = extract_zip(str(zip_path), str(temp_extract_dir), password)
                
                # Process DBF files (look for dividend-specific DBF)
                for file_path in extracted_files:
                    file_path = Path(file_path)
                    if file_path.suffix.lower() == '.dbf':
                        file_stem_lower = file_path.stem.lower()
                        # More flexible keyword matching - check if it's NOT a NAV file
                        # CAMS dividend files often don't have 'div' in name, just process all DBF
                        # that aren't clearly NAV files
                        is_nav_file = any(kw in file_stem_lower for kw in ['nav', 'w1n', 'w2n', 'w3n', 'w4n'])
                        
                        if not is_nav_file:
                            logger.info(f"🔍 Processing potential dividend DBF: {file_path.name}")
                            csv_path = self._convert_cams_dividend_dbf_to_csv(file_path)
                            if csv_path:
                                extracted_csvs.append(csv_path)
                
            except Exception as e:
                logger.exception(f"❌ Failed to process {zip_path.name}: {e}")
                continue
        
        logger.info(f"✅ Extracted {len(extracted_csvs)} CAMS historical dividend files")
        return extracted_csvs
    
    def _convert_cams_dividend_dbf_to_csv(self, dbf_path: Path) -> Optional[Path]:
        """
        Convert CAMS dividend DBF to CSV
        
        Expected fields:
        - product_code
        - scheme_nam
        - dividend_bonus_flag
        - ex_dividend_date
        - record_date
        - dividend_rate_per_unit
        - bonus_ratio
        - corp_div_rate
        """
        try:
            logger.info(f"🔄 Converting CAMS Dividend DBF: {dbf_path.name}")
            
            table = DBF(str(dbf_path), encoding='latin1', char_decode_errors='ignore')
            records = list(table)
            
            if not records:
                logger.warning(f"⚠️  Empty DBF file: {dbf_path.name}")
                return None
            
            df = pd.DataFrame(records)
            logger.info(f"📋 Columns in {dbf_path.name}: {list(df.columns)}")
            
            # Normalize column names
            df.columns = [col.upper().strip() for col in df.columns]
            
            # Column mapping (adjust based on actual DBF structure)
            column_mapping = {
                'PRODCODE': 'product_code',
                'PCODE': 'product_code',
    
                # DBF truncates to 10 chars!
                'SCHEME_NAM': 'scheme_name',  # Was SCHEMENAME
                'SCHNAME': 'scheme_name',
                'NAME': 'scheme_name',
    
                'DIVI_BONUS': 'dividend_bonus_flag',  # Truncated
                'DIVFLAG': 'dividend_bonus_flag',
                'FLAG': 'dividend_bonus_flag',
                'TYPE': 'dividend_bonus_flag',
    
                'EX_DIV_DAT': 'ex_dividend_date',  # Truncated
                'EXDIVDATE': 'ex_dividend_date',
                'EXDATE': 'ex_dividend_date',
    
                 'RECORD_DAT': 'record_date',  # Truncated
                 'RECDATE': 'record_date',
                 'RECORDDATE': 'record_date',
    
                 'DIV_RATE_P': 'dividend_rate_per_unit',  # Truncated
                 'DIVRATE': 'dividend_rate_per_unit',
                 'RATE': 'dividend_rate_per_unit',
    
                 'BONUS_RATI': 'bonus_ratio',  # Truncated
                 'BONUSRATIO': 'bonus_ratio',
                 'RATIO': 'bonus_ratio',
    
                 'CORP_DIV_R': 'corp_div_rate',  # Truncated
                 'CORPDIV': 'corp_div_rate',
                 'CORPRATE': 'corp_div_rate',
    
                  'ISIN': 'isin',
                  'ISIN_NO': 'isin',
            }
            
            df.rename(columns=column_mapping, inplace=True)
            
            # Convert dates
            date_cols = ['ex_dividend_date', 'record_date']
            for col in date_cols:
                if col in df.columns:
                    df[col] = pd.to_datetime(df[col], errors='coerce')
            
            # Convert numeric
            numeric_cols = ['dividend_rate_per_unit', 'corp_div_rate']
            for col in numeric_cols:
                if col in df.columns:
                    df[col] = pd.to_numeric(df[col], errors='coerce')
            
            # Save CSV
            csv_path = self.output_dir / f"cams_dividend_hist_{dbf_path.stem}.csv"
            df.to_csv(csv_path, index=False)
            
            logger.info(f"✅ Converted {len(df)} dividend records → {csv_path.name}")
            return csv_path
            
        except Exception as e:
            logger.exception(f"❌ Failed to convert CAMS dividend DBF {dbf_path.name}: {e}")
            return None
    
    def extract_kfin_historical_dividends(self) -> List[Path]:
        """
        Extract KFIN historical dividend DBF files from ZIPs
        Returns list of extracted CSV files
        """
        logger.info("📦 Extracting KFIN historical dividends (DBF)...")
        
        if not self.kfin_historical_dir.exists():
            logger.warning(f"⚠️  KFIN historical directory not found: {self.kfin_historical_dir}")
            return []
        
        zip_files = list(self.kfin_historical_dir.glob("*.zip"))
        if not zip_files:
            logger.warning("⚠️  No ZIP files found in KFIN historical directory")
            return []
        
        logger.info(f"Found {len(zip_files)} KFIN ZIP files")
        
        extracted_csvs = []
        temp_extract_dir = self.output_dir / "kfin_hist_temp"
        ensure_dir(temp_extract_dir)
        
        for zip_path in zip_files:
            try:
                logger.info(f"📂 Processing: {zip_path.name}")
                
                # Get password
                password = self.config.get("nav", {}).get("kfin_zip_password")
                
                # Extract ZIP
                extracted_files = extract_zip(str(zip_path), str(temp_extract_dir), password)
                
                # Process DBF and CSV files
                for file_path in extracted_files:
                    file_path = Path(file_path)
                    file_stem_lower = file_path.stem.lower()
                    
                    # KFIN uses CSV for dividends sometimes
                    if file_path.suffix.lower() == '.csv':
                        # Check if it's a dividend CSV
                        if any(kw in file_stem_lower for kw in ['div', 'dividend', 'idcw', 'bonus']):
                            logger.info(f"🔍 Found KFIN dividend CSV: {file_path.name}")
                            # Copy CSV directly to output
                            csv_path = self.output_dir / f"kfin_dividend_hist_{file_path.stem}.csv"
                            import shutil
                            shutil.copy(str(file_path), str(csv_path))
                            extracted_csvs.append(csv_path)
                    
                    elif file_path.suffix.lower() == '.dbf':
                        # Check if it's NOT a NAV file
                        is_nav_file = any(kw in file_stem_lower for kw in ['nav', 'w1n', 'w2n', 'w3n', 'w4n'])
                        
                        if not is_nav_file:
                            logger.info(f"🔍 Processing potential dividend DBF: {file_path.name}")
                            csv_path = self._convert_kfin_dividend_dbf_to_csv(file_path)
                            if csv_path:
                                extracted_csvs.append(csv_path)
                
            except Exception as e:
                logger.exception(f"❌ Failed to process {zip_path.name}: {e}")
                continue
        
        logger.info(f"✅ Extracted {len(extracted_csvs)} KFIN historical dividend files")
        return extracted_csvs
    
    def _convert_kfin_dividend_dbf_to_csv(self, dbf_path: Path) -> Optional[Path]:
        """
        Convert KFIN dividend DBF to CSV
        
        Expected fields:
        - fund (AMC)
        - scheme (Scheme code)
        - pln (Plan code)
        - funddesc (Description)
        - fcode (Product code)
        - div_date (Dividend date)
        - nday (Number of days)
        - drate (Dividend rate)
        - dnav (Ex-Dividend NAV)
        - status
        - reinvdt
       
        """
        try:
            logger.info(f"🔄 Converting KFIN Dividend DBF: {dbf_path.name}")
            
            table = DBF(str(dbf_path), encoding='latin1', char_decode_errors='ignore')
            records = list(table)
            
            if not records:
                logger.warning(f"⚠️  Empty DBF file: {dbf_path.name}")
                return None
            
            df = pd.DataFrame(records)
            logger.info(f"📋 Columns in {dbf_path.name}: {list(df.columns)}")
            
            # Normalize column names
            df.columns = [col.upper().strip() for col in df.columns]
            
            # Column mapping
            column_mapping = {
                'FUND': 'fund',
                'SCHEME': 'scheme',
                'PLN': 'pln',
                'PLAN': 'pln',
                'FUNDDESC': 'funddesc',
                'DESC': 'funddesc',
                'FCODE': 'fcode',
                'DIVDATE': 'div_date',
                'DIV_DATE': 'div_date',
                'NDAY': 'nday',
                'DRATE': 'drate',
                'RATE': 'drate',
                'DNAV': 'dnav',
                'STATUS': 'status',
                'ISIN': 'isin',
                'SCHEMEISIN': 'isin',
                'REINVDT': 'reinvdt',
            }
            
            df.rename(columns=column_mapping, inplace=True)
            
            # Convert dates
            if 'div_date' in df.columns:
                df['div_date'] = pd.to_datetime(df['div_date'], errors='coerce')
            
            # Convert numeric
            numeric_cols = ['nday', 'drate', 'dnav']
            for col in numeric_cols:
                if col in df.columns:
                    df[col] = pd.to_numeric(df[col], errors='coerce')
            
            # Save CSV
            csv_path = self.output_dir / f"kfin_dividend_hist_{dbf_path.stem}.csv"
            df.to_csv(csv_path, index=False)
            
            logger.info(f"✅ Converted {len(df)} dividend records → {csv_path.name}")
            return csv_path
            
        except Exception as e:
            logger.exception(f"❌ Failed to convert KFIN dividend DBF {dbf_path.name}: {e}")
            return None
    
    # ================================================================
    # DAILY DIVIDENDS (Gmail Excel) - Delegate to GmailDividendFetcher
    # ================================================================
    
    def fetch_daily_dividends(self) -> Dict[str, List[Path]]:
        """
        Fetch daily dividend files from Gmail
        Returns dict with CAMS and KFIN Excel file lists
        """
        logger.info("📧 Fetching daily dividends from Gmail...")
        
        try:
            return self.gmail_fetcher.fetch_all_dividend_files()
        except Exception as e:
            logger.exception(f"❌ Failed to fetch daily dividends from Gmail: {e}")
            return {'cams_dividend': [], 'kfin_dividend': []}
    
    # ================================================================
    # MAIN ORCHESTRATOR
    # ================================================================
    
    def fetch_all_dividend_data(self) -> dict:
        """
        Fetch ALL dividend data:
        - Historical (DBF from ZIPs)
        - Daily (Excel from Gmail)
        
        Returns dict with all extracted file paths
        """
        logger.info("🚀 Starting Complete Dividend Data Extraction...")
        logger.info("=" * 60)
        
        results = {
            'cams_historical': [],
            'kfin_historical': [],
            'cams_daily': [],
            'kfin_daily': [],
        }
        
        # ----------------------------------------------------------------
        # PHASE 1: Historical Dividends (DBF)
        # ----------------------------------------------------------------
        logger.info("\n📊 PHASE 1: Historical Dividends (DBF)")
        logger.info("-" * 60)
        
        try:
            results['cams_historical'] = self.extract_cams_historical_dividends()
        except Exception as e:
            logger.exception(f"❌ CAMS historical extraction failed: {e}")
        
        try:
            results['kfin_historical'] = self.extract_kfin_historical_dividends()
        except Exception as e:
            logger.exception(f"❌ KFIN historical extraction failed: {e}")
        
        # ----------------------------------------------------------------
        # PHASE 2: Daily Dividends (Gmail Excel)
        # ----------------------------------------------------------------
        logger.info("\n📧 PHASE 2: Daily Dividends (Gmail)")
        logger.info("-" * 60)
        
        daily_results = self.fetch_daily_dividends()
        results['cams_daily'] = daily_results.get('cams_dividend', [])
        results['kfin_daily'] = daily_results.get('kfin_dividend', [])
        
        # ----------------------------------------------------------------
        # Summary
        # ----------------------------------------------------------------
        logger.info("\n" + "=" * 60)
        logger.info("🎉 Complete Dividend Extraction Summary:")
        logger.info(f"   CAMS Historical (DBF): {len(results['cams_historical'])} files")
        logger.info(f"   KFIN Historical (DBF): {len(results['kfin_historical'])} files")
        logger.info(f"   CAMS Daily (Gmail): {len(results['cams_daily'])} files")
        logger.info(f"   KFIN Daily (Gmail): {len(results['kfin_daily'])} files")
        logger.info("=" * 60)
        
        return results