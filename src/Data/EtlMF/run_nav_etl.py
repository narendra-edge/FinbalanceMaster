# run_nav_etl.py (FIXED VERSION)
"""
Complete NAV + Dividend ETL Pipeline

Data Sources:
-------------
NAV:
  - CAMS Historical (ZIP → DBF → CSV)
  - KFIN Historical (ZIP → DBF → CSV)
  - AMFI Daily (TXT → CSV)

DIVIDEND:
  - CAMS Historical (ZIP → DBF → CSV) - same location as NAV
  - KFIN Historical (ZIP → DBF → CSV) - same location as NAV
  - CAMS Daily (Gmail → Excel download)
  - KFIN Daily (Gmail → Excel download)

Process:
--------
1. Extract NAV data
2. Extract Dividend data (Historical DBF + Daily Gmail)
3. Load all raw data into Postgres
4. Execute stored procedures (NAV + Dividend)
5. Generate comprehensive reports
"""

import logging
from datetime import datetime, timezone
from pathlib import Path

from Etl.utils import settings, ensure_dir
from Etl.nav_fetcher import NavFetcher
from Etl.nav_loader import NavLoader  # CHANGED: Use improved loader
from Etl.complete_dividend_fetcher import CompleteDividendFetcher  # FIXED: Correct import
from Etl.complete_dividend_loader import CompleteDividendLoader

logger = logging.getLogger("run_nav_etl")


def configure_logging(settings_dict: dict) -> Path:
    """Setup logging for complete NAV + Dividend ETL"""
    paths_cfg = settings_dict.get("paths", {})
    logs_dir = Path(paths_cfg.get("logs_dir", "C:/Data_MF/etl_logs"))
    logs_dir.mkdir(parents=True, exist_ok=True)
    
    ts = datetime.now(timezone.utc).strftime("%Y%m%d_%H%M%S")
    log_file = logs_dir / f"nav_dividend_complete_etl_{ts}.log"
    
    logging.basicConfig(
        level=logging.INFO,
        format="%(asctime)s [%(levelname)s] %(name)s - %(message)s",
        handlers=[
            logging.FileHandler(log_file, encoding="utf-8"),
            logging.StreamHandler(),
        ],
    )
    
    global logger
    logger = logging.getLogger("run_nav_etl")
    logger.info(f"📜 Complete NAV+Dividend ETL Logging to → {log_file}")
    return log_file


