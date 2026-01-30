# Etl/complete_dividend_loader.py
"""
Complete Dividend Loader
-------------------------
Loads dividend data from ALL sources:
1. CAMS Historical (DBF → CSV)
2. KFIN Historical (DBF → CSV)
3. CAMS Daily (Gmail Excel)
4. KFIN Daily (Gmail Excel)

Into tables:
- cams_dividend_historical_raw
- kfin_dividend_historical_raw
- cams_dividend_daily_raw
- kfin_dividend_daily_raw
"""

import logging
from pathlib import Path
from typing import List
import pandas as pd

from Etl.utils import engine
from Etl.excel_to_postgres import load_dataframe_to_table, safe_read_excel

logger = logging.getLogger("mf.etl.complete_dividend_loader")


class CompleteDividendLoader:
    """
    Loads dividend data from all sources into PostgreSQL
    """
    
    def __init__(self):
        self.engine = engine
    
    # ================================================================
    # 1. CAMS HISTORICAL (DBF → CSV)
    # ================================================================
    
    def load_cams_historical(self, csv_files: List[Path]) -> int:
        """Load CAMS historical dividend CSV files"""
        if not csv_files:
            logger.warning("⚠️  No CAMS historical dividend files")
            return 0
        
        logger.info(f"📥 Loading {len(csv_files)} CAMS historical dividend files...")
        total_rows = 0
        
        for csv_path in csv_files:
            try:
                df = pd.read_csv(csv_path, dtype=str)
                
                if df.empty:
                    continue
                
                # Add metadata
                df['source_file'] = str(csv_path)
                df['source'] = 'CAMS_DIV_HISTORICAL'
                
                # Convert data types
                date_cols = ['ex_dividend_date', 'record_date']
                for col in date_cols:
                    if col in df.columns:
                        df[col] = pd.to_datetime(df[col], errors='coerce')
                
                numeric_cols = ['dividend_rate_per_unit', 'corp_div_rate']
                for col in numeric_cols:
                    if col in df.columns:
                        df[col] = pd.to_numeric(df[col], errors='coerce')
                
                # Load to database
                load_dataframe_to_table(
                    df, self.engine,
                    table_name='cams_dividend_historical_raw',
                    if_exists='append',
                    use_fast_copy=True,
                    source_file=str(csv_path),
                     skip_amc_columns=True
                )
                
                total_rows += len(df)
                logger.info(f"✅ Loaded {len(df)} rows from {csv_path.name}")
                
            except Exception as e:
                logger.exception(f"❌ Failed to load {csv_path.name}: {e}")
        
        logger.info(f"🎉 CAMS Historical: {total_rows} total rows")
        return total_rows
    
    # ================================================================
    # 2. KFIN HISTORICAL (DBF → CSV)
    # ================================================================
    
    def load_kfin_historical(self, csv_files: List[Path]) -> int:
        """Load KFIN historical dividend CSV files"""
        if not csv_files:
            logger.warning("⚠️  No KFIN historical dividend files")
            return 0
        
        logger.info(f"📥 Loading {len(csv_files)} KFIN historical dividend files...")
        total_rows = 0
        
        for csv_path in csv_files:
            try:
                df = pd.read_csv(csv_path, dtype=str)
                
                if df.empty:
                    continue
                
                # Add metadata
                df['source_file'] = str(csv_path)
                df['source'] = 'KFIN_DIV_HISTORICAL'
                
                # Convert data types
                if 'div_date' in df.columns:
                    df['div_date'] = pd.to_datetime(df['div_date'], errors='coerce')
                
                numeric_cols = ['nday', 'drate', 'dnav']
                for col in numeric_cols:
                    if col in df.columns:
                        df[col] = pd.to_numeric(df[col], errors='coerce')
                
                # Load to database
                load_dataframe_to_table(
                    df, self.engine,
                    table_name='kfin_dividend_historical_raw',
                    if_exists='append',
                    use_fast_copy=True,
                    source_file=str(csv_path),
                    skip_amc_columns=True
                )
                
                total_rows += len(df)
                logger.info(f"✅ Loaded {len(df)} rows from {csv_path.name}")
                
            except Exception as e:
                logger.exception(f"❌ Failed to load {csv_path.name}: {e}")
        
        logger.info(f"🎉 KFIN Historical: {total_rows} total rows")
        return total_rows
    
    # ================================================================
    # 3. CAMS DAILY (Gmail Excel)
    # ================================================================
    
    def load_cams_daily(self, excel_files: List[Path]) -> int:
        """Load CAMS daily dividend Excel files from Gmail"""
        if not excel_files:
            logger.warning("⚠️  No CAMS daily dividend files")
            return 0
        
        logger.info(f"📥 Loading {len(excel_files)} CAMS daily dividend files...")
        total_rows = 0
        
        for excel_path in excel_files:
            try:
                df = safe_read_excel(str(excel_path))
                
                if df.empty:
                    continue
                
                logger.info(f"📋 Columns in {excel_path.name}: {list(df.columns)}")
                
                # Add metadata
                df['source_file'] = str(excel_path)
                df['source'] = 'CAMS_DIV_DAILY'
                
                # Column mapping (adjust based on actual Excel)
                column_mapping = {
                    'Product Code': 'product_code',
                    'Scheme Name': 'scheme_name',
                    'ISIN': 'isin',
                    'Ex-Dividend Date': 'ex_dividend_date',
                    'Ex Dividend Date': 'ex_dividend_date',
                    'Record Date': 'record_date',
                    'Payment Date': 'payment_date',
                    'Dividend Rate': 'dividend_rate_per_unit',
                    'Dividend Per Unit': 'dividend_rate_per_unit',
                    'Type': 'dividend_bonus_flag',
                    'Flag': 'dividend_bonus_flag',
                    'Bonus Ratio': 'bonus_ratio',
                    'Corp Rate': 'corp_div_rate',
                    'Plan': 'plan_type',
                    'Option': 'option_type',
                }
                
                df.rename(columns=column_mapping, inplace=True)
                
                # Convert dates
                date_cols = ['ex_dividend_date', 'record_date', 'payment_date']
                for col in date_cols:
                    if col in df.columns:
                        df[col] = pd.to_datetime(df[col], errors='coerce')
                
                # Convert numeric
                numeric_cols = ['dividend_rate_per_unit', 'corp_div_rate']
                for col in numeric_cols:
                    if col in df.columns:
                        df[col] = pd.to_numeric(df[col], errors='coerce')
                
                # Load to database
                load_dataframe_to_table(
                    df, self.engine,
                    table_name='cams_dividend_daily_raw',
                    if_exists='append',
                    use_fast_copy=True,
                    source_file=str(excel_path)
                )
                
                total_rows += len(df)
                logger.info(f"✅ Loaded {len(df)} rows from {excel_path.name}")
                
            except Exception as e:
                logger.exception(f"❌ Failed to load {excel_path.name}: {e}")
        
        logger.info(f"🎉 CAMS Daily: {total_rows} total rows")
        return total_rows
    
    # ================================================================
    # 4. KFIN DAILY (Gmail Excel)
    # ================================================================
    
    def load_kfin_daily(self, excel_files: List[Path]) -> int:
        """Load KFIN daily dividend Excel files from Gmail"""
        if not excel_files:
            logger.warning("⚠️  No KFIN daily dividend files")
            return 0
        
        logger.info(f"📥 Loading {len(excel_files)} KFIN daily dividend files...")
        total_rows = 0
        
        for excel_path in excel_files:
            try:
                df = safe_read_excel(str(excel_path))
                
                if df.empty:
                    continue
                
                logger.info(f"📋 Columns in {excel_path.name}: {list(df.columns)}")
                
                # Add metadata
                df['source_file'] = str(excel_path)
                df['source'] = 'KFIN_DIV_DAILY'
                
                # Column mapping (adjust based on actual Excel)
                column_mapping = {
                    'AMC': 'fund',
                    'Fund': 'fund',
                    'Scheme Code': 'scheme',
                    'Scheme': 'scheme',
                    'Scheme Name': 'scheme_name',
                    'ISIN': 'isin',
                    'Dividend Date': 'div_date',
                    'Div Date': 'div_date',
                    'Ex-Dividend Date': 'ex_dividend_date',
                    'Ex Date': 'ex_dividend_date',
                    'Record Date': 'record_date',
                    'Payment Date': 'payment_date',
                    'Dividend Rate': 'drate',
                    'Rate': 'drate',
                    'Dividend Amount': 'dividend_amount',
                    'Amount': 'dividend_amount',
                    'Ex-Div NAV': 'dnav',
                    'NAV': 'dnav',
                    'Status': 'status',
                    'Plan': 'plan_type',
                    'Option': 'option_type',
                }
                
                df.rename(columns=column_mapping, inplace=True)
                
                # Convert dates
                date_cols = ['div_date', 'ex_dividend_date', 'record_date', 'payment_date']
                for col in date_cols:
                    if col in df.columns:
                        df[col] = pd.to_datetime(df[col], errors='coerce')
                
                # Convert numeric
                numeric_cols = ['drate', 'dividend_amount', 'dnav']
                for col in numeric_cols:
                    if col in df.columns:
                        df[col] = pd.to_numeric(df[col], errors='coerce')
                
                # Load to database
                load_dataframe_to_table(
                    df, self.engine,
                    table_name='kfin_dividend_daily_raw',
                    if_exists='append',
                    use_fast_copy=True,
                    source_file=str(excel_path)
                )
                
                total_rows += len(df)
                logger.info(f"✅ Loaded {len(df)} rows from {excel_path.name}")
                
            except Exception as e:
                logger.exception(f"❌ Failed to load {excel_path.name}: {e}")
        
        logger.info(f"🎉 KFIN Daily: {total_rows} total rows")
        return total_rows
    
    # ================================================================
    # MAIN LOADER
    # ================================================================
    
    def load_all_dividend_data(self, fetch_results: dict) -> dict:
        """
        Load ALL dividend data from fetcher results
        
        Args:
            fetch_results: dict from CompleteDividendFetcher.fetch_all_dividend_data()
            
        Returns:
            dict with row counts per source
        """
        logger.info("🚀 Starting Complete Dividend Data Loading...")
        logger.info("=" * 60)
        
        row_counts = {
            'cams_historical': 0,
            'kfin_historical': 0,
            'cams_daily': 0,
            'kfin_daily': 0,
        }
        
        # Load all sources
        row_counts['cams_historical'] = self.load_cams_historical(
            fetch_results.get('cams_historical', [])
        )
        
        row_counts['kfin_historical'] = self.load_kfin_historical(
            fetch_results.get('kfin_historical', [])
        )
        
        row_counts['cams_daily'] = self.load_cams_daily(
            fetch_results.get('cams_daily', [])
        )
        
        row_counts['kfin_daily'] = self.load_kfin_daily(
            fetch_results.get('kfin_daily', [])
        )
        
        # Summary
        total_rows = sum(row_counts.values())
        
        logger.info("\n" + "=" * 60)
        logger.info("🎉 Complete Dividend Loading Summary:")
        logger.info(f"   CAMS Historical: {row_counts['cams_historical']:,} rows")
        logger.info(f"   KFIN Historical: {row_counts['kfin_historical']:,} rows")
        logger.info(f"   CAMS Daily: {row_counts['cams_daily']:,} rows")
        logger.info(f"   KFIN Daily: {row_counts['kfin_daily']:,} rows")
        logger.info(f"   TOTAL: {total_rows:,} rows")
        logger.info("=" * 60)
        
        return row_counts
