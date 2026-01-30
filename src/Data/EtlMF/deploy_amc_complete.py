"""
deploy_amc_complete.py
----------------------
Complete AMC Master deployment with all dependencies
"""

import logging
from pathlib import Path
from sqlalchemy import text
from Etl.utils import engine

logging.basicConfig(level=logging.INFO, format='%(levelname)s - %(message)s')
logger = logging.getLogger(__name__)


def check_dependencies():
    """Check if required extensions and functions exist"""
    logger.info("Checking dependencies...")
    
    checks = {
        'pg_trgm extension': """
            SELECT COUNT(*) FROM pg_extension WHERE extname = 'pg_trgm'
        """,
        'normalize_amc_name_sql function': """
            SELECT COUNT(*) FROM pg_proc 
            WHERE proname = 'normalize_amc_name_sql'
        """,
        'amc_master table': """
            SELECT COUNT(*) FROM information_schema.tables 
            WHERE table_name = 'amc_master'
        """,
        'amc_match_review table': """
            SELECT COUNT(*) FROM information_schema.tables 
            WHERE table_name = 'amc_match_review'
        """
    }
    
    missing = []
    
    with engine.connect() as conn:
        for check_name, query in checks.items():
            result = conn.execute(text(query)).scalar()
            if result == 0:
                missing.append(check_name)
                logger.warning(f"  X {check_name} - MISSING")
            else:
                logger.info(f"  Y {check_name} - OK")
    
    return missing


def install_pg_trgm():
    """Install pg_trgm extension if missing"""
    logger.info("Installing pg_trgm extension...")
    try:
        with engine.begin() as conn:
            conn.execute(text("CREATE EXTENSION IF NOT EXISTS pg_trgm"))
        logger.info("  Y pg_trgm extension installed")
        return True
    except Exception as e:
        logger.error(f"  X Failed to install pg_trgm: {e}")
        logger.error("  Please run as superuser: CREATE EXTENSION pg_trgm;")
        return False


def deploy_amc_schema():
    """Deploy complete AMC master schema"""
    logger.info("Deploying AMC master schema...")
    
    # SQL statements in order
    statements = [
        # 1. Create amc_staging table
        """
        DROP TABLE IF EXISTS amc_staging CASCADE;
        CREATE TABLE amc_staging (
            id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
            source_table TEXT NOT NULL,
            source_amc_name TEXT NOT NULL,
            source_amc_code TEXT,
            normalized_name TEXT,
            suggested_amc_id UUID,
            match_confidence NUMERIC,
            match_type TEXT,
            scheme_count INTEGER DEFAULT 0,
            status TEXT DEFAULT 'PENDING',
            reviewed_by TEXT,
            reviewed_at TIMESTAMP,
            notes TEXT,
            created_at TIMESTAMP DEFAULT now(),
            updated_at TIMESTAMP
        )
        """,
        
        # 2. Create indexes
        "CREATE INDEX idx_amc_staging_status ON amc_staging(status)",
        "CREATE INDEX idx_amc_staging_source ON amc_staging(source_table, source_amc_code)",
        "CREATE UNIQUE INDEX idx_amc_staging_unique ON amc_staging(source_table, source_amc_name, source_amc_code)",
        
        # 3. Add BSE/NSE columns to amc_master
        "ALTER TABLE amc_master ADD COLUMN IF NOT EXISTS bse_amc_code TEXT",
        "ALTER TABLE amc_master ADD COLUMN IF NOT EXISTS nse_amc_code TEXT",
        
        # 4. Create find_amc_match_v3 function
        """
        CREATE OR REPLACE FUNCTION find_amc_match_v3(
          p_name TEXT,
          p_code TEXT,
          OUT amc_id UUID,
          OUT match_type TEXT,
          OUT confidence NUMERIC
        )
        LANGUAGE plpgsql
        AS $$
        DECLARE
          v_norm_name TEXT;
          v_code TEXT;
          v_candidate RECORD;
          v_short_name TEXT;
        BEGIN
          amc_id := NULL;
          match_type := NULL;
          confidence := 0.0;
          
          v_norm_name := normalize_amc_name_sql(p_name);
          v_code := UPPER(TRIM(COALESCE(p_code, '')));
          
          IF v_norm_name = '' AND v_code = '' THEN
            match_type := 'EMPTY_INPUT';
            RETURN;
          END IF;
          
          IF v_code <> '' THEN
            SELECT am.amc_id INTO amc_id
            FROM amc_master am
            WHERE v_code = ANY(string_to_array(UPPER(COALESCE(am.cams_amc_code, '')), ','))
               OR v_code = ANY(string_to_array(UPPER(COALESCE(am.kfin_amc_code, '')), ','))
            LIMIT 1;
            
            IF amc_id IS NOT NULL THEN
              match_type := 'CODE_EXACT';
              confidence := 1.0;
              RETURN;
            END IF;
          END IF;
          
          IF v_norm_name <> '' THEN
            SELECT am.amc_id INTO amc_id
            FROM amc_master am
            WHERE normalize_amc_name_sql(am.amc_full_name) = v_norm_name
               OR normalize_amc_name_sql(am.amc_short_name) = v_norm_name
            LIMIT 1;
            
            IF amc_id IS NOT NULL THEN
              match_type := 'NAME_EXACT';
              confidence := 0.95;
              RETURN;
            END IF;
          END IF;
          
          IF v_norm_name <> '' AND LENGTH(v_norm_name) <= 15 THEN
            v_short_name := split_part(v_norm_name, ' ', 1);
            
            SELECT am.amc_id INTO amc_id
            FROM amc_master am
            WHERE normalize_amc_name_sql(am.amc_short_name) = v_short_name
               OR normalize_amc_name_sql(am.amc_full_name) LIKE v_short_name || '%'
            LIMIT 1;
            
            IF amc_id IS NOT NULL THEN
              match_type := 'NAME_PARTIAL';
              confidence := 0.85;
              RETURN;
            END IF;
          END IF;
          
          IF v_norm_name <> '' THEN
            SELECT 
              am.amc_id,
              GREATEST(
                similarity(normalize_amc_name_sql(am.amc_full_name), v_norm_name),
                similarity(normalize_amc_name_sql(am.amc_short_name), v_norm_name)
              ) as sim
            INTO v_candidate
            FROM amc_master am
            WHERE similarity(normalize_amc_name_sql(am.amc_full_name), v_norm_name) >= 0.50
               OR similarity(normalize_amc_name_sql(am.amc_short_name), v_norm_name) >= 0.50
            ORDER BY sim DESC
            LIMIT 1;
            
            IF v_candidate.amc_id IS NOT NULL THEN
              amc_id := v_candidate.amc_id;
              confidence := v_candidate.sim;
              
              IF confidence >= 0.80 THEN
                match_type := 'NAME_FUZZY_HIGH';
              ELSIF confidence >= 0.65 THEN
                match_type := 'NAME_FUZZY_MEDIUM';
              ELSE
                match_type := 'NAME_FUZZY_LOW';
              END IF;
              
              RETURN;
            END IF;
          END IF;
          
          match_type := 'NO_MATCH';
          confidence := 0.0;
        END;
        $$
        """,
        
        # 5. Create get_amc_id_v3 function
        """
        CREATE OR REPLACE FUNCTION get_amc_id_v3(
          p_name TEXT,
          p_code TEXT,
          p_source_table TEXT DEFAULT NULL,
          p_source_scheme_id TEXT DEFAULT NULL
        )
        RETURNS UUID
        LANGUAGE plpgsql
        AS $$
        DECLARE
          v_match RECORD;
          v_norm_name TEXT;
          v_existing_review UUID;
        BEGIN
          SELECT * INTO v_match FROM find_amc_match_v3(p_name, p_code);
          
          IF v_match.match_type = 'EMPTY_INPUT' THEN
            RETURN NULL;
          END IF;
          
          IF v_match.amc_id IS NOT NULL AND v_match.confidence >= 0.80 THEN
            RETURN v_match.amc_id;
          END IF;
          
          v_norm_name := normalize_amc_name_sql(p_name);
          
          SELECT id INTO v_existing_review
          FROM amc_match_review
          WHERE source_amc_name = v_norm_name
            AND source_amc_code = TRIM(COALESCE(p_code, ''))
            AND status = 'OPEN'
          LIMIT 1;
          
          IF v_existing_review IS NULL THEN
            INSERT INTO amc_match_review (
              source_table, source_scheme_id, source_amc_name, source_amc_code,
              suggested_amc_id, suggested_amc_name, similarity, match_type,
              status, created_at
            )
            VALUES (
              p_source_table, p_source_scheme_id, v_norm_name, TRIM(COALESCE(p_code, '')),
              v_match.amc_id, 
              (SELECT amc_full_name FROM amc_master WHERE amc_id = v_match.amc_id),
              v_match.confidence, v_match.match_type, 'OPEN', now()
            );
          END IF;
          
          RETURN NULL;
        END;
        $$
        """,
        
        # 6. Backward compatibility wrapper
        """
        CREATE OR REPLACE FUNCTION get_amc_id(p_name TEXT, p_code TEXT)
        RETURNS UUID
        LANGUAGE plpgsql
        AS $$
        BEGIN
          RETURN get_amc_id_v3(p_name, p_code, NULL, NULL);
        END;
        $$
        """,
    ]
    
    try:
        with engine.begin() as conn:
            for idx, stmt in enumerate(statements, 1):
                logger.info(f"  Executing step {idx}/{len(statements)}...")
                conn.execute(text(stmt))
        
        logger.info("Y AMC schema core functions deployed successfully!")
        return True
    
    except Exception as e:
        logger.exception(f"X Schema deployment failed: {e}")
        return False


def deploy_procedures():
    """Deploy stored procedures from SQL file"""
    logger.info("Deploying stored procedures...")
    
    sql_file = Path("Database/amc_master_generator.sql")
    
    if not sql_file.exists():
        logger.warning(f"  SQL file not found: {sql_file}")
        logger.warning("  Skipping procedure deployment - core functions are ready")
        return False
    
    try:
        with open(sql_file, 'r', encoding='utf-8-sig') as f:
            sql_content = f.read()
        
        # Remove BOM if present
        if sql_content.startswith('\ufeff'):
            sql_content = sql_content[1:]
        
        with engine.begin() as conn:
            conn.execute(text(sql_content))
        
        logger.info("Y Stored procedures deployed!")
        return True
    
    except Exception as e:
        logger.warning(f"  Procedure deployment failed: {e}")
        logger.info("  Core functions are ready - you can deploy procedures later")
        return False


def verify_deployment():
    """Verify deployment success"""
    logger.info("Verifying deployment...")
    
    checks = {
        'amc_staging table': "SELECT COUNT(*) FROM amc_staging",
        'find_amc_match_v3': "SELECT find_amc_match_v3('Test', NULL)",
        'get_amc_id_v3': "SELECT get_amc_id_v3('Test', NULL)",
        'get_amc_id': "SELECT get_amc_id('Test', NULL)"
    }
    
    all_ok = True
    
    with engine.connect() as conn:
        for check_name, query in checks.items():
            try:
                conn.execute(text(query))
                logger.info(f"  Y {check_name} - OK")
            except Exception as e:
                logger.error(f"  X {check_name} - FAILED: {e}")
                all_ok = False
    
    return all_ok


def main():
    """Main deployment workflow"""
    logger.info("="*60)
    logger.info("AMC MASTER COMPLETE DEPLOYMENT")
    logger.info("="*60)
    
    # Step 1: Check dependencies
    missing = check_dependencies()
    
    if 'pg_trgm extension' in missing:
        if not install_pg_trgm():
            logger.error("Cannot proceed without pg_trgm extension")
            return False
    
    # Step 2: Deploy core schema and functions
    if not deploy_amc_schema():
        logger.error("Core schema deployment failed!")
        return False
    
    # Step 3: Deploy procedures (optional - core functions work without this)
    deploy_procedures()
    
    # Step 4: Verify
    if not verify_deployment():
        logger.error("Verification failed!")
        return False
    
    logger.info("="*60)
    logger.info("DEPLOYMENT SUCCESSFUL!")
    logger.info("="*60)
    logger.info("")
    logger.info("Next steps:")
    logger.info("1. Run: python -m Etl.amc_master_builder build")
    logger.info("2. Or SQL: CALL sp_build_amc_master_complete();")
    logger.info("")
    
    return True


if __name__ == "__main__":
    import sys
    success = main()
    sys.exit(0 if success else 1)