def main():
    """
    Main NAV + Dividend ETL Pipeline (Complete System)
    """
    
    # 1. Configure logging
    configure_logging(settings)
    logger.info("🚀 Starting COMPLETE NAV + DIVIDEND ETL PIPELINE")
    logger.info("=" * 80)
    logger.info("📊 NAV Sources: CAMS Historical, KFIN Historical, AMFI Daily")
    logger.info("💰 Dividend Sources: CAMS Historical (DBF), KFIN Historical (DBF),")
    logger.info("                     CAMS Daily (Gmail), KFIN Daily (Gmail)")
    logger.info("=" * 80)
    
    # 2. Build paths configuration
    paths_cfg = settings.get("paths", {})
    nav_cfg = settings.get("nav", {})
    dividend_cfg = settings.get("dividend", {})
    
    paths = {
        "downloads_dir": paths_cfg.get("downloads_dir", "C:/Data_MF/downloads"),
        "nav_output_dir": paths_cfg.get("nav_output_dir", "C:/Data_MF/downloads/nav"),
        "dividend_output_dir": paths_cfg.get("dividend_output_dir", "C:/Data_MF/downloads/dividend"),
        "cams_dividend_dir": paths_cfg.get("cams_dividend_dir", "C:/Data_MF/downloads/dividend/cams"),
        "kfin_dividend_dir": paths_cfg.get("kfin_dividend_dir", "C:/Data_MF/downloads/dividend/kfin"),
        "archive_dir": paths_cfg.get("archive_dir", "C:/Data_MF/archive"),
    }
    
    ensure_dir(paths["nav_output_dir"])
    ensure_dir(paths["dividend_output_dir"])
    ensure_dir(paths["cams_dividend_dir"])
    ensure_dir(paths["kfin_dividend_dir"])
    
    # Update settings with NAV+Dividend specific paths
    unified_config = {
        **settings,
        # NAV paths
        "cams_nav_historical_dir": nav_cfg.get("cams_historical_dir", 
                                               "C:/Data_MF/Cams_Data_HistoricalNav"),
        "kfin_nav_historical_dir": nav_cfg.get("kfin_historical_dir",
                                               "C:/Data_MF/Kfin_Data_HistoricalNav"),
        "cams_zip_password": nav_cfg.get("cams_zip_password", 
                                        settings.get("gmail", {}).get("zip_password")),
        "kfin_zip_password": nav_cfg.get("kfin_zip_password", None),
        "amfi_nav_url": nav_cfg.get("amfi_daily_url",
                                    "https://portal.amfiindia.com/spages/NAVAll.txt"),
    }
    
    # Check processing flags
    process_dividends = dividend_cfg.get("process_dividends", True)
    process_div_historical = dividend_cfg.get("process_historical", True)
    process_div_daily = dividend_cfg.get("process_daily", True)
    
    # ========================================================================
    # PHASE 1: EXTRACT NAV DATA
    # ========================================================================
    logger.info("\n" + "="*80)
    logger.info("PHASE 1: NAV DATA EXTRACTION")
    logger.info("="*80)
    
    nav_fetcher = NavFetcher(unified_config, paths)
    nav_results = nav_fetcher.fetch_all_nav_data()
    
    logger.info("\n📊 NAV Extraction Summary:")
    logger.info(f"   CAMS Historical: {len(nav_results['cams_historical'])} files")
    logger.info(f"   KFIN Historical: {len(nav_results['kfin_historical'])} files")
    logger.info(f"   AMFI Daily: {'✅ Downloaded' if nav_results['amfi_daily'] else '❌ Failed'}")
    
    # ========================================================================
    # PHASE 2: EXTRACT DIVIDEND DATA (Historical DBF + Daily Gmail)
    # ========================================================================
    dividend_results = {
        'cams_historical': [],
        'kfin_historical': [],
        'cams_daily': [],
        'kfin_daily': [],
    }
    
    if process_dividends:
        logger.info("\n" + "="*80)
        logger.info("PHASE 2: DIVIDEND DATA EXTRACTION")
        logger.info("="*80)
        logger.info("📊 Two Sources: Historical (DBF from ZIPs) + Daily (Gmail Excel)")
        
        try:
            complete_div_fetcher = CompleteDividendFetcher(unified_config, paths)
            
            # This fetches BOTH historical and daily
            dividend_results = complete_div_fetcher.fetch_all_dividend_data()
            
            logger.info("\n💰 Dividend Extraction Summary:")
            logger.info(f"   CAMS Historical (DBF): {len(dividend_results['cams_historical'])} files")
            logger.info(f"   KFIN Historical (DBF): {len(dividend_results['kfin_historical'])} files")
            logger.info(f"   CAMS Daily (Gmail): {len(dividend_results['cams_daily'])} Excel files")
            logger.info(f"   KFIN Daily (Gmail): {len(dividend_results['kfin_daily'])} Excel files")
            
            total_div_files = (len(dividend_results['cams_historical']) + 
                              len(dividend_results['kfin_historical']) +
                              len(dividend_results['cams_daily']) +
                              len(dividend_results['kfin_daily']))
            
            if total_div_files == 0:
                logger.warning("⚠️  No dividend files extracted from any source")
                logger.warning("   Check:")
                logger.warning("   1. ZIP files exist in historical directories")
                logger.warning("   2. Gmail emails exist matching configured criteria")
                logger.warning("   3. Passwords are correct")
        
        except Exception as e:
            logger.exception(f"❌ Dividend extraction failed: {e}")
            logger.warning("⚠️  Continuing without dividend data...")
    else:
        logger.info("\n⚠️  Dividend processing is DISABLED in config")
        logger.info("   Set dividend.process_dividends: true to enable")
    
    # ========================================================================
    # PHASE 3: LOAD RAW DATA INTO POSTGRES
    # ========================================================================
    logger.info("\n" + "="*80)
    logger.info("PHASE 3: LOADING RAW DATA TO POSTGRES")
    logger.info("="*80)
    
    # Load NAV data using IMPROVED loader
    logger.info("📥 Loading NAV data with validation...")
    nav_loader = NavLoader()  # Use improved version
    nav_stats = nav_loader.load_all_nav_data(nav_results)
    
    # Verify raw table counts
    nav_loader.get_raw_table_counts()
    
    # Load Dividend data (all 4 sources)
    dividend_row_counts = {}
    if process_dividends:
        logger.info("\n📥 Loading Dividend data (Historical DBF + Daily Gmail)...")
        complete_div_loader = CompleteDividendLoader()
        dividend_row_counts = complete_div_loader.load_all_dividend_data(dividend_results)
        
        total_div_rows = sum(dividend_row_counts.values())
        if total_div_rows == 0:
            logger.warning("⚠️  No dividend rows loaded - check extraction phase")
    
    # ========================================================================
    # PHASE 4: EXECUTE STORED PROCEDURES
    # ========================================================================
    logger.info("\n" + "="*80)
    logger.info("PHASE 4: EXECUTING STORED PROCEDURES (SET-BASED)")
    logger.info("="*80)
    logger.info("⚡ Using optimized batch processing (1000x faster than cursors)")
    
    from sqlalchemy import text
    
    try:
        with nav_loader.engine.begin() as conn:
            # 1. Refresh NAV Master
            logger.info("1️⃣  Refreshing NAV Master (batch mode)...")
            conn.execute(text("CALL sp_refresh_nav_master()"))
            logger.info("✅ NAV Master refresh complete")
            
            # 2. Refresh Dividend Master (if dividend data loaded)
            if process_dividends and sum(dividend_row_counts.values()) > 0:
                logger.info("2️⃣  Refreshing Dividend Master (batch mode)...")
                conn.execute(text("CALL sp_refresh_dividend_master()"))
                logger.info("✅ Dividend Master refresh complete")
            else:
                logger.info("2️⃣  Skipping Dividend Master (no data loaded)")
            
            # 3. Update NAV Statistics
            logger.info("3️⃣  Updating NAV Statistics...")
            conn.execute(text("CALL sp_update_nav_statistics()"))
            logger.info("✅ NAV Statistics update complete")
            
            # 4. Identify Unmapped AMFI Schemes
            logger.info("4️⃣  Identifying unmapped AMFI schemes...")
            conn.execute(text("CALL sp_identify_unmapped_amfi_schemes()"))
            logger.info("✅ Unmapped schemes identification complete")
        
        logger.info("✅ All stored procedures completed successfully!")
        
    except Exception as e:
        logger.exception(f"❌ Stored procedure execution failed: {e}")
        raise
    
    # ========================================================================
    # PHASE 5: GENERATE REPORTS
    # ========================================================================
    logger.info("\n" + "="*80)
    logger.info("PHASE 5: GENERATING COMPREHENSIVE REPORTS")
    logger.info("="*80)
    
    generate_comprehensive_reports(nav_loader, process_dividends, dividend_row_counts)
    
    # ========================================================================
    # COMPLETION
    # ========================================================================
    logger.info("\n" + "="*80)
    logger.info("🎉 COMPLETE NAV + DIVIDEND ETL PIPELINE FINISHED SUCCESSFULLY!")
    logger.info("="*80)
    
    print_next_steps(process_dividends, dividend_results, dividend_row_counts)


