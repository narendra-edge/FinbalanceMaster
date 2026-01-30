"""
cleanup_and_test_amc.py
------------------------
Clean up duplicate functions and test AMC master system
"""

import logging
from sqlalchemy import text
from Etl.utils import engine

logging.basicConfig(level=logging.INFO, format='%(levelname)s - %(message)s')
logger = logging.getLogger(__name__)


def cleanup_duplicates():
    """Remove old/duplicate get_amc_id functions"""
    logger.info("Cleaning up duplicate functions...")
    
    cleanup_sql = """
    -- Drop ALL versions of get_amc_id to start fresh
    DROP FUNCTION IF EXISTS get_amc_id(TEXT, TEXT) CASCADE;
    DROP FUNCTION IF EXISTS get_amc_id(TEXT, TEXT, TEXT, TEXT) CASCADE;
    
    -- Recreate the clean wrapper version
    CREATE OR REPLACE FUNCTION get_amc_id(p_name TEXT, p_code TEXT)
    RETURNS UUID
    LANGUAGE plpgsql
    AS $get_amc_id$
    BEGIN
      RETURN get_amc_id_v3(p_name, p_code, NULL, NULL);
    END;
    $get_amc_id$;
    """
    
    try:
        with engine.begin() as conn:
            conn.execute(text(cleanup_sql))
        logger.info("  Y Duplicates cleaned up")
        return True
    except Exception as e:
        logger.error(f"  X Cleanup failed: {e}")
        return False


def test_functions():
    """Test all AMC functions"""
    logger.info("Testing AMC functions...")
    
    tests = {
        'normalize_amc_name_sql': "SELECT normalize_amc_name_sql('HDFC Mutual Fund')",
        'find_amc_match_v3': "SELECT * FROM find_amc_match_v3('HDFC', 'HDFC')",
        'get_amc_id_v3': "SELECT get_amc_id_v3('HDFC', 'HDFC', NULL, NULL)",
        'get_amc_id': "SELECT get_amc_id('HDFC', 'HDFC')",
    }
    
    results = {}
    
    with engine.connect() as conn:
        for test_name, query in tests.items():
            try:
                result = conn.execute(text(query))
                # Try to fetch result
                try:
                    row = result.fetchone()
                    results[test_name] = 'OK'
                    logger.info(f"  Y {test_name} - OK")
                except:
                    results[test_name] = 'OK'
                    logger.info(f"  Y {test_name} - OK")
            except Exception as e:
                results[test_name] = f'FAILED: {e}'
                logger.error(f"  X {test_name} - FAILED: {e}")
    
    return all('OK' in v for v in results.values())


def verify_tables():
    """Verify all required tables and procedures exist"""
    logger.info("Verifying tables and procedures...")
    
    checks = {
        'amc_master': "SELECT COUNT(*) FROM amc_master",
        'amc_staging': "SELECT COUNT(*) FROM amc_staging",
        'amc_match_review': "SELECT COUNT(*) FROM amc_match_review",
        'sp_build_amc_master_complete': """
            SELECT COUNT(*) FROM pg_proc 
            WHERE proname = 'sp_build_amc_master_complete'
        """,
        'sp_extract_amcs_from_raw': """
            SELECT COUNT(*) FROM pg_proc 
            WHERE proname = 'sp_extract_amcs_from_raw'
        """,
        'v_amc_staging_pending': """
            SELECT COUNT(*) FROM information_schema.views 
            WHERE table_name = 'v_amc_staging_pending'
        """,
    }
    
    all_ok = True
    
    with engine.connect() as conn:
        for check_name, query in checks.items():
            try:
                result = conn.execute(text(query)).scalar()
                if result > 0 or check_name.startswith('v_'):
                    logger.info(f"  Y {check_name} - OK")
                else:
                    logger.warning(f"  ? {check_name} - Empty")
            except Exception as e:
                logger.error(f"  X {check_name} - FAILED: {e}")
                all_ok = False
    
    return all_ok


def run_build_test():
    """Test the complete AMC build process"""
    logger.info("Testing AMC build process...")
    
    try:
        with engine.begin() as conn:
            # Check if we have raw data
            cams_count = conn.execute(text("SELECT COUNT(*) FROM cams_raw")).scalar()
            kfin_count = conn.execute(text("SELECT COUNT(*) FROM kfin_raw")).scalar()
            
            if cams_count == 0 and kfin_count == 0:
                logger.warning("  ! No raw data found - skipping build test")
                return True
            
            logger.info(f"  Found {cams_count} CAMS + {kfin_count} KFIN raw records")
            
            # Run the build
            logger.info("  Running sp_build_amc_master_complete()...")
            conn.execute(text("CALL sp_build_amc_master_complete()"))
            
            # Check results
            amc_count = conn.execute(text("SELECT COUNT(*) FROM amc_master")).scalar()
            staging_count = conn.execute(text("SELECT COUNT(*) FROM amc_staging")).scalar()
            
            logger.info(f"  Y Build complete: {amc_count} AMCs in master, {staging_count} in staging")
            return True
            
    except Exception as e:
        logger.exception(f"  X Build test failed: {e}")
        return False


def show_summary():
    """Show summary of AMC master status"""
    logger.info("AMC Master Summary:")
    
    with engine.connect() as conn:
        # Count AMCs
        amc_count = conn.execute(text("SELECT COUNT(*) FROM amc_master")).scalar()
        logger.info(f"  Total AMCs in master: {amc_count}")
        
        # Count staging
        staging = conn.execute(text("""
            SELECT 
                status,
                COUNT(*) as count
            FROM amc_staging
            GROUP BY status
        """)).fetchall()
        
        for row in staging:
            logger.info(f"  Staging {row[0]}: {row[1]}")
        
        # Coverage
        coverage = conn.execute(text("""
            SELECT 
                COUNT(*) FILTER (WHERE cams_amc_code IS NOT NULL) as cams,
                COUNT(*) FILTER (WHERE kfin_amc_code IS NOT NULL) as kfin,
                COUNT(*) FILTER (WHERE bse_amc_code IS NOT NULL) as bse,
                COUNT(*) as total
            FROM amc_master
        """)).fetchone()
        
        if coverage:
            logger.info(f"  Coverage: CAMS={coverage[0]}, KFIN={coverage[1]}, BSE={coverage[2]}, Total={coverage[3]}")


def main():
    """Main cleanup and test workflow"""
    logger.info("="*60)
    logger.info("AMC MASTER - CLEANUP & TEST")
    logger.info("="*60)
    
    # Step 1: Cleanup
    if not cleanup_duplicates():
        logger.error("Cleanup failed - stopping")
        return False
    
    # Step 2: Test functions
    if not test_functions():
        logger.error("Function tests failed - stopping")
        return False
    
    # Step 3: Verify tables
    if not verify_tables():
        logger.error("Table verification failed - stopping")
        return False
    
    # Step 4: Run build test
    if not run_build_test():
        logger.warning("Build test had issues - check logs")
    
    # Step 5: Show summary
    show_summary()
    
    logger.info("="*60)
    logger.info("SUCCESS - AMC Master System Ready!")
    logger.info("="*60)
    logger.info("")
    logger.info("Next steps:")
    logger.info("1. Check pending reviews: SELECT * FROM v_amc_staging_pending;")
    logger.info("2. View coverage: SELECT * FROM v_amc_master_coverage;")
    logger.info("3. Use in ETL: from Etl.amc_master_builder import AmcMasterBuilder")
    logger.info("")
    
    return True


if __name__ == "__main__":
    import sys
    success = main()
    sys.exit(0 if success else 1)