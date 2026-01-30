-- =====================================================
-- AMC MASTER DATA GENERATION SYSTEM (FIXED ENCODING)
-- Purpose: Extract and maintain AMC master from raw sources
--          Support incremental updates for BSE/NSE codes
-- =====================================================

-- =====================================================
-- STEP 1: AMC STAGING TABLE (for discovery & review)
-- =====================================================

DROP TABLE IF EXISTS amc_staging CASCADE;
CREATE TABLE amc_staging (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    
    -- Raw AMC data from sources
    source_table TEXT NOT NULL,
    source_amc_name TEXT NOT NULL,
    source_amc_code TEXT,
    
    -- Normalized data
    normalized_name TEXT,
    
    -- Matching to master
    suggested_amc_id UUID,
    match_confidence NUMERIC,
    match_type TEXT,
    
    -- Counts (how many schemes use this AMC)
    scheme_count INTEGER DEFAULT 0,
    
    -- Review status
    status TEXT DEFAULT 'PENDING',
    reviewed_by TEXT,
    reviewed_at TIMESTAMP,
    notes TEXT,
    
    created_at TIMESTAMP DEFAULT now(),
    updated_at TIMESTAMP
);

CREATE INDEX idx_amc_staging_status ON amc_staging(status);
CREATE INDEX idx_amc_staging_source ON amc_staging(source_table, source_amc_code);
CREATE UNIQUE INDEX idx_amc_staging_unique ON amc_staging(source_table, source_amc_name, source_amc_code);

-- =====================================================
-- STEP 1B: Add BSE/NSE columns to amc_master if missing
-- =====================================================
ALTER TABLE amc_master ADD COLUMN IF NOT EXISTS bse_amc_code TEXT;
ALTER TABLE amc_master ADD COLUMN IF NOT EXISTS nse_amc_code TEXT;

-- =====================================================
-- STEP 1C: IMPROVED MATCHING FUNCTION (v3)
-- =====================================================

CREATE OR REPLACE FUNCTION find_amc_match_v3(
  p_name TEXT,
  p_code TEXT,
  OUT amc_id UUID,
  OUT match_type TEXT,
  OUT confidence NUMERIC
)
LANGUAGE plpgsql
AS $
DECLARE
  v_norm_name TEXT;
  v_code TEXT;
  v_candidate RECORD;
  v_short_name TEXT;
BEGIN
  -- Initialize outputs
  amc_id := NULL;
  match_type := NULL;
  confidence := 0.0;
  
  v_norm_name := normalize_amc_name_sql(p_name);
  v_code := UPPER(TRIM(COALESCE(p_code, '')));
  
  -- Skip if both inputs are empty
  IF v_norm_name = '' AND v_code = '' THEN
    match_type := 'EMPTY_INPUT';
    RETURN;
  END IF;
  
  -- PRIORITY 1: Exact code match in comma-separated lists
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
  
  -- PRIORITY 2: Exact normalized name match
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
  
  -- PRIORITY 3: Partial word match (for short names like "Hdfc", "Sbi")
  IF v_norm_name <> '' AND LENGTH(v_norm_name) <= 15 THEN
    -- Extract first word from input
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
  
  -- PRIORITY 4: Fuzzy match using trigram similarity
  IF v_norm_name <> '' THEN
    -- Try against full name
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
      
      -- Classify fuzzy match quality
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
  
  -- No match found
  match_type := 'NO_MATCH';
  confidence := 0.0;
END;
$;

-- =====================================================
-- STEP 1D: UPDATE get_amc_id to use v3 matching
-- =====================================================

CREATE OR REPLACE FUNCTION get_amc_id_v3(
  p_name TEXT,
  p_code TEXT,
  p_source_table TEXT DEFAULT NULL,
  p_source_scheme_id TEXT DEFAULT NULL
)
RETURNS UUID
LANGUAGE plpgsql
AS $
DECLARE
  v_match RECORD;
  v_norm_name TEXT;
  v_review_id UUID;
  v_existing_review UUID;
BEGIN
  -- Get match result using improved function
  SELECT * INTO v_match
  FROM find_amc_match_v3(p_name, p_code);
  
  -- Skip empty inputs
  IF v_match.match_type = 'EMPTY_INPUT' THEN
    RETURN NULL;
  END IF;
  
  -- AUTO-ASSIGN: High confidence matches (>= 0.80)
  IF v_match.amc_id IS NOT NULL AND v_match.confidence >= 0.80 THEN
    RETURN v_match.amc_id;
  END IF;
  
  -- MANUAL REVIEW: Medium/low confidence or no match
  v_norm_name := normalize_amc_name_sql(p_name);
  
  -- Check if review record already exists (avoid duplicates)
  SELECT id INTO v_existing_review
  FROM amc_match_review
  WHERE source_amc_name = v_norm_name
    AND source_amc_code = TRIM(COALESCE(p_code, ''))
    AND status = 'OPEN'
  LIMIT 1;
  
  IF v_existing_review IS NULL THEN
    -- Create new review record
    INSERT INTO amc_match_review (
      source_table,
      source_scheme_id,
      source_amc_name,
      source_amc_code,
      suggested_amc_id,
      suggested_amc_name,
      similarity,
      match_type,
      status,
      created_at
    )
    VALUES (
      p_source_table,
      p_source_scheme_id,
      v_norm_name,
      TRIM(COALESCE(p_code, '')),
      v_match.amc_id,
      (SELECT amc_full_name FROM amc_master WHERE amc_id = v_match.amc_id),
      v_match.confidence,
      v_match.match_type,
      'OPEN',
      now()
    )
    RETURNING id INTO v_review_id;
  END IF;
  
  -- Return NULL for manual review
  RETURN NULL;
END;
$;

-- Keep backward compatibility: get_amc_id calls get_amc_id_v3
CREATE OR REPLACE FUNCTION get_amc_id(p_name TEXT, p_code TEXT)
RETURNS UUID
LANGUAGE plpgsql
AS $
BEGIN
  RETURN get_amc_id_v3(p_name, p_code, NULL, NULL);
END;
$;

-- =====================================================
-- STEP 2: EXTRACT AMCs FROM RAW SOURCES
-- =====================================================

CREATE OR REPLACE PROCEDURE sp_extract_amcs_from_raw()
LANGUAGE plpgsql
AS $$
BEGIN
    RAISE NOTICE 'Extracting AMCs from raw sources...';
    
    -- Extract from CAMS raw
    INSERT INTO amc_staging (
        source_table,
        source_amc_name,
        source_amc_code,
        normalized_name,
        scheme_count,
        status,
        created_at
    )
    SELECT 
        'cams_raw' as source_table,
        amc as source_amc_name,
        amc_code as source_amc_code,
        normalize_amc_name_sql(amc) as normalized_name,
        COUNT(*) as scheme_count,
        'PENDING' as status,
        now()
    FROM cams_raw
    WHERE amc IS NOT NULL 
      AND TRIM(amc) <> ''
    GROUP BY amc, amc_code
    ON CONFLICT (source_table, source_amc_name, source_amc_code) 
    DO UPDATE SET
        scheme_count = EXCLUDED.scheme_count,
        updated_at = now();
    
    RAISE NOTICE 'Extracted CAMS AMCs';
    
    -- Extract from KFIN raw
    INSERT INTO amc_staging (
        source_table,
        source_amc_name,
        source_amc_code,
        normalized_name,
        scheme_count,
        status,
        created_at
    )
    SELECT 
        'kfin_raw' as source_table,
        "AMC Name" as source_amc_name,
        "AMC Code" as source_amc_code,
        normalize_amc_name_sql("AMC Name") as normalized_name,
        COUNT(*) as scheme_count,
        'PENDING' as status,
        now()
    FROM kfin_raw
    WHERE "AMC Name" IS NOT NULL 
      AND TRIM("AMC Name") <> ''
    GROUP BY "AMC Name", "AMC Code"
    ON CONFLICT (source_table, source_amc_name, source_amc_code) 
    DO UPDATE SET
        scheme_count = EXCLUDED.scheme_count,
        updated_at = now();
    
    RAISE NOTICE 'Extracted KFIN AMCs';
    RAISE NOTICE 'AMC extraction complete!';
END;
$$;

-- =====================================================
-- STEP 3: AUTO-MATCH STAGING AMCs TO MASTER
-- =====================================================

CREATE OR REPLACE PROCEDURE sp_match_staging_to_master()
LANGUAGE plpgsql
AS $$
DECLARE
    rec RECORD;
    v_match RECORD;
BEGIN
    RAISE NOTICE 'Matching staging AMCs to master...';
    
    FOR rec IN 
        SELECT * FROM amc_staging 
        WHERE status = 'PENDING'
          AND suggested_amc_id IS NULL
    LOOP
        -- Use the improved matching function
        SELECT * INTO v_match
        FROM find_amc_match_v3(rec.source_amc_name, rec.source_amc_code);
        
        -- Update staging record with match result
        UPDATE amc_staging
        SET 
            suggested_amc_id = v_match.amc_id,
            match_confidence = v_match.confidence,
            match_type = v_match.match_type,
            updated_at = now()
        WHERE id = rec.id;
        
    END LOOP;
    
    RAISE NOTICE 'Matching complete!';
END;
$$;

-- =====================================================
-- STEP 4: GENERATE AMC MASTER FROM STAGING
-- =====================================================

CREATE OR REPLACE PROCEDURE sp_generate_amc_master(
    p_auto_approve_threshold NUMERIC DEFAULT 0.85
)
LANGUAGE plpgsql
AS $$
DECLARE
    rec RECORD;
    v_new_amc_id UUID;
    v_amc_code TEXT;
    v_counter INTEGER;
    v_base_code TEXT;
BEGIN
    RAISE NOTICE 'Generating AMC master from staging...';
    
    -- AUTO-APPROVE: High-confidence matches
    FOR rec IN 
        SELECT * FROM amc_staging 
        WHERE status = 'PENDING'
          AND suggested_amc_id IS NOT NULL
          AND match_confidence >= p_auto_approve_threshold
    LOOP
        UPDATE amc_staging
        SET 
            status = 'APPROVED',
            reviewed_by = 'system',
            reviewed_at = now()
        WHERE id = rec.id;
    END LOOP;
    
    RAISE NOTICE 'Auto-approved high-confidence matches';
    
    -- CREATE NEW AMCs: For unmatched staging records
    FOR rec IN 
        SELECT 
            normalized_name,
            array_agg(DISTINCT source_amc_code) FILTER (WHERE source_amc_code IS NOT NULL) as codes,
            array_agg(DISTINCT source_table) as sources,
            SUM(scheme_count) as total_schemes
        FROM amc_staging
        WHERE status = 'PENDING'
          AND (suggested_amc_id IS NULL OR match_confidence < 0.60)
        GROUP BY normalized_name
        HAVING COUNT(*) > 0
    LOOP
        -- Generate unique AMC code
        v_base_code := UPPER(LEFT(regexp_replace(rec.normalized_name, '[^A-Za-z]', '', 'g'), 3));
        IF LENGTH(v_base_code) < 3 THEN
            v_base_code := (v_base_code || 'XXX')::TEXT;
            v_base_code := LEFT(v_base_code, 3);
        END IF;
        
        -- Find next available counter
        SELECT COALESCE(MAX(NULLIF(regexp_replace(amc_code, '^[A-Z]{3}', ''), '')::INTEGER), 0) + 1
        INTO v_counter
        FROM amc_master
        WHERE amc_code LIKE v_base_code || '%';
        
        v_amc_code := v_base_code || LPAD(v_counter::TEXT, 3, '0');
        
        -- Insert into amc_master
        INSERT INTO amc_master (
            amc_code,
            amc_short_name,
            amc_full_name,
            cams_amc_code,
            kfin_amc_code,
            created_at,
            updated_at
        )
        VALUES (
            v_amc_code,
            rec.normalized_name,
            rec.normalized_name,
            CASE WHEN 'cams_raw' = ANY(rec.sources) 
                 THEN array_to_string(rec.codes, ',') 
                 ELSE NULL END,
            CASE WHEN 'kfin_raw' = ANY(rec.sources) 
                 THEN array_to_string(rec.codes, ',') 
                 ELSE NULL END,
            now(),
            now()
        )
        RETURNING amc_id INTO v_new_amc_id;
        
        -- Update staging records to point to new master
        UPDATE amc_staging
        SET 
            suggested_amc_id = v_new_amc_id,
            status = 'APPROVED',
            match_confidence = 1.0,
            match_type = 'NEW_AMC',
            reviewed_by = 'system',
            reviewed_at = now()
        WHERE normalized_name = rec.normalized_name
          AND status = 'PENDING';
        
        RAISE NOTICE 'Created new AMC: % (code: %, schemes: %)', 
            rec.normalized_name, v_amc_code, rec.total_schemes;
    END LOOP;
    
    RAISE NOTICE 'AMC master generation complete!';
END;
$$;

-- =====================================================
-- STEP 5: UPDATE AMC CODES (for new sources like BSE/NSE)
-- =====================================================

CREATE OR REPLACE PROCEDURE sp_update_amc_codes(
    p_source TEXT,
    p_amc_code TEXT,
    p_amc_id UUID DEFAULT NULL,
    p_amc_name TEXT DEFAULT NULL
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_amc_id UUID;
    v_column_name TEXT;
    v_current_codes TEXT;
    v_updated_codes TEXT;
BEGIN
    -- Determine which column to update
    v_column_name := LOWER(p_source) || '_amc_code';
    
    -- Validate column exists
    IF v_column_name NOT IN ('cams_amc_code', 'kfin_amc_code', 'bse_amc_code', 'nse_amc_code') THEN
        RAISE EXCEPTION 'Invalid source: %. Must be cams/kfin/bse/nse', p_source;
    END IF;
    
    -- Find AMC if not provided
    IF p_amc_id IS NULL THEN
        IF p_amc_name IS NULL THEN
            RAISE EXCEPTION 'Either p_amc_id or p_amc_name must be provided';
        END IF;
        
        SELECT amc_id INTO v_amc_id
        FROM amc_master
        WHERE normalize_amc_name_sql(amc_full_name) = normalize_amc_name_sql(p_amc_name)
        LIMIT 1;
        
        IF v_amc_id IS NULL THEN
            RAISE EXCEPTION 'AMC not found: %', p_amc_name;
        END IF;
    ELSE
        v_amc_id := p_amc_id;
    END IF;
    
    -- Get current codes
    EXECUTE format('SELECT %I FROM amc_master WHERE amc_id = $1', v_column_name)
    INTO v_current_codes
    USING v_amc_id;
    
    -- Append new code if not already present
    IF v_current_codes IS NULL OR v_current_codes = '' THEN
        v_updated_codes := p_amc_code;
    ELSIF position(p_amc_code IN v_current_codes) = 0 THEN
        v_updated_codes := v_current_codes || ',' || p_amc_code;
    ELSE
        v_updated_codes := v_current_codes;
    END IF;
    
    -- Update the column
    EXECUTE format('UPDATE amc_master SET %I = $1, updated_at = now() WHERE amc_id = $2', v_column_name)
    USING v_updated_codes, v_amc_id;
    
    RAISE NOTICE 'Updated % for AMC % with code %', v_column_name, v_amc_id, p_amc_code;
END;
$$;

-- =====================================================
-- STEP 6: HELPER VIEWS FOR REVIEW
-- =====================================================

CREATE OR REPLACE VIEW v_amc_staging_pending AS
SELECT 
    s.id,
    s.source_table,
    s.source_amc_name,
    s.source_amc_code,
    s.normalized_name,
    s.scheme_count,
    s.match_type,
    s.match_confidence,
    m.amc_code as suggested_master_code,
    m.amc_full_name as suggested_master_name,
    CASE 
        WHEN s.match_confidence >= 0.70 THEN 'High - Review & Approve'
        WHEN s.match_confidence >= 0.50 THEN 'Medium - Needs Review'
        WHEN s.suggested_amc_id IS NULL THEN 'No Match - Will Create New'
        ELSE 'Low - Manual Decision'
    END as recommendation
FROM amc_staging s
LEFT JOIN amc_master m ON m.amc_id = s.suggested_amc_id
WHERE s.status = 'PENDING'
ORDER BY s.scheme_count DESC, s.match_confidence DESC;

CREATE OR REPLACE VIEW v_amc_master_coverage AS
SELECT 
    amc_code,
    amc_short_name,
    amc_full_name,
    CASE WHEN cams_amc_code IS NOT NULL THEN 'Y' ELSE 'N' END as has_cams,
    CASE WHEN kfin_amc_code IS NOT NULL THEN 'Y' ELSE 'N' END as has_kfin,
    CASE WHEN bse_amc_code IS NOT NULL THEN 'Y' ELSE 'N' END as has_bse,
    cams_amc_code,
    kfin_amc_code,
    bse_amc_code,
    created_at
FROM amc_master
ORDER BY amc_code;

-- =====================================================
-- STEP 7: MANUAL APPROVAL HELPERS
-- =====================================================

CREATE OR REPLACE PROCEDURE sp_approve_staging_amc(
    p_staging_id UUID,
    p_master_amc_id UUID,
    p_reviewer TEXT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_source_table TEXT;
    v_source_code TEXT;
BEGIN
    -- Get staging details
    SELECT source_table, source_amc_code
    INTO v_source_table, v_source_code
    FROM amc_staging
    WHERE id = p_staging_id;
    
    IF NOT FOUND THEN
        RAISE EXCEPTION 'Staging record % not found', p_staging_id;
    END IF;
    
    -- Update staging status
    UPDATE amc_staging
    SET 
        suggested_amc_id = p_master_amc_id,
        status = 'APPROVED',
        reviewed_by = p_reviewer,
        reviewed_at = now()
    WHERE id = p_staging_id;
    
    -- Update master with source code
    IF v_source_code IS NOT NULL THEN
        IF v_source_table = 'cams_raw' THEN
            CALL sp_update_amc_codes('cams', v_source_code, p_master_amc_id);
        ELSIF v_source_table = 'kfin_raw' THEN
            CALL sp_update_amc_codes('kfin', v_source_code, p_master_amc_id);
        END IF;
    END IF;
    
    RAISE NOTICE 'Approved staging AMC % to Master %', p_staging_id, p_master_amc_id;
END;
$$;

-- =====================================================
-- COMPLETE WORKFLOW
-- =====================================================

CREATE OR REPLACE PROCEDURE sp_build_amc_master_complete()
LANGUAGE plpgsql
AS $$
BEGIN
    RAISE NOTICE 'Starting complete AMC master build...';
    
    -- Step 1: Extract from raw sources
    CALL sp_extract_amcs_from_raw();
    
    -- Step 2: Match to existing master
    CALL sp_match_staging_to_master();
    
    -- Step 3: Generate master (auto-approve high confidence, create new for rest)
    CALL sp_generate_amc_master(0.85);
    
    RAISE NOTICE 'AMC master build complete!';
    RAISE NOTICE 'Review pending: SELECT * FROM v_amc_staging_pending;';
    RAISE NOTICE 'Master coverage: SELECT * FROM v_amc_master_coverage;';
END;
$$;

-- =====================================================
-- SUCCESS MESSAGE
-- =====================================================
DO $$
BEGIN
    RAISE NOTICE '========================================';
    RAISE NOTICE 'AMC Master Schema Deployed Successfully!';
    RAISE NOTICE '========================================';
    RAISE NOTICE 'Next steps:';
    RAISE NOTICE '1. Run: CALL sp_build_amc_master_complete();';
    RAISE NOTICE '2. Check: SELECT * FROM v_amc_staging_pending;';
    RAISE NOTICE '3. Review: SELECT * FROM v_amc_master_coverage;';
    RAISE NOTICE '========================================';
END $$;