# Etl/nav_loader.py
"""
NAV Data Loader
-------------------------
Enhanced version with:
- Better error handling
- ISIN validation and truncation
- Proper raw_row JSONB handling
- Batch processing
- Data quality checks
"""

import logging
from pathlib import Path
from typing import List, Dict
import pandas as pd
from sqlalchemy import text
import json

from Etl.utils import engine
from Etl.excel_to_postgres import load_dataframe_to_table, safe_read_excel
from Etl.cleaner import is_valid_isin

logger = logging.getLogger("mf.etl.nav_loader")


class NavLoader:
    """
    Enhanced NAV data loader with validation and error handling
    """
    
    def __init__(self):
        self.engine = engine
        self.stats = {
            'cams_loaded': 0,
            'kfin_loaded': 0,
            'amfi_loaded': 0,
            'cams_errors': 0,
            'kfin_errors': 0,
            'amfi_errors': 0,
            'invalid_isins': 0,
        }
    
    def validate_and_fix_isin(self, isin_value) -> str:
        """
        Validate and fix ISIN to ensure it's exactly 12 characters
        Returns None if invalid
        """
        if pd.isna(isin_value) or not isin_value:
            return None
        
        isin_str = str(isin_value).strip().upper()
        
        # Truncate to 12 characters if longer
        if len(isin_str) > 12:
            isin_str = isin_str[:12]
        
        # Validate format: IN + 10 alphanumeric
        if is_valid_isin(isin_str):
            return isin_str
        
        return None
    
    def prepare_raw_row_jsonb(self, row: pd.Series) -> str:
        """
        Convert DataFrame row to JSONB-compatible JSON string
        Handles NaN, datetime, and other non-JSON serializable types
        """
        try:
            # Convert to dict and handle special types
            row_dict = {}
            for key, value in row.items():
                if pd.isna(value):
                    row_dict[key] = None
                elif isinstance(value, (pd.Timestamp, pd.DatetimeTZDtype)):
                    row_dict[key] = value.isoformat() if not pd.isna(value) else None
                elif isinstance(value, (int, float)):
                    row_dict[key] = None if pd.isna(value) else float(value)
                else:
                    row_dict[key] = str(value)
            
            return json.dumps(row_dict)
        except Exception as e:
            logger.warning(f"Failed to create raw_row JSON: {e}")
            return json.dumps({"error": "Failed to serialize row"})
    
    # ================================================================
    # CAMS Historical NAV Loader
    # ================================================================
    
    def load_cams_historical(self, csv_files: List[Path]) -> int:
        """
        Load CAMS historical NAV CSV files with validation
        """
        if not csv_files:
            logger.warning("⚠️  No CAMS historical files to load")
            return 0
        
        logger.info(f"📥 Loading {len(csv_files)} CAMS historical NAV files...")
        total_rows = 0
        
        for csv_path in csv_files:
            try:
                df = pd.read_csv(csv_path, dtype=str)
                
                if df.empty:
                    logger.warning(f"⚠️  Empty file: {csv_path.name}")
                    continue
                
                logger.info(f"📋 Processing {csv_path.name}: {len(df)} rows")
                
                # Validate and fix ISINs
                if 'isin_no' in df.columns:
                    df['isin_no_validated'] = df['isin_no'].apply(self.validate_and_fix_isin)
                    invalid_count = df['isin_no_validated'].isna().sum()
                    if invalid_count > 0:
                        logger.warning(f"⚠️  {invalid_count} invalid ISINs found in {csv_path.name}")
                        self.stats['invalid_isins'] += invalid_count
                    df['isin_no'] = df['isin_no_validated']
                    df.drop('isin_no_validated', axis=1, inplace=True)
                
                # Add metadata
                df['source_file'] = str(csv_path)
                df['source'] = 'CAMS_HISTORICAL'
                df['imported_at'] = pd.Timestamp.now()
                
                # Create raw_row JSONB
                df['raw_row'] = df.apply(self.prepare_raw_row_jsonb, axis=1)
                
                # Convert data types
                if 'nav_date' in df.columns:
                    df['nav_date'] = pd.to_datetime(df['nav_date'], errors='coerce')
                else:
                    # Try to find date column with different name
                    date_like_cols = [c for c in df.columns if 'date' in c.lower()]
                    if date_like_cols:
                        logger.warning(f"⚠️  'nav_date' not found, trying: {date_like_cols[0]}")
                        df['nav_date'] = pd.to_datetime(df[date_like_cols[0]], errors='coerce')
                    else:
                        logger.error(f"❌ No date column found in {csv_path.name}")
                        continue
                
                # Ensure nav_value column exists
                if 'nav_value' not in df.columns:
                    # Try NAV or similar
                    nav_like_cols = [c for c in df.columns if c.lower() in ['nav', 'navvalue', 'nav_val']]
                    if nav_like_cols:
                        df['nav_value'] = df[nav_like_cols[0]]
                        logger.warning(f"⚠️  Using '{nav_like_cols[0]}' as nav_value")
                
                numeric_cols = ['nav_value', 'dividend_per_unit', 'corp_div_rate']
                for col in numeric_cols:
                    if col in df.columns:
                        df[col] = pd.to_numeric(df[col], errors='coerce')
                
                # Remove rows with no ISIN or no NAV date
                if 'isin_no' not in df.columns or 'nav_date' not in df.columns:
                    logger.error(f"❌ Missing required columns in {csv_path.name}")
                    continue
                    
                df_valid = df[df['isin_no'].notna() & df['nav_date'].notna()].copy()
                
                if len(df_valid) < len(df):
                    logger.warning(f"⚠️  Filtered out {len(df) - len(df_valid)} rows (missing ISIN or date)")
                
                if df_valid.empty:
                    logger.warning(f"⚠️  No valid rows in {csv_path.name}")
                    continue
                
                # Load to database (skip_amc_columns=True for NAV tables)
                load_dataframe_to_table(
                    df_valid,
                    self.engine,
                    table_name='cams_nav_historical_raw',
                    if_exists='append',
                    use_fast_copy=True,
                    source_file=str(csv_path),
                    skip_amc_columns=True  # NAV tables don't have AMC columns
                )
                
                total_rows += len(df_valid)
                self.stats['cams_loaded'] += len(df_valid)
                logger.info(f"✅ Loaded {len(df_valid)} valid rows from {csv_path.name}")
                
            except Exception as e:
                logger.exception(f"❌ Failed to load {csv_path.name}: {e}")
                self.stats['cams_errors'] += 1
                continue
        
        logger.info(f"🎉 CAMS Historical: Loaded {total_rows} total rows")
        return total_rows
    
    # ================================================================
    # KFIN Historical NAV Loader
    # ================================================================
    
    def load_kfin_historical(self, csv_files: List[Path]) -> int:
        """
        Load KFIN historical NAV CSV files with validation
        """
        if not csv_files:
            logger.warning("⚠️  No KFIN historical files to load")
            return 0
        
        logger.info(f"📥 Loading {len(csv_files)} KFIN historical NAV files...")
        total_rows = 0
        
        for csv_path in csv_files:
            try:
                df = pd.read_csv(csv_path, dtype=str)
                
                if df.empty:
                    logger.warning(f"⚠️  Empty file: {csv_path.name}")
                    continue
                
                logger.info(f"📋 Processing {csv_path.name}: {len(df)} rows")
                
                # Validate and fix ISINs
                if 'schemeisin' in df.columns:
                    df['schemeisin_validated'] = df['schemeisin'].apply(self.validate_and_fix_isin)
                    invalid_count = df['schemeisin_validated'].isna().sum()
                    if invalid_count > 0:
                        logger.warning(f"⚠️  {invalid_count} invalid ISINs found in {csv_path.name}")
                        self.stats['invalid_isins'] += invalid_count
                    df['schemeisin'] = df['schemeisin_validated']
                    df.drop('schemeisin_validated', axis=1, inplace=True)
                
                # Add metadata
                df['source_file'] = str(csv_path)
                df['source'] = 'KFIN_HISTORICAL'
                df['imported_at'] = pd.Timestamp.now()
                
                # Create raw_row JSONB
                df['raw_row'] = df.apply(self.prepare_raw_row_jsonb, axis=1)
                
                # Convert data types
                if 'navdate' in df.columns:
                    df['navdate'] = pd.to_datetime(df['navdate'], errors='coerce')
                if 'crdate' in df.columns:
                    df['crdate'] = pd.to_datetime(df['crdate'], errors='coerce')
                
                numeric_cols = ['nav', 'ppop', 'rpop']
                for col in numeric_cols:
                    if col in df.columns:
                        df[col] = pd.to_numeric(df[col], errors='coerce')
                
                # Remove rows with no ISIN or no NAV date
                df_valid = df[df['schemeisin'].notna() & df['navdate'].notna()].copy()
                
                if len(df_valid) < len(df):
                    logger.warning(f"⚠️  Filtered out {len(df) - len(df_valid)} rows (missing ISIN or date)")
                
                if df_valid.empty:
                    logger.warning(f"⚠️  No valid rows in {csv_path.name}")
                    continue
                
                # Load to database (skip_amc_columns=True for NAV tables)
                load_dataframe_to_table(
                    df_valid,
                    self.engine,
                    table_name='kfin_nav_historical_raw',
                    if_exists='append',
                    use_fast_copy=True,
                    source_file=str(csv_path),
                    skip_amc_columns=True  # NAV tables don't have AMC columns
                )
                
                total_rows += len(df_valid)
                self.stats['kfin_loaded'] += len(df_valid)
                logger.info(f"✅ Loaded {len(df_valid)} valid rows from {csv_path.name}")
                
            except Exception as e:
                logger.exception(f"❌ Failed to load {csv_path.name}: {e}")
                self.stats['kfin_errors'] += 1
                continue
        
        logger.info(f"🎉 KFIN Historical: Loaded {total_rows} total rows")
        return total_rows
    
    # ================================================================
    # AMFI Daily NAV Loader
    # ================================================================
    
    def load_amfi_daily(self, csv_path: Path) -> int:
        """
        Load AMFI daily NAV CSV with validation
        """
        if not csv_path or not csv_path.exists():
            logger.warning("⚠️  No AMFI daily file to load")
            return 0
        
        logger.info(f"📥 Loading AMFI daily NAV: {csv_path.name}")
        
        try:
            df = pd.read_csv(csv_path, dtype=str)
            
            if df.empty:
                logger.warning("⚠️  Empty AMFI daily file")
                return 0
            
            logger.info(f"📋 Processing {len(df)} AMFI NAV records")
            
            # Validate both ISIN columns
            for isin_col in ['isin_div_payout_growth', 'isin_div_reinvestment']:
                if isin_col in df.columns:
                    df[f'{isin_col}_validated'] = df[isin_col].apply(self.validate_and_fix_isin)
                    invalid_count = df[f'{isin_col}_validated'].isna().sum()
                    if invalid_count > 0:
                        logger.info(f"ℹ️  {invalid_count} invalid ISINs in {isin_col}")
                    df[isin_col] = df[f'{isin_col}_validated']
                    df.drop(f'{isin_col}_validated', axis=1, inplace=True)
            
            # Add metadata
            df['source_file'] = str(csv_path)
            df['source'] = 'AMFI_DAILY'
            df['imported_at'] = pd.Timestamp.now()
            
            # Create raw_row JSONB
            df['raw_row'] = df.apply(self.prepare_raw_row_jsonb, axis=1)
            
            # Convert data types
            if 'nav_date' in df.columns:
                df['nav_date'] = pd.to_datetime(df['nav_date'], errors='coerce')
            
            if 'net_asset_value' in df.columns:
                df['net_asset_value'] = pd.to_numeric(df['net_asset_value'], errors='coerce')
            
            # Filter: Must have at least one valid ISIN and valid date
            df_valid = df[
                (df['isin_div_payout_growth'].notna() | df['isin_div_reinvestment'].notna()) & 
                df['nav_date'].notna()
            ].copy()
            
            if len(df_valid) < len(df):
                logger.warning(f"⚠️  Filtered out {len(df) - len(df_valid)} rows (no valid ISIN or date)")
            
            if df_valid.empty:
                logger.warning("⚠️  No valid AMFI NAV records")
                return 0
            
            # Load to database (skip_amc_columns=True for NAV tables)
            load_dataframe_to_table(
                df_valid,
                self.engine,
                table_name='amfi_nav_daily_raw',
                if_exists='append',
                use_fast_copy=True,
                source_file=str(csv_path),
                skip_amc_columns=True  # NAV tables don't have AMC columns
            )
            
            self.stats['amfi_loaded'] = len(df_valid)
            logger.info(f"✅ AMFI Daily: Loaded {len(df_valid)} rows")
            return len(df_valid)
            
        except Exception as e:
            logger.exception(f"❌ Failed to load AMFI daily NAV: {e}")
            self.stats['amfi_errors'] += 1
            return 0
    
    # ================================================================
    # MAIN LOADER WITH COMPREHENSIVE REPORTING
    # ================================================================
    
    def load_all_nav_data(self, nav_results: Dict) -> Dict:
        """
        Load all NAV data with comprehensive error tracking
        Returns loading statistics
        """
        logger.info("🚀 Starting NAV Data Loading...")
        logger.info("=" * 60)
        
        # Reset stats
        self.stats = {k: 0 for k in self.stats.keys()}
        
        # Load CAMS Historical
        logger.info("\n📦 Loading CAMS Historical NAV...")
        self.load_cams_historical(nav_results.get('cams_historical', []))
        
        # Load KFIN Historical
        logger.info("\n📦 Loading KFIN Historical NAV...")
        self.load_kfin_historical(nav_results.get('kfin_historical', []))
        
        # Load AMFI Daily
        logger.info("\n📦 Loading AMFI Daily NAV...")
        self.load_amfi_daily(nav_results.get('amfi_daily'))
        
        # Generate summary report
        logger.info("\n" + "=" * 60)
        logger.info("📊 NAV LOADING SUMMARY")
        logger.info("=" * 60)
        logger.info(f"✅ CAMS Historical: {self.stats['cams_loaded']:,} rows loaded")
        logger.info(f"✅ KFIN Historical: {self.stats['kfin_loaded']:,} rows loaded")
        logger.info(f"✅ AMFI Daily: {self.stats['amfi_loaded']:,} rows loaded")
        
        total_loaded = self.stats['cams_loaded'] + self.stats['kfin_loaded'] + self.stats['amfi_loaded']
        logger.info(f"\n🎉 TOTAL NAV RECORDS LOADED: {total_loaded:,}")
        
        # Report errors if any
        total_errors = self.stats['cams_errors'] + self.stats['kfin_errors'] + self.stats['amfi_errors']
        if total_errors > 0:
            logger.warning("\n⚠️  ERRORS ENCOUNTERED:")
            if self.stats['cams_errors'] > 0:
                logger.warning(f"   CAMS: {self.stats['cams_errors']} file(s) failed")
            if self.stats['kfin_errors'] > 0:
                logger.warning(f"   KFIN: {self.stats['kfin_errors']} file(s) failed")
            if self.stats['amfi_errors'] > 0:
                logger.warning(f"   AMFI: {self.stats['amfi_errors']} file(s) failed")
        
        if self.stats['invalid_isins'] > 0:
            logger.warning(f"\n⚠️  {self.stats['invalid_isins']:,} invalid ISINs were filtered out")
        
        logger.info("=" * 60)
        
        return self.stats
    
    def get_raw_table_counts(self) -> Dict:
        """
        Query actual row counts from raw tables for verification
        """
        try:
            with self.engine.connect() as conn:
                counts = {}
                
                result = conn.execute(text("SELECT COUNT(*) FROM cams_nav_historical_raw"))
                counts['cams_historical_raw'] = result.scalar()
                
                result = conn.execute(text("SELECT COUNT(*) FROM kfin_nav_historical_raw"))
                counts['kfin_historical_raw'] = result.scalar()
                
                result = conn.execute(text("SELECT COUNT(*) FROM amfi_nav_daily_raw"))
                counts['amfi_daily_raw'] = result.scalar()
                
                logger.info("\n📋 RAW TABLE VERIFICATION:")
                for table, count in counts.items():
                    logger.info(f"   {table}: {count:,} rows")
                
                return counts
        except Exception as e:
            logger.exception(f"Failed to verify raw table counts: {e}")
            return {}