def generate_comprehensive_reports(loader: NavLoader, include_dividends: bool, div_counts: dict):
    """Generate comprehensive NAV + Dividend coverage reports"""
    from sqlalchemy import text
    import pandas as pd
    
    logger.info("📊 Generating Comprehensive Reports...")
    
    try:
        # =====================================================================
        # REPORT 1: Overall NAV Coverage
        # =====================================================================
        query_coverage = """
        SELECT 
            COUNT(DISTINCT scheme_id) as total_schemes_with_nav,
            COUNT(*) as total_nav_records,
            MIN(nav_date) as earliest_nav_date,
            MAX(nav_date) as latest_nav_date
        FROM nav_master
        WHERE scheme_id IS NOT NULL
        """
        
        df_coverage = pd.read_sql(query_coverage, loader.engine)
        logger.info("\n📈 Overall NAV Coverage:")
        logger.info(f"   Schemes with NAV: {df_coverage['total_schemes_with_nav'].iloc[0]:,}")
        logger.info(f"   Total NAV records: {df_coverage['total_nav_records'].iloc[0]:,}")
        logger.info(f"   Date range: {df_coverage['earliest_nav_date'].iloc[0]} to {df_coverage['latest_nav_date'].iloc[0]}")
        
        # =====================================================================
        # REPORT 2: NAV by RTA Source
        # =====================================================================
        query_by_source = """
        SELECT 
            rta_source,
            COUNT(DISTINCT scheme_id) as schemes,
            COUNT(*) as nav_records
        FROM nav_master
        WHERE scheme_id IS NOT NULL
        GROUP BY rta_source
        ORDER BY nav_records DESC
        """
        
        df_by_source = pd.read_sql(query_by_source, loader.engine)
        logger.info("\n📊 NAV by RTA Source:")
        for _, row in df_by_source.iterrows():
            logger.info(f"   {row['rta_source']}: {row['schemes']:,} schemes, {row['nav_records']:,} records")
        
        # =====================================================================
        # REPORT 3: Dividend Coverage (if enabled)
        # =====================================================================
        if include_dividends and sum(div_counts.values()) > 0:
            query_div_coverage = """
            SELECT 
                data_type,
                rta_source,
                COUNT(DISTINCT scheme_id) as schemes,
                COUNT(*) as dividend_records,
                MIN(ex_dividend_date) as earliest_date,
                MAX(ex_dividend_date) as latest_date
            FROM dividend_master
            WHERE scheme_id IS NOT NULL
            GROUP BY data_type, rta_source
            ORDER BY data_type, rta_source
            """
            
            try:
                df_div = pd.read_sql(query_div_coverage, loader.engine)
                if not df_div.empty:
                    logger.info("\n💰 Dividend Coverage by Source:")
                    for _, row in df_div.iterrows():
                        logger.info(f"   {row['data_type']} - {row['rta_source']}: "
                                  f"{row['schemes']:,} schemes, {row['dividend_records']:,} records")
                        logger.info(f"      Date range: {row['earliest_date']} to {row['latest_date']}")
                    
                    # Overall dividend summary
                    query_div_total = """
                    SELECT 
                        COUNT(DISTINCT scheme_id) as total_schemes,
                        COUNT(*) as total_records
                    FROM dividend_master
                    WHERE scheme_id IS NOT NULL
                    """
                    df_div_total = pd.read_sql(query_div_total, loader.engine)
                    logger.info(f"\n   TOTAL: {df_div_total['total_schemes'].iloc[0]:,} schemes, "
                              f"{df_div_total['total_records'].iloc[0]:,} dividend records")
                    
                    # Dividend vs Bonus breakdown
                    query_div_types = """
                    SELECT 
                        dividend_bonus_flag,
                        COUNT(*) as count
                    FROM dividend_master
                    GROUP BY dividend_bonus_flag
                    """
                    df_div_types = pd.read_sql(query_div_types, loader.engine)
                    logger.info("\n💎 Dividend Type Breakdown:")
                    for _, row in df_div_types.iterrows():
                        logger.info(f"   {row['dividend_bonus_flag']}: {row['count']:,} records")
                else:
                    logger.warning("⚠️  No dividend data found in dividend_master table")
            except Exception as e:
                logger.warning(f"⚠️  Could not query dividend data: {e}")
        
        # =====================================================================
        # REPORT 4: Unmapped AMFI Schemes
        # =====================================================================
        query_unmapped = """
        SELECT 
            status,
            COUNT(*) as count
        FROM unmapped_nav_schemes
        GROUP BY status
        ORDER BY count DESC
        """
        
        df_unmapped = pd.read_sql(query_unmapped, loader.engine)
        logger.info("\n📋 Unmapped NAV Schemes by Status:")
        for _, row in df_unmapped.iterrows():
            logger.info(f"   {row['status']}: {row['count']:,} schemes")
        
        # =====================================================================
        # REPORT 5: scheme_master_final Update Status
        # =====================================================================
        query_master = """
        SELECT 
            COUNT(*) as total_schemes,
            COUNT(latest_nav) FILTER (WHERE latest_nav IS NOT NULL) as with_nav,
            COUNT(latest_nav) FILTER (WHERE latest_nav IS NULL) as without_nav
        FROM scheme_master_final
        """
        
        df_master = pd.read_sql(query_master, loader.engine)
        logger.info("\n🎯 Scheme Master Final Update:")
        logger.info(f"   Total schemes: {df_master['total_schemes'].iloc[0]:,}")
        logger.info(f"   With NAV: {df_master['with_nav'].iloc[0]:,}")
        logger.info(f"   Without NAV: {df_master['without_nav'].iloc[0]:,}")
        
        # Export reports
        export_unmapped_report(loader)
        
    except Exception as e:
        logger.exception(f"❌ Failed to generate reports: {e}")


def export_unmapped_report(loader: NavLoader):
    """Export detailed unmapped schemes to CSV"""
    try:
        reports_dir = Path("C:/Data_MF/reports")
        reports_dir.mkdir(parents=True, exist_ok=True)
        
        query = """
        SELECT 
            isin, scheme_code, scheme_name, rta_source,
            first_nav_date, last_nav_date, nav_count, status
        FROM unmapped_nav_schemes
        WHERE status IN ('UNMAPPED', 'NO_NAV_DATA', 'AMFI_ONLY')
        ORDER BY status, nav_count DESC
        """
        
        df = pd.read_sql(query, loader.engine)
        
        if not df.empty:
            output_path = reports_dir / "unmapped_nav_schemes.csv"
            df.to_csv(output_path, index=False)
            logger.info(f"\n📄 Unmapped schemes report → {output_path}")
            logger.info(f"   Total unmapped: {len(df):,}")
            
            # AMFI schemes without NAV
            df_no_nav = df[df['status'] == 'NO_NAV_DATA']
            if not df_no_nav.empty:
                no_nav_path = reports_dir / "amfi_schemes_without_nav.csv"
                df_no_nav.to_csv(no_nav_path, index=False)
                logger.info(f"📄 AMFI schemes without NAV → {no_nav_path}")
                logger.info(f"   Count: {len(df_no_nav):,}")
        
    except Exception as e:
        logger.exception(f"❌ Failed to export unmapped report: {e}")


def print_next_steps(dividends_processed: bool, dividend_results: dict, div_counts: dict):
    """Print next steps for user"""
    logger.info("\n📋 Next Steps:")
    logger.info("   1. Review nav_master and nav_statistics tables")
    logger.info("   2. Check scheme_master_final.latest_nav is populated")
    logger.info("   3. Review unmapped_nav_schemes (~4000 AMFI schemes)")
    logger.info("   4. Check reports in C:/Data_MF/reports/")
    
    if dividends_processed:
        total_div_files = sum(len(v) if isinstance(v, list) else 0 for v in dividend_results.values())
        total_div_rows = sum(div_counts.values()) if div_counts else 0
        
        if total_div_rows > 0:
            logger.info(f"   5. ✅ Dividend data loaded: {total_div_rows:,} rows from {total_div_files} files")
            logger.info("      - Check dividend_master table")
            logger.info("      - Query: SELECT dividend_bonus_flag, COUNT(*) FROM dividend_master GROUP BY dividend_bonus_flag;")
        else:
            logger.info("   5. ⚠️  No dividend data loaded")
            logger.info("      Reasons:")
            logger.info("      - No DBF files found in historical directories")
            logger.info("      - No Gmail emails matching criteria")
            logger.info("      - Check extraction logs above for details")
    else:
        logger.info("   5. ⚠️  Dividend processing was DISABLED")
        logger.info("      To enable: Set dividend.process_dividends: true in settings.yaml")


if __name__ == "__main__":
    main()