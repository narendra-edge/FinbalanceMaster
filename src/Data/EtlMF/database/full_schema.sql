    -- =============================================
-- FULL SCHEMA: Mutual Fund Data Warehouse
-- Purpose: Keep raw tables (CAMS, KFIN, AMFI)
--          Build combined & cleaned normalized tables
--          Provide merge logic from raw → normalized
-- =============================================

-- Enable extensions (pgcrypto for gen_random_uuid, pg_trgm for trigram similarity)
SET client_min_messages = WARNING;

-- Ensure required extensions exist
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";
CREATE EXTENSION IF NOT EXISTS pg_trgm;

SET client_min_messages = WARNING;
-- -------------------------
-- 1. Utility: ISIN validation
-- -------------------------
CREATE OR REPLACE FUNCTION is_valid_isin(txt TEXT)
RETURNS BOOLEAN
LANGUAGE sql
IMMUTABLE
AS $$
  SELECT txt IS NOT NULL
    AND regexp_replace(txt, '\s+','', 'g') ~* '^IN[A-Z0-9]{10}$';
$$;

-- small normalizer for scheme names
CREATE OR REPLACE FUNCTION normalize_name(txt TEXT)
RETURNS TEXT
LANGUAGE sql IMMUTABLE
AS $$
  SELECT lower(regexp_replace(coalesce(txt,''),'[^A-Za-z0-9 ]','','g'));
$$;

-- ----------------------------------------------------------------
-- 2.RAW staging tables (CAMS columnized per CR2, KFIN exact, AMFI json)
-- ----------------------------------------------------------------
DROP TABLE IF EXISTS cams_raw;
CREATE TABLE IF NOT EXISTS cams_raw (
    id                  UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    imported_at         TIMESTAMP DEFAULT now(),
    source              VARCHAR,                
    amc_code            VARCHAR,
    amc                 VARCHAR,
    sch_code            VARCHAR,
    sch_name            VARCHAR,
    sch_type            VARCHAR,
    div_reinv           VARCHAR,
    sip_allow           VARCHAR,
    lien                VARCHAR,
    swt_mn_amt          DECIMAL,
    swt_mx_amt          NUMERIC,
    swt_mn_unt          DECIMAL,
    swt_mx_unt          NUMERIC,
    swi_multi           DECIMAL,
    adp_mn_amt          DECIMAL,
    adp_mx_amt          NUMERIC,
    adp_mn_unt          DECIMAL,
    adp_mx_unt          DECIMAL,
    newp_mnval          DECIMAL,
    newp_mxval          DECIMAL,
    p_mn_incr           DECIMAL,
    red_mn_amt          DECIMAL,
    red_mx_amt          NUMERIC,
    red_mn_unt          DECIMAL,
    red_mx_unt          NUMERIC,
    red_incr            DECIMAL,
    mn_swp_amt          DECIMAL,
    mx_swp_amt          NUMERIC,
    close_end           VARCHAR,
    elss_sch            VARCHAR,
    mature_dt           VARCHAR,
    face_value          DECIMAL,
    asset_class         VARCHAR,
    sebi_class          VARCHAR,
    settle_per          DECIMAL,
    sip_dates           VARCHAR,
    swp_dates           VARCHAR,
    stp_dates           VARCHAR,
    sys_freqs           VARCHAR,
    swp_allow           VARCHAR,
    stp_allow           VARCHAR,
    plan_type           VARCHAR,
    sf_code             VARCHAR,
    sf_name             VARCHAR,
    start_date          DATE,
    swt_allow           VARCHAR,
    adp_mn_inc          DECIMAL,
    pur_allow           VARCHAR,
    red_allow           VARCHAR,
    sip_mn_ins          DECIMAL,
    sip_mn_amt          DECIMAL,
    sip_multi           DECIMAL,
    sip_mx_amt          NUMERIC,
    swp_mn_ins          DECIMAL,
    swp_multi           DECIMAL,
    parent_scheme_code  VARCHAR,
    isin_no             VARCHAR,
    display_data_entry  VARCHAR,
    nfo_end_dt          VARCHAR,
    open_date           VARCHAR,
    allotment_date      VARCHAR,
    raw_row             JSONB               -- store raw row for audit
);

DROP TABLE IF EXISTS kfin_raw;
CREATE TABLE kfin_raw (
  id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
  imported_at TIMESTAMP DEFAULT now(),
  source VARCHAR,
  "Product Code" VARCHAR,
  "AMC Code" VARCHAR,
  "AMC Name" VARCHAR,
  "Scheme Code" VARCHAR,
  "Scheme Description" VARCHAR,
  "Plan Code" VARCHAR,
  "Plan Description" VARCHAR,
  "Option Code" VARCHAR,
  "Option Description" VARCHAR,
  "Nature" VARCHAR,
  "Fund Type" VARCHAR,
  "NFO Start Date" DATE,
  "NFO End Date" VARCHAR,
  "Close Date" VARCHAR,
  "Open Date" VARCHAR,
  "ISIN Number" VARCHAR,
  "ISIN Type" NUMERIC,
  "Purchased Allowed" VARCHAR,
  "IPO Amount" NUMERIC,
  "IPO Min Amount" NUMERIC,
  "IPO Multiple Amount" NUMERIC,
  "New Purchase Amount" NUMERIC,
  "New Purchase Multiple Amount" NUMERIC,
  "NRI New Multiple Amount" NUMERIC,
  "NRI NEW MIN AMOUNT" NUMERIC,
  "Add Purchase Amount" NUMERIC,
  "Add Purchase Multiple Amount" NUMERIC,
  "Redemption Allowed" VARCHAR,
  "Redemption Min Amount" NUMERIC,
  "Redemption Multiple Amount" DECIMAL,
  "Redemption Min Units" DECIMAL,
  "Redemption Multiple Units" NUMERIC,
  "Switch In Allowed" VARCHAR,
  "Switch Out Allowed" VARCHAR,
  "Switch Out Min Amount" NUMERIC,
  "Switch In Min Amount" NUMERIC,
  "Lateral In Allowed" VARCHAR,
  "Lateral Out Allowed" VARCHAR,
  "STP In Allowed" VARCHAR,
  "STP Out Allowed" VARCHAR,
  "STP Frequency" VARCHAR,
  "STP Dates" VARCHAR,
  "STP Min Amount" NUMERIC,
  "SIP Allowed" VARCHAR,
  "SIP Min Amount" NUMERIC,
  "SIP Frequency" VARCHAR,
  "SIP Dates" VARCHAR,
  "SWP In Allowed" VARCHAR,
  "SWP Out Allowed" VARCHAR,
  "SWP Frequency" VARCHAR,
  "SWP Dates" VARCHAR,
  "SWP Min Amount" NUMERIC,
  "Load Details" VARCHAR,
  "Purchase Cutoff Time" VARCHAR,
  "Redemption Cutoff Time" VARCHAR,
  "Switch Cutoff Time" VARCHAR,
  "Maturity Date" VARCHAR,
  "Reopen Date" VARCHAR,
  "NFO Face Value" DECIMAL,
  "Demat Allowed" VARCHAR,
  "Risk Type" VARCHAR,
  "Allotment Date" VARCHAR,
  "Last Update Date" VARCHAR,
  raw_row JSONB
);

DROP TABLE IF EXISTS amfi_raw;
CREATE TABLE IF NOT EXISTS amfi_raw (
    id                  UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    imported_at         TIMESTAMP DEFAULT now(),
    source VARCHAR,
    "AMC" VARCHAR,
    "Code" NUMERIC,
    "Scheme Name" VARCHAR,
    "Scheme Type" VARCHAR,
    "Scheme Category" VARCHAR,
    "Scheme NAV Name" VARCHAR,
    "Scheme Minimum Amount" VARCHAR,
    "Launch Date" DATE,
    "Closure Date" DATE,
    "ISIN Div Payout/ISIN GrowthISIN Div Reinvestment" VARCHAR,
    source_file TEXT,
    raw_payload JSONB,
    raw_row  JSONB
);

-- -------------------------
-- 3. without-isin raw tables
-- -------------------------
DROP TABLE IF EXISTS cams_without_isin;
CREATE TABLE cams_without_isin (LIKE cams_raw INCLUDING ALL);

DROP TABLE IF EXISTS kfin_without_isin;
CREATE TABLE kfin_without_isin (LIKE kfin_raw INCLUDING ALL);


-- -------------------------
-- 4. CAMS + KFIN master tables (identical schema)
--    both have scheme_id (text) and scheme-level fields
-- -------------------------
DROP TABLE IF EXISTS cams_scheme_master;
CREATE TABLE cams_scheme_master (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  scheme_id TEXT UNIQUE,             -- UPSERT key for CAMS (A1)
  rta_source TEXT NOT NULL DEFAULT 'CAMS',
  amc_code TEXT,
  amc_name TEXT,
  rta_scheme_code TEXT,
  scheme_code TEXT,
  scheme_nav_code TEXT,
  scheme_name TEXT,
  plan_type TEXT,
  option_type TEXT,
  isin TEXT,
  nature TEXT,
  start_date DATE,
  close_date DATE,
  face_value NUMERIC,
  purchase_allowed BOOLEAN,
  redemption_allowed BOOLEAN,
  sip_flag BOOLEAN,
  stp_in_allowed BOOLEAN,
  stp_out_allowed BOOLEAN,
  swp_in_allowed BOOLEAN,
  swp_out_allowed BOOLEAN,
  switch_in_allowed BOOLEAN,
  switch_out_allowed BOOLEAN,
  load_details TEXT,
  scheme_nav_name TEXT,
  imported_at TIMESTAMP,
  source_file TEXT,
  raw_payload JSONB,
  created_at TIMESTAMP DEFAULT now(),
  updated_at TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_cams_scheme_master_isin ON cams_scheme_master(isin);
CREATE INDEX IF NOT EXISTS idx_cams_scheme_master_norm ON cams_scheme_master USING gin (scheme_name gin_trgm_ops);

DROP TABLE IF EXISTS kfin_scheme_master;
CREATE TABLE kfin_scheme_master (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  scheme_id TEXT UNIQUE,             -- UPSERT key for KFIN (B1)
  rta_source TEXT NOT NULL DEFAULT 'KFIN',
  amc_code TEXT,
  amc_name TEXT,
  rta_scheme_code TEXT,
  scheme_code TEXT,
  scheme_nav_code TEXT,
  scheme_name TEXT,
  plan_type TEXT,
  option_type TEXT,
  isin TEXT,
  nature TEXT,
  start_date DATE,
  close_date DATE,
  face_value NUMERIC,
  purchase_allowed BOOLEAN,
  redemption_allowed BOOLEAN,
  sip_flag BOOLEAN,
  stp_in_allowed BOOLEAN,
  stp_out_allowed BOOLEAN,
  swp_in_allowed BOOLEAN,
  swp_out_allowed BOOLEAN,
  switch_in_allowed BOOLEAN,
  switch_out_allowed BOOLEAN,
  load_details TEXT,
  scheme_nav_name TEXT,
  imported_at TIMESTAMP,
  source_file TEXT,
  raw_payload JSONB,
  created_at TIMESTAMP DEFAULT now(),
  updated_at TIMESTAMP
);
CREATE INDEX IF NOT EXISTS idx_kfin_scheme_master_isin ON kfin_scheme_master(isin);
CREATE INDEX IF NOT EXISTS idx_kfin_scheme_master_norm ON kfin_scheme_master USING gin (scheme_name gin_trgm_ops);



-- -------------------------
-- 5. rta_combined_scheme_master
--    UPSERT key: scheme_id (C1)
-- -------------------------
DROP TABLE IF EXISTS rta_combined_scheme_master;
CREATE TABLE rta_combined_scheme_master (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  scheme_id TEXT UNIQUE,             -- primary upsert key across RTAs
  rta_source TEXT,
  amc_code TEXT,
  amc_name TEXT,
  rta_scheme_code TEXT,
  scheme_code TEXT,
  scheme_nav_code TEXT,
  scheme_name TEXT,
  plan_type TEXT,
  option_type TEXT,
  isin TEXT,
  nature TEXT,
  start_date DATE,
  close_date DATE,
  face_value NUMERIC,
  purchase_allowed BOOLEAN,
  redemption_allowed BOOLEAN,
  sip_flag BOOLEAN,
  stp_in_allowed BOOLEAN,
  stp_out_allowed BOOLEAN,
  swp_in_allowed BOOLEAN,
  swp_out_allowed BOOLEAN,
  switch_in_allowed BOOLEAN,
  switch_out_allowed BOOLEAN,
  load_details TEXT,
  scheme_nav_name TEXT,
  imported_at TIMESTAMP,
  source_file TEXT,
  raw_payload JSONB,
  created_at TIMESTAMP DEFAULT now(),
  updated_at TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_rta_combined_isin ON rta_combined_scheme_master(isin);
CREATE INDEX IF NOT EXISTS idx_rta_combined_norm ON rta_combined_scheme_master USING gin (scheme_name gin_trgm_ops);

-- -------------------------
-- 6. AMFI scheme (one row per ISIN)
--    Unique key: (code, isin_single) to upsert per AMFI scheme+ISIN
-- -------------------------
DROP TABLE IF EXISTS amfi_scheme;
CREATE TABLE amfi_scheme (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  amfi_code TEXT,          -- raw "Code"
  amc_name TEXT,
  scheme_name TEXT,
  scheme_type TEXT,
  category TEXT,
  sub_category TEXT,
  scheme_nav_name TEXT,
  scheme_min_amount NUMERIC,
  launch_date DATE,
  closure_date DATE,
  isin_single TEXT,        -- one ISIN per row
  isin_all TEXT,           -- all ISINs csv for audit
  imported_at TIMESTAMP,
  source_file TEXT,
  raw_payload JSONB,
  created_at TIMESTAMP DEFAULT now(),
  updated_at TIMESTAMP
);

-- unique constraint for upsert (amfi code + isin_single)
CREATE UNIQUE INDEX IF NOT EXISTS uq_amfi_code_isin ON amfi_scheme (amfi_code, isin_single);

CREATE INDEX IF NOT EXISTS idx_amfi_isin ON amfi_scheme(isin_single);
CREATE INDEX IF NOT EXISTS idx_amfi_norm ON amfi_scheme USING gin (scheme_name gin_trgm_ops);

-- -------------------------
-- 7. scheme_mapping
-- -------------------------
DROP TABLE IF EXISTS scheme_mapping;
CREATE TABLE scheme_mapping (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  rta_id UUID REFERENCES rta_combined_scheme_master(id),
  rta_scheme_id TEXT,     -- rta_combined.scheme_id
  amfi_id UUID REFERENCES amfi_scheme(id),
  amfi_code TEXT,
  rta_source TEXT,
  rta_scheme_code TEXT,
  rta_scheme_nav_name TEXT,
  amfi_scheme_nav_name TEXT,
  isin_match BOOLEAN,
  name_match_score NUMERIC,
  match_confidence INTEGER,
  mapping_source TEXT,
  verified_by TEXT,
  verified_at TIMESTAMP,
  created_at TIMESTAMP DEFAULT now(),
  updated_at TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_mapping_rta_schemeid ON scheme_mapping(rta_scheme_id);
CREATE INDEX IF NOT EXISTS idx_mapping_amfi_id ON scheme_mapping(amfi_id);

-- -------------------------
-- 8. scheme_master_final (best practice: unified canonical table)
--    - Upsert key: scheme_id (scheme_id from rta_combined)
--    - Includes important RTA columns plus AMFI columns
--    - Keep raw JSON blobs for audit and future reprojection
-- -------------------------
DROP TABLE IF EXISTS scheme_master_final;
CREATE TABLE scheme_master_final (
  scheme_id TEXT PRIMARY KEY,   -- match rta_combined.scheme_id - UPSERT key
  canonical_scheme_code TEXT,
  canonical_scheme_name TEXT,
  amc_name TEXT,
  isin TEXT,
  category TEXT,
  sub_category TEXT,
  plan_type TEXT,
  option_type TEXT,
  rta_sources JSONB,       -- list/object of RTA records (helps traceability)
  amfi_matches JSONB,      -- array of matched amfi records (id + code + name)
  rta_raw JSONB,
  amfi_raw JSONB,
  latest_nav NUMERIC,
  latest_nav_date DATE,
  latest_aaum NUMERIC,
  created_at TIMESTAMP DEFAULT now(),
  updated_at TIMESTAMP
  amcid uuid
);

CREATE INDEX IF NOT EXISTS idx_master_final_isin ON scheme_master_final(isin);
CREATE INDEX IF NOT EXISTS idx_master_final_amc ON scheme_master_final(amc_name);

CREATE TABLE IF NOT EXISTS amc_master (
  amc_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  amc_code TEXT UNIQUE NOT NULL,       -- canonical code (e.g. HDF001)
  amc_short_name TEXT,
  amc_full_name TEXT,
  cams_amc_code TEXT,                  -- comma-separated external codes found in CAMS
  kfin_amc_code TEXT,
  website TEXT,
  logo_url TEXT,
  created_at TIMESTAMP DEFAULT now(),
  updated_at TIMESTAMP
);

-- helpful indexes
CREATE INDEX IF NOT EXISTS idx_amc_fullname_lower ON amc_master(lower(amc_full_name));
CREATE INDEX IF NOT EXISTS idx_amc_codes_text ON amc_master(cams_amc_code);


CREATE TABLE IF NOT EXISTS amc_match_review (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  source_table TEXT,         -- e.g. 'cams_scheme_master'
  source_scheme_id TEXT,     -- the scheme_id or raw id being matched
  source_amc_name TEXT,
  source_amc_code TEXT,
  suggested_amc_id UUID,     -- top automated candidate (may be null)
  suggested_amc_name TEXT,
  similarity NUMERIC,        -- trigram similarity score (0-1)
  status TEXT DEFAULT 'OPEN',-- OPEN / IGNORED / MERGED
  notes TEXT,
  created_at TIMESTAMP DEFAULT now(),
  reviewed_by TEXT,
  reviewed_at TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_amc_match_review_source ON amc_match_review(source_table, source_scheme_id);

-- Enhanced AMC name normalization function (mirrors Python logic)

CREATE OR REPLACE FUNCTION normalize_amc_name_sql(name TEXT)
RETURNS TEXT
LANGUAGE plpgsql
IMMUTABLE
AS $$
BEGIN
  IF name IS NULL OR trim(name) = '' THEN
    RETURN '';
  END IF;
  
  -- Remove "Mutual Fund" suffix (case insensitive)
  name := regexp_replace(name, '\s*(?:mutual\s*fund|mf)\s*$', '', 'gi');
  
  -- Remove special characters except spaces and &
  name := regexp_replace(name, '[^A-Za-z0-9 &]', ' ', 'g');
  
  -- Collapse multiple spaces
  name := regexp_replace(name, '\s+', ' ', 'g');
  
  -- Trim and title case
  name := trim(name);
  name := initcap(name);
  
  RETURN name;
END;
$$;

-- 2. Core AMC matching function (returns match with confidence)
CREATE OR REPLACE FUNCTION find_amc_match(
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
BEGIN
  -- Initialize outputs
  amc_id := NULL;
  match_type := NULL;
  confidence := 0.0;
  
  v_norm_name := normalize_amc_name_sql(p_name);
  v_code := trim(coalesce(p_code, ''));
  
  -- PRIORITY 1: Exact code match (highest confidence)
  IF v_code <> '' THEN
    SELECT am.amc_id INTO amc_id
    FROM amc_master am
    WHERE (am.cams_amc_code IS NOT NULL AND position(v_code IN am.cams_amc_code) > 0)
       OR (am.kfin_amc_code IS NOT NULL AND position(v_code IN am.kfin_amc_code) > 0)
    LIMIT 1;
    
    IF amc_id IS NOT NULL THEN
      match_type := 'CODE_EXACT';
      confidence := 1.0;
      RETURN;
    END IF;
  END IF;
  
  -- PRIORITY 2: Exact normalized name match (very high confidence)
  IF v_norm_name <> '' THEN
    SELECT am.amc_id INTO amc_id
    FROM amc_master am
    WHERE normalize_amc_name_sql(am.amc_full_name) = v_norm_name
    LIMIT 1;
    
    IF amc_id IS NOT NULL THEN
      match_type := 'NAME_EXACT';
      confidence := 0.95;
      RETURN;
    END IF;
  END IF;
  
  -- PRIORITY 3: Fuzzy match using trigram similarity
  IF v_norm_name <> '' THEN
    SELECT 
      am.amc_id,
      similarity(normalize_amc_name_sql(am.amc_full_name), v_norm_name) as sim
    INTO v_candidate
    FROM amc_master am
    WHERE similarity(normalize_amc_name_sql(am.amc_full_name), v_norm_name) >= 0.60
    ORDER BY sim DESC
    LIMIT 1;
    
    IF v_candidate.amc_id IS NOT NULL THEN
      amc_id := v_candidate.amc_id;
      confidence := v_candidate.sim;
      
      -- Classify fuzzy match quality
      IF confidence >= 0.80 THEN
        match_type := 'NAME_FUZZY_HIGH';
      ELSIF confidence >= 0.70 THEN
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
$$;


-- 3. Main AMC ID resolution function with automatic review creation
CREATE OR REPLACE FUNCTION get_amc_id(
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
  v_review_id UUID;
  v_existing_review UUID;
BEGIN
  -- Get match result
  SELECT * INTO v_match
  FROM find_amc_match(p_name, p_code);
  
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
    AND source_amc_code = trim(coalesce(p_code, ''))
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
      trim(coalesce(p_code, '')),
      v_match.amc_id,
      (SELECT amc_full_name FROM amc_master WHERE amc_id = v_match.amc_id),
      v_match.confidence,
      v_match.match_type,
      'OPEN',
      now()
    )
    RETURNING id INTO v_review_id;
    
    RAISE NOTICE 'Created review record % for AMC: % (confidence: %)', 
      v_review_id, v_norm_name, v_match.confidence;
  ELSE
    -- Update existing review with source info if not set
    UPDATE amc_match_review
    SET 
      source_table = COALESCE(source_table, p_source_table),
      source_scheme_id = COALESCE(source_scheme_id, p_source_scheme_id)
    WHERE id = v_existing_review;
  END IF;
  
  -- Return NULL for manual review
  RETURN NULL;
END;
$$;

ALTER TABLE IF EXISTS cams_scheme_master ADD COLUMN IF NOT EXISTS amc_id UUID;
ALTER TABLE IF EXISTS kfin_scheme_master ADD COLUMN IF NOT EXISTS amc_id UUID;
ALTER TABLE IF EXISTS rta_combined_scheme_master ADD COLUMN IF NOT EXISTS amc_id UUID;
ALTER TABLE IF EXISTS scheme_master_final ADD COLUMN IF NOT EXISTS amc_id UUID;
ALTER TABLE amc_match_review ADD COLUMN IF NOT EXISTS match_type TEXT;

-- 5. Simplified backfill procedure
CREATE OR REPLACE PROCEDURE sp_backfill_amc_ids()
LANGUAGE plpgsql
AS $$
DECLARE
  r RECORD;
  v_amc_id UUID;
  v_updated_count INTEGER := 0;
BEGIN
  RAISE NOTICE 'Starting AMC ID backfill...';
  
  -- CAMS schemes
  FOR r IN 
    SELECT scheme_id, amc_name, amc_code 
    FROM cams_scheme_master 
    WHERE amc_id IS NULL 
      AND (amc_name IS NOT NULL OR amc_code IS NOT NULL)
  LOOP
    v_amc_id := get_amc_id(
      r.amc_name, 
      r.amc_code,
      'cams_scheme_master',
      r.scheme_id
    );
    
    IF v_amc_id IS NOT NULL THEN
      UPDATE cams_scheme_master 
      SET amc_id = v_amc_id, updated_at = now()
      WHERE scheme_id = r.scheme_id;
      v_updated_count := v_updated_count + 1;
    END IF;
  END LOOP;
  
  RAISE NOTICE 'Updated % CAMS records', v_updated_count;
  v_updated_count := 0;
  
  -- KFIN schemes
  FOR r IN 
    SELECT scheme_id, amc_name, amc_code 
    FROM kfin_scheme_master 
    WHERE amc_id IS NULL 
      AND (amc_name IS NOT NULL OR amc_code IS NOT NULL)
  LOOP
    v_amc_id := get_amc_id(
      r.amc_name, 
      r.amc_code,
      'kfin_scheme_master',
      r.scheme_id
    );
    
    IF v_amc_id IS NOT NULL THEN
      UPDATE kfin_scheme_master 
      SET amc_id = v_amc_id, updated_at = now()
      WHERE scheme_id = r.scheme_id;
      v_updated_count := v_updated_count + 1;
    END IF;
  END LOOP;
  
  RAISE NOTICE 'Updated % KFIN records', v_updated_count;
  v_updated_count := 0;
  
  -- RTA combined
  FOR r IN 
    SELECT scheme_id, amc_name, amc_code 
    FROM rta_combined_scheme_master 
    WHERE amc_id IS NULL 
      AND (amc_name IS NOT NULL OR amc_code IS NOT NULL)
  LOOP
    v_amc_id := get_amc_id(
      r.amc_name, 
      r.amc_code,
      'rta_combined_scheme_master',
      r.scheme_id
    );
    
    IF v_amc_id IS NOT NULL THEN
      UPDATE rta_combined_scheme_master 
      SET amc_id = v_amc_id, updated_at = now()
      WHERE scheme_id = r.scheme_id;
      v_updated_count := v_updated_count + 1;
    END IF;
  END LOOP;
  
  RAISE NOTICE 'Updated % RTA combined records', v_updated_count;
  v_updated_count := 0;
  
  -- Scheme master final
  FOR r IN 
    SELECT scheme_id, amc_name 
    FROM scheme_master_final 
    WHERE amc_id IS NULL 
      AND amc_name IS NOT NULL
  LOOP
    v_amc_id := get_amc_id(
      r.amc_name, 
      NULL,
      'scheme_master_final',
      r.scheme_id
    );
    
    IF v_amc_id IS NOT NULL THEN
      UPDATE scheme_master_final 
      SET amc_id = v_amc_id, updated_at = now()
      WHERE scheme_id = r.scheme_id;
      v_updated_count := v_updated_count + 1;
    END IF;
  END LOOP;
  
  RAISE NOTICE 'Updated % final records', v_updated_count;
  
  -- Summary
  RAISE NOTICE '=== Backfill Complete ===';
  RAISE NOTICE 'Review pending matches: SELECT * FROM amc_match_review WHERE status = ''OPEN'';';
END;
$$;

-- 6. Helper: Approve review match
CREATE OR REPLACE PROCEDURE sp_approve_amc_match(
  p_review_id UUID,
  p_approved_amc_id UUID,
  p_reviewer TEXT DEFAULT 'system'
)
LANGUAGE plpgsql
AS $$
DECLARE
  v_source_table TEXT;
  v_source_scheme_id TEXT;
BEGIN
  -- Get review details
  SELECT source_table, source_scheme_id
  INTO v_source_table, v_source_scheme_id
  FROM amc_match_review
  WHERE id = p_review_id;
  
  IF NOT FOUND THEN
    RAISE EXCEPTION 'Review record % not found', p_review_id;
  END IF;
  
  -- Update review status
  UPDATE amc_match_review
  SET 
    suggested_amc_id = p_approved_amc_id,
    status = 'APPROVED',
    reviewed_by = p_reviewer,
    reviewed_at = now()
  WHERE id = p_review_id;
  
  -- Apply to source table
  IF v_source_table = 'cams_scheme_master' THEN
    UPDATE cams_scheme_master 
    SET amc_id = p_approved_amc_id, updated_at = now()
    WHERE scheme_id = v_source_scheme_id;
    
  ELSIF v_source_table = 'kfin_scheme_master' THEN
    UPDATE kfin_scheme_master 
    SET amc_id = p_approved_amc_id, updated_at = now()
    WHERE scheme_id = v_source_scheme_id;
    
  ELSIF v_source_table = 'rta_combined_scheme_master' THEN
    UPDATE rta_combined_scheme_master 
    SET amc_id = p_approved_amc_id, updated_at = now()
    WHERE scheme_id = v_source_scheme_id;
    
  ELSIF v_source_table = 'scheme_master_final' THEN
    UPDATE scheme_master_final 
    SET amc_id = p_approved_amc_id, updated_at = now()
    WHERE scheme_id = v_source_scheme_id;
  END IF;
  
  RAISE NOTICE 'Applied AMC match to %.%', v_source_table, v_source_scheme_id;
END;
$$;


-- 7. Helper: View pending reviews with suggestions
CREATE OR REPLACE VIEW v_amc_review_pending AS
SELECT 
  r.id as review_id,
  r.source_table,
  r.source_amc_name,
  r.source_amc_code,
  r.suggested_amc_name,
  r.match_type,
  r.similarity,
  CASE 
    WHEN r.similarity >= 0.70 THEN 'Medium Confidence'
    WHEN r.similarity >= 0.60 THEN 'Low Confidence'
    ELSE 'No Match'
  END as confidence_level,
  r.created_at
FROM amc_match_review r
WHERE r.status = 'OPEN'
ORDER BY r.similarity DESC, r.created_at DESC;

-- ===================================================================
-- PART 2 — UPSERT-BASED PROCEDURES FOR CLEANED, MINIMAL SCHEMA
-- ===================================================================

-- ========================================================
-- 1) Move INVALID ISIN rows from raw → *_without_isin
-- ========================================================
CREATE OR REPLACE PROCEDURE sp_move_invalid_isins()
LANGUAGE plpgsql
AS $$
BEGIN
    -- CAMS invalid ISIN
    INSERT INTO cams_without_isin SELECT * FROM cams_raw
    WHERE NOT is_valid_isin(coalesce(isin_no,''))
    ON CONFLICT DO NOTHING;

    DELETE FROM cams_raw
    WHERE NOT is_valid_isin(coalesce(isin_no,''));

    -- KFIN invalid ISIN
    INSERT INTO kfin_without_isin SELECT * FROM kfin_raw
    WHERE NOT is_valid_isin(coalesce("ISIN Number",''))
    ON CONFLICT DO NOTHING;

    DELETE FROM kfin_raw
    WHERE NOT is_valid_isin(coalesce("ISIN Number",''));
END;
$$;

-- ===================================================================
-- 2) CAMS MASTER UPSERT (scheme_id = sf_code or sch_code or fallback)
-- ===================================================================
CREATE OR REPLACE PROCEDURE sp_refresh_cams_scheme_master()
LANGUAGE plpgsql
AS $$
DECLARE
  rec RECORD;
  v_scheme_id TEXT;
  v_amc_id UUID;
  v_option_type TEXT;
  v_nature TEXT;
  
BEGIN
  FOR rec IN
    SELECT *
    FROM cams_raw
    WHERE is_valid_isin(coalesce(isin_no,''))
  LOOP
    -- 1) Build scheme_id = amc_code + sch_code
    v_scheme_id := coalesce(rec.amc_code, '') || coalesce(rec.sch_code, '');

    -- 2) Derive option_type from div_reinv
    v_option_type := CASE
      WHEN coalesce(rec.div_reinv, '') = 'Z' THEN 'Growth'
      WHEN coalesce(rec.div_reinv, '') = 'Y' THEN 'IDCW Reinvestment'
      WHEN coalesce(rec.div_reinv, '') IN ('X','N') THEN 'IDCW Payout'
      ELSE NULL
    END;

    -- 3) Derive nature from close_end
    v_nature := CASE
      WHEN coalesce(rec.close_end,'') = 'Y' THEN 'Close Ended'
      WHEN coalesce(rec.close_end,'') = 'N' THEN 'Open Ended'
      ELSE NULL
    END;

    
     

    -- 5) Lookup canonical amc_id
    BEGIN
      v_amc_id := get_amc_id(coalesce(rec.amc, rec.amc_code, ''), coalesce(rec.amc_code,''));
    EXCEPTION WHEN OTHERS THEN
      v_amc_id := NULL;
    END;

    -- 6) UPSERT into cams_scheme_master
    INSERT INTO cams_scheme_master (
      scheme_id,
      rta_source,
      amc_code,
      amc_name,
      amc_id,
      rta_scheme_code,
      scheme_code,
      scheme_nav_code,
      scheme_name,
      plan_type,
      option_type,
      isin,
      nature,
      start_date,
      close_date,
      face_value,
      purchase_allowed,
      redemption_allowed,
      sip_flag,
      stp_in_allowed,
      stp_out_allowed,
      swp_in_allowed,
      swp_out_allowed,
      switch_in_allowed,
      switch_out_allowed,
      load_details,
      scheme_nav_name,
      imported_at,
      source_file,
      raw_payload,
      created_at,
      updated_at
    )
    VALUES (
      v_scheme_id,
      'CAMS',
      rec.amc_code,
      rec.amc,
      v_amc_id,
      coalesce(rec.sch_code, '')::text,
      coalesce(rec.sf_code, '')::text,
      coalesce(rec.parent_scheme_code, '')::text,
      coalesce(rec.sf_name, '')::text,
      rec.plan_type,
      v_option_type,
      coalesce(rec.isin_no, NULL),
      v_nature,
      rec.start_date,  -- FIXED: Already a DATE, don't cast from string
      NULL,            -- FIXED: close_date doesn't exist in cams_raw, use NULL
      rec.face_value,
      (coalesce(rec.pur_allow,'') = 'Y'),
      (coalesce(rec.red_allow,'') = 'Y'),
      (coalesce(rec.sip_allow,'') = 'Y'),
      (coalesce(rec.stp_allow,'') = 'Y'),
      (coalesce(rec.stp_allow,'') = 'Y'),
      (coalesce(rec.swp_allow,'') = 'Y'),
      (coalesce(rec.swp_allow,'') = 'Y'),
      (coalesce(rec.swt_allow,'') = 'Y'),  -- FIXED: Use swt_allow not display_data_entry
      (coalesce(rec.swt_allow,'') = 'Y'),
      NULL,            -- load_details not in cams_raw
      COALESCE(rec.sch_name ,'')::text,
      now(),
      rec.source,
      to_jsonb(rec),
      now(),
      now()
    )
    ON CONFLICT (scheme_id)
    DO UPDATE SET
      rta_source = EXCLUDED.rta_source,
      amc_code = COALESCE(EXCLUDED.amc_code, cams_scheme_master.amc_code),
      amc_name = COALESCE(EXCLUDED.amc_name, cams_scheme_master.amc_name),
      amc_id = COALESCE(EXCLUDED.amc_id, cams_scheme_master.amc_id),
      rta_scheme_code = COALESCE(EXCLUDED.rta_scheme_code, cams_scheme_master.rta_scheme_code),
      scheme_code = COALESCE(EXCLUDED.scheme_code, cams_scheme_master.scheme_code),
      scheme_nav_code = COALESCE(EXCLUDED.scheme_nav_code, cams_scheme_master.scheme_nav_code),
      scheme_name = COALESCE(EXCLUDED.scheme_name, cams_scheme_master.scheme_name),
      plan_type = COALESCE(EXCLUDED.plan_type, cams_scheme_master.plan_type),
      option_type = COALESCE(EXCLUDED.option_type, cams_scheme_master.option_type),
      isin = COALESCE(EXCLUDED.isin, cams_scheme_master.isin),
      nature = COALESCE(EXCLUDED.nature, cams_scheme_master.nature),
      start_date = COALESCE(EXCLUDED.start_date, cams_scheme_master.start_date),
      close_date = COALESCE(EXCLUDED.close_date, cams_scheme_master.close_date),
      face_value = COALESCE(EXCLUDED.face_value, cams_scheme_master.face_value),
      purchase_allowed = COALESCE(EXCLUDED.purchase_allowed, cams_scheme_master.purchase_allowed),
      redemption_allowed = COALESCE(EXCLUDED.redemption_allowed, cams_scheme_master.redemption_allowed),
      sip_flag = COALESCE(EXCLUDED.sip_flag, cams_scheme_master.sip_flag),
      stp_in_allowed = COALESCE(EXCLUDED.stp_in_allowed, cams_scheme_master.stp_in_allowed),
      stp_out_allowed = COALESCE(EXCLUDED.stp_out_allowed, cams_scheme_master.stp_out_allowed),
      swp_in_allowed = COALESCE(EXCLUDED.swp_in_allowed, cams_scheme_master.swp_in_allowed),
      swp_out_allowed = COALESCE(EXCLUDED.swp_out_allowed, cams_scheme_master.swp_out_allowed),
      switch_in_allowed = COALESCE(EXCLUDED.switch_in_allowed, cams_scheme_master.switch_in_allowed),
      switch_out_allowed = COALESCE(EXCLUDED.switch_out_allowed, cams_scheme_master.switch_out_allowed),
      load_details = COALESCE(EXCLUDED.load_details, cams_scheme_master.load_details),
      scheme_nav_name = COALESCE(EXCLUDED.scheme_nav_name, cams_scheme_master.scheme_nav_name),
      imported_at = EXCLUDED.imported_at,
      source_file = EXCLUDED.source_file,
      raw_payload = EXCLUDED.raw_payload,
      updated_at = now();

  END LOOP;
END;
$$;
-- ===================================================================
-- 2) Kfin MASTER UPSERT (scheme_id = scheme code or fallback)
-- ===================================================================
CREATE OR REPLACE PROCEDURE sp_refresh_kfin_scheme_master()
LANGUAGE plpgsql
AS $$
DECLARE
  rec RECORD;
  v_scheme_id TEXT;
  v_amc_id UUID;
  v_plan_type TEXT;
  v_option_type TEXT;
  v_nature TEXT;
  v_scheme_nav_name TEXT;
BEGIN

  FOR rec IN SELECT * FROM kfin_raw LOOP

    --------------------------------------------------------------------
    -- FIXED: Build scheme_id = Product Code + Option Code (19,567 unique)
    --------------------------------------------------------------------
    v_scheme_id := COALESCE(rec."Product Code",'') || COALESCE(rec."Option Code",'');

    --------------------------------------------------------------------
    -- Derive plan_type from "Plan Description"
    --------------------------------------------------------------------
    v_plan_type := CASE
      WHEN lower(COALESCE(rec."Plan Description",'')) IN ('reg','regular') THEN 'Regular'
      WHEN lower(COALESCE(rec."Plan Description",'')) IN ('dir','direct')  THEN 'Direct'
      WHEN rec."Plan Description" IS NULL OR rec."Plan Description" = '' THEN NULL
      ELSE 'Other'
    END;

    --------------------------------------------------------------------
    -- Derive option_type from "Option Code"
    --------------------------------------------------------------------
    v_option_type := CASE
      WHEN rec."Option Code" = 'G' THEN 'Growth'
      WHEN rec."Option Code" = 'R' THEN 'IDCW Reinvestment'
      WHEN rec."Option Code" IN ('D','M','P') THEN 'IDCW Payout'
      WHEN rec."Option Code" = 'B' THEN 'Bonus'
      ELSE NULL
    END;

    --------------------------------------------------------------------
    -- Derive nature from "Nature"
    --------------------------------------------------------------------
    v_nature := CASE
      WHEN lower(COALESCE(rec."Nature",'')) = 'open ended' THEN 'Open Ended'
      WHEN lower(COALESCE(rec."Nature",'')) = 'close ended' THEN 'Close Ended'
      ELSE rec."Nature"
    END;

    --------------------------------------------------------------------
    -- scheme_nav_name = Scheme Description + Plan + Option
    --------------------------------------------------------------------
    v_scheme_nav_name :=
        COALESCE(rec."Scheme Description",'')
        || CASE WHEN v_plan_type IS NOT NULL THEN '-' || v_plan_type ELSE '' END
        || CASE WHEN v_option_type IS NOT NULL THEN '-' || v_option_type ELSE '' END;

    --------------------------------------------------------------------
    -- Lookup AMC-ID
    --------------------------------------------------------------------
    BEGIN
      v_amc_id := get_amc_id(
                      COALESCE(rec."AMC Name",''),
                      COALESCE(rec."AMC Code",'')
                  );
    EXCEPTION WHEN OTHERS THEN
      v_amc_id := NULL;
    END;

    --------------------------------------------------------------------
    -- UPSERT INTO kfin_scheme_master
    --------------------------------------------------------------------
    INSERT INTO kfin_scheme_master (
      scheme_id,
      rta_source,
      amc_code,
      amc_name,
      amc_id,
      rta_scheme_code,
      scheme_code,
      scheme_nav_code,
      scheme_name,
      plan_type,
      option_type,
      isin,
      nature,
      start_date,
      close_date,
      face_value,
      purchase_allowed,
      redemption_allowed,
      sip_flag,
      stp_in_allowed,
      stp_out_allowed,
      swp_in_allowed,
      swp_out_allowed,
      switch_in_allowed,
      switch_out_allowed,
      load_details,
      scheme_nav_name,
      imported_at,
      source_file,
      raw_payload,
      created_at,
      updated_at
    )
    VALUES (
      v_scheme_id,
      'KFIN',
      rec."AMC Code",
      rec."AMC Name",
      v_amc_id,

      -- rta_scheme_code = Scheme Code + Plan Code
      COALESCE(rec."Scheme Code",'') || COALESCE(rec."Plan Code",''),

      -- scheme_code = Scheme Code
      COALESCE(rec."Scheme Code",''),

      -- scheme_nav_code = Product Code (the real unique identifier)
      COALESCE(rec."Product Code",''),

      -- scheme_name = Scheme Description
      COALESCE(rec."Scheme Description",''),

      v_plan_type,
      v_option_type,
      rec."ISIN Number",
      v_nature,

      rec."NFO Start Date",
      CASE 
        WHEN rec."Close Date" IS NOT NULL AND rec."Close Date" <> '' 
        THEN rec."Close Date"::date 
        ELSE NULL 
      END,
      rec."NFO Face Value",

      (COALESCE(rec."Purchased Allowed",'') = 'Y'),
      (COALESCE(rec."Redemption Allowed",'') = 'Y'),
      (COALESCE(rec."SIP Allowed",'') = 'Y'),
      (COALESCE(rec."STP In Allowed",'') = 'Y'),
      (COALESCE(rec."STP Out Allowed",'') = 'Y'),
      (COALESCE(rec."SWP In Allowed",'') = 'Y'),
      (COALESCE(rec."SWP Out Allowed",'') = 'Y'),
      (COALESCE(rec."Switch In Allowed",'') = 'Y'),
      (COALESCE(rec."Switch Out Allowed",'') = 'Y'),

      rec."Load Details",
      v_scheme_nav_name,

      now(),
      rec.source,
      to_jsonb(rec),
      now(),
      now()
    )

    ON CONFLICT (scheme_id)
    DO UPDATE SET
      amc_code = COALESCE(EXCLUDED.amc_code, kfin_scheme_master.amc_code),
      amc_name = COALESCE(EXCLUDED.amc_name, kfin_scheme_master.amc_name),
      amc_id   = COALESCE(EXCLUDED.amc_id, kfin_scheme_master.amc_id),

      rta_scheme_code = COALESCE(EXCLUDED.rta_scheme_code, kfin_scheme_master.rta_scheme_code),
      scheme_code = COALESCE(EXCLUDED.scheme_code, kfin_scheme_master.scheme_code),
      scheme_nav_code = COALESCE(EXCLUDED.scheme_nav_code, kfin_scheme_master.scheme_nav_code),
      scheme_name = COALESCE(EXCLUDED.scheme_name, kfin_scheme_master.scheme_name),

      plan_type = COALESCE(EXCLUDED.plan_type, kfin_scheme_master.plan_type),
      option_type = COALESCE(EXCLUDED.option_type, kfin_scheme_master.option_type),
      isin = COALESCE(EXCLUDED.isin, kfin_scheme_master.isin),
      nature = COALESCE(EXCLUDED.nature, kfin_scheme_master.nature),

      start_date = COALESCE(EXCLUDED.start_date, kfin_scheme_master.start_date),
      close_date = COALESCE(EXCLUDED.close_date, kfin_scheme_master.close_date),
      face_value = COALESCE(EXCLUDED.face_value, kfin_scheme_master.face_value),

      purchase_allowed = COALESCE(EXCLUDED.purchase_allowed, kfin_scheme_master.purchase_allowed),
      redemption_allowed = COALESCE(EXCLUDED.redemption_allowed, kfin_scheme_master.redemption_allowed),
      sip_flag = COALESCE(EXCLUDED.sip_flag, kfin_scheme_master.sip_flag),
      stp_in_allowed = COALESCE(EXCLUDED.stp_in_allowed, kfin_scheme_master.stp_in_allowed),
      stp_out_allowed = COALESCE(EXCLUDED.stp_out_allowed, kfin_scheme_master.stp_out_allowed),
      swp_in_allowed = COALESCE(EXCLUDED.swp_in_allowed, kfin_scheme_master.swp_in_allowed),
      swp_out_allowed = COALESCE(EXCLUDED.swp_out_allowed, kfin_scheme_master.swp_out_allowed),
      switch_in_allowed = COALESCE(EXCLUDED.switch_in_allowed, kfin_scheme_master.switch_in_allowed),
      switch_out_allowed = COALESCE(EXCLUDED.switch_out_allowed, kfin_scheme_master.switch_out_allowed),

      load_details = COALESCE(EXCLUDED.load_details, kfin_scheme_master.load_details),
      scheme_nav_name = COALESCE(EXCLUDED.scheme_nav_name, kfin_scheme_master.scheme_nav_name),

      imported_at = EXCLUDED.imported_at,
      source_file = EXCLUDED.source_file,
      raw_payload = EXCLUDED.raw_payload,
      updated_at = now();

  END LOOP;

END;
$$;


CREATE OR REPLACE PROCEDURE sp_refresh_rta_combined_scheme_master()
LANGUAGE plpgsql
AS $$
DECLARE
    rec RECORD;
BEGIN
    ----------------------------------------------------------------------
    -- 1. Refresh from CAMS first (CAMS has priority when duplicates exist)
    ----------------------------------------------------------------------
    FOR rec IN 
        SELECT *
        FROM cams_scheme_master
    LOOP
        INSERT INTO rta_combined_scheme_master (
            scheme_id,
            rta_source,
            amc_id,
            amc_code,
            amc_name,

            rta_scheme_code,
            scheme_code,
            scheme_nav_code,
            scheme_name,
            plan_type,
            option_type,
            isin,
            nature,
            start_date,
            close_date,
            face_value,

            purchase_allowed,
            redemption_allowed,
            sip_flag,
            stp_in_allowed,
            stp_out_allowed,
            swp_in_allowed,
            swp_out_allowed,
            switch_in_allowed,
            switch_out_allowed,
            load_details,

            imported_at,
            scheme_nav_name,
            source_file,
            raw_payload,
            created_at,
            updated_at
        )
        VALUES (
            rec.scheme_id,
            'CAMS',
            rec.amc_id,
            rec.amc_code,
            rec.amc_name,

            rec.rta_scheme_code,
            rec.scheme_code,
            rec.scheme_nav_code,
            rec.scheme_name,
            rec.plan_type,
            rec.option_type,
            rec.isin,
            rec.nature,
            rec.start_date,
            rec.close_date,
            rec.face_value,

            rec.purchase_allowed,
            rec.redemption_allowed,
            rec.sip_flag,
            rec.stp_in_allowed,
            rec.stp_out_allowed,
            rec.swp_in_allowed,
            rec.swp_out_allowed,
            rec.switch_in_allowed,
            rec.switch_out_allowed,
            rec.load_details,

            rec.imported_at,
            rec.scheme_nav_name,
            rec.source_file,
            rec.raw_payload,
            rec.created_at,
            now()
        )

        ON CONFLICT (scheme_id)
        DO UPDATE SET
            rta_source = 'CAMS',
            amc_id = COALESCE(EXCLUDED.amc_id, rta_combined_scheme_master.amc_id),
            amc_code = COALESCE(EXCLUDED.amc_code, rta_combined_scheme_master.amc_code),
            amc_name = COALESCE(EXCLUDED.amc_name, rta_combined_scheme_master.amc_name),

            rta_scheme_code = COALESCE(EXCLUDED.rta_scheme_code, rta_combined_scheme_master.rta_scheme_code),
            scheme_code = COALESCE(EXCLUDED.scheme_code, rta_combined_scheme_master.scheme_code),
            scheme_nav_code = COALESCE(EXCLUDED.scheme_nav_code, rta_combined_scheme_master.scheme_nav_code),
            scheme_name = COALESCE(EXCLUDED.scheme_name, rta_combined_scheme_master.scheme_name),

            plan_type = COALESCE(EXCLUDED.plan_type, rta_combined_scheme_master.plan_type),
            option_type = COALESCE(EXCLUDED.option_type, rta_combined_scheme_master.option_type),
            isin = COALESCE(EXCLUDED.isin, rta_combined_scheme_master.isin),
            nature = COALESCE(EXCLUDED.nature, rta_combined_scheme_master.nature),

            start_date = COALESCE(EXCLUDED.start_date, rta_combined_scheme_master.start_date),
            close_date = COALESCE(EXCLUDED.close_date, rta_combined_scheme_master.close_date),
            face_value = COALESCE(EXCLUDED.face_value, rta_combined_scheme_master.face_value),

            purchase_allowed = COALESCE(EXCLUDED.purchase_allowed, rta_combined_scheme_master.purchase_allowed),
            redemption_allowed = COALESCE(EXCLUDED.redemption_allowed, rta_combined_scheme_master.redemption_allowed),
            sip_flag = COALESCE(EXCLUDED.sip_flag, rta_combined_scheme_master.sip_flag),
            stp_in_allowed = COALESCE(EXCLUDED.stp_in_allowed, rta_combined_scheme_master.stp_in_allowed),
            stp_out_allowed = COALESCE(EXCLUDED.stp_out_allowed, rta_combined_scheme_master.stp_out_allowed),
            swp_in_allowed = COALESCE(EXCLUDED.swp_in_allowed, rta_combined_scheme_master.swp_in_allowed),
            swp_out_allowed = COALESCE(EXCLUDED.swp_out_allowed, rta_combined_scheme_master.swp_out_allowed),
            switch_in_allowed = COALESCE(EXCLUDED.switch_in_allowed, rta_combined_scheme_master.switch_in_allowed),
            switch_out_allowed = COALESCE(EXCLUDED.switch_out_allowed, rta_combined_scheme_master.switch_out_allowed),

            load_details = COALESCE(EXCLUDED.load_details, rta_combined_scheme_master.load_details),
            imported_at = EXCLUDED.imported_at,
            scheme_nav_name = COALESCE(EXCLUDED.scheme_nav_name, rta_combined_scheme_master.scheme_nav_name),
            source_file = COALESCE(EXCLUDED.source_file, rta_combined_scheme_master.source_file),
            raw_payload = COALESCE(EXCLUDED.raw_payload, rta_combined_scheme_master.raw_payload),
            updated_at = now();
    END LOOP;


    ----------------------------------------------------------------------
    -- 2. Refresh from KFIN (only insert where CAMS did not override)
    ----------------------------------------------------------------------
    FOR rec IN
        SELECT *
        FROM kfin_scheme_master
    LOOP

        INSERT INTO rta_combined_scheme_master (
            scheme_id,
            rta_source,
            amc_id,
            amc_code,
            amc_name,

            rta_scheme_code,
            scheme_code,
            scheme_nav_code,
            scheme_name,
            plan_type,
            option_type,
            isin,
            nature,
            start_date,
            close_date,
            face_value,

            purchase_allowed,
            redemption_allowed,
            sip_flag,
            stp_in_allowed,
            stp_out_allowed,
            swp_in_allowed,
            swp_out_allowed,
            switch_in_allowed,
            switch_out_allowed,
            load_details,

            imported_at,
            scheme_nav_name,
            source_file,
            raw_payload,
            created_at,
            updated_at
        )
        VALUES (
            rec.scheme_id,
            'KFIN',
            rec.amc_id,
            rec.amc_code,
            rec.amc_name,

            rec.rta_scheme_code,
            rec.scheme_code,
            rec.scheme_nav_code,
            rec.scheme_name,
            rec.plan_type,
            rec.option_type,
            rec.isin,
            rec.nature,
            rec.start_date,
            rec.close_date,
            rec.face_value,

            rec.purchase_allowed,
            rec.redemption_allowed,
            rec.sip_flag,
            rec.stp_in_allowed,
            rec.stp_out_allowed,
            rec.swp_in_allowed,
            rec.swp_out_allowed,
            rec.switch_in_allowed,
            rec.switch_out_allowed,
            rec.load_details,

            rec.imported_at,
            rec.scheme_nav_name,
            rec.source_file,
            rec.raw_payload,
            now(),
            now()
        )

        ON CONFLICT (scheme_id)
        DO NOTHING;  -- CAMS always wins when both exist

    END LOOP;

END;
$$;


CREATE OR REPLACE PROCEDURE sp_refresh_amfi_scheme()
LANGUAGE plpgsql
AS $$
DECLARE
    rec RECORD;
    v_amfi_code TEXT;
    v_amc_name TEXT;
    v_scheme_name TEXT;
    v_category TEXT;
    v_sub_category TEXT;
    v_isin_field TEXT;
    v_isin_clean TEXT;
    v_isin TEXT;
    v_isin_count INTEGER;
    v_pos INTEGER;
    v_nav_name TEXT;
    v_inserted_count INTEGER := 0;
    v_skipped_invalid INTEGER := 0;
    v_skipped_blank INTEGER := 0;
BEGIN

    FOR rec IN SELECT * FROM amfi_raw LOOP

        -- Convert NUMERIC to TEXT properly
        v_amfi_code := CASE 
            WHEN rec."Code" IS NULL THEN ''
            ELSE rec."Code"::TEXT
        END;

        v_scheme_name := COALESCE(rec."Scheme Name",'');
        v_amc_name := COALESCE(rec."AMC",'');

        -- Split category → category / sub-category
        IF rec."Scheme Category" IS NULL OR rec."Scheme Category" = '' THEN
            v_category := NULL;
            v_sub_category := NULL;
        ELSE
            IF POSITION('-' IN rec."Scheme Category") > 0 THEN
                v_category := TRIM(SPLIT_PART(rec."Scheme Category", '-', 1));
                v_sub_category := TRIM(SPLIT_PART(rec."Scheme Category", '-', 2));
            ELSE
                v_category := TRIM(rec."Scheme Category");
                v_sub_category := NULL;
            END IF;
        END IF;

        v_nav_name := COALESCE(rec."Scheme NAV Name",'');

        -- Get ISIN field and clean whitespace
        v_isin_field := REGEXP_REPLACE(
            COALESCE(rec."ISIN Div Payout/ISIN GrowthISIN Div Reinvestment", ''),
            '\s+', '', 'g'
        );

        -- Skip if blank ISIN
        IF v_isin_field IS NULL OR v_isin_field = '' THEN
            v_skipped_blank := v_skipped_blank + 1;
            CONTINUE;
        END IF;

        -- Clean the ISIN field (remove any slashes that might exist)
        v_isin_clean := REPLACE(v_isin_field, '/', '');

        -- Calculate how many ISINs (each ISIN is exactly 12 characters)
        v_isin_count := LENGTH(v_isin_clean) / 12;

        -- Parse each 12-character ISIN
        FOR i IN 1..v_isin_count LOOP
            v_pos := ((i - 1) * 12) + 1;
            v_isin := UPPER(SUBSTRING(v_isin_clean FROM v_pos FOR 12));

            -- Validate ISIN format
            IF v_isin !~ '^IN[A-Z0-9]{10}$' THEN
                v_skipped_invalid := v_skipped_invalid + 1;
                CONTINUE;
            END IF;

            -- Insert into amfi_scheme
            BEGIN
                INSERT INTO amfi_scheme (
                    amfi_code,
                    amc_name,
                    scheme_name,
                    scheme_type,
                    category,
                    sub_category,
                    scheme_nav_name,
                    scheme_min_amount,
                    launch_date,
                    closure_date,
                    isin_single,
                    isin_all,
                    imported_at,
                    source_file,
                    raw_payload,
                    created_at,
                    updated_at
                )
                VALUES (
                    v_amfi_code,
                    v_amc_name,
                    v_scheme_name,
                    rec."Scheme Type",
                    v_category,
                    v_sub_category,
                    v_nav_name,
                    NULLIF(rec."Scheme Minimum Amount",'')::NUMERIC,
                    rec."Launch Date",
                    rec."Closure Date",
                    v_isin,
                    rec."ISIN Div Payout/ISIN GrowthISIN Div Reinvestment",
                    NOW(),
                    rec.source_file,
                    rec.raw_payload,
                    NOW(),
                    NOW()
                )
                ON CONFLICT (amfi_code, isin_single)
                DO UPDATE SET
                    scheme_name = COALESCE(EXCLUDED.scheme_name, amfi_scheme.scheme_name),
                    amc_name = COALESCE(EXCLUDED.amc_name, amfi_scheme.amc_name),
                    scheme_type = COALESCE(EXCLUDED.scheme_type, amfi_scheme.scheme_type),
                    category = COALESCE(EXCLUDED.category, amfi_scheme.category),
                    sub_category = COALESCE(EXCLUDED.sub_category, amfi_scheme.sub_category),
                    scheme_nav_name = COALESCE(EXCLUDED.scheme_nav_name, amfi_scheme.scheme_nav_name),
                    scheme_min_amount = COALESCE(EXCLUDED.scheme_min_amount, amfi_scheme.scheme_min_amount),
                    launch_date = COALESCE(EXCLUDED.launch_date, amfi_scheme.launch_date),
                    closure_date = COALESCE(EXCLUDED.closure_date, amfi_scheme.closure_date),
                    isin_all = COALESCE(EXCLUDED.isin_all, amfi_scheme.isin_all),
                    imported_at = NOW(),
                    source_file = EXCLUDED.source_file,
                    raw_payload = EXCLUDED.raw_payload,
                    updated_at = NOW();

                v_inserted_count := v_inserted_count + 1;

            EXCEPTION WHEN OTHERS THEN
                RAISE NOTICE 'Failed to insert ISIN % for scheme %: %', v_isin, v_amfi_code, SQLERRM;
                CONTINUE;
            END;

        END LOOP;

    END LOOP;

    RAISE NOTICE '✅ AMFI processing complete:';
    RAISE NOTICE '   - Inserted/Updated: % scheme-ISIN combinations', v_inserted_count;
    RAISE NOTICE '   - Skipped (blank ISIN): % schemes', v_skipped_blank;
    RAISE NOTICE '   - Skipped (invalid ISIN): % ISINs', v_skipped_invalid;

END;
$$;

CREATE OR REPLACE PROCEDURE sp_refresh_scheme_mapping()
LANGUAGE plpgsql
AS $$
DECLARE
    rta RECORD;
    fuzzy RECORD;
    v_score NUMERIC;
BEGIN

    DELETE FROM scheme_mapping WHERE verified_by IS NULL;

    FOR rta IN SELECT * FROM rta_combined_scheme_master LOOP

        ------------------------------------------------------------------
        -- STEP 1: ISIN Match (Highest Priority)
        ------------------------------------------------------------------
        IF rta.isin IS NOT NULL THEN
            INSERT INTO scheme_mapping (
                rta_id, rta_scheme_id, amfi_id, amfi_code,
                rta_source, rta_scheme_code, rta_scheme_nav_name, amfi_scheme_nav_name,
                isin_match, name_match_score, match_confidence, mapping_source,
                created_at, updated_at
            )
            SELECT
                rta.id, rta.scheme_id, a.id, a.amfi_code,
                rta.rta_source, rta.rta_scheme_code, rta.scheme_nav_name, a.scheme_nav_name,
                TRUE, 1.0, 100, 'AUTO_ISIN',
                now(), now()
            FROM amfi_scheme a
            WHERE a.isin_single = rta.isin
            LIMIT 1;

            IF FOUND THEN
                CONTINUE;
            END IF;
        END IF;

        ------------------------------------------------------------------
        -- STEP 2: Exact Name Match + ISIN Validation
        ------------------------------------------------------------------
        INSERT INTO scheme_mapping (
            rta_id, rta_scheme_id, amfi_id, amfi_code,
            rta_source, rta_scheme_code, rta_scheme_nav_name, amfi_scheme_nav_name,
            isin_match, name_match_score, match_confidence, mapping_source,
            created_at, updated_at
        )
        SELECT
            rta.id, rta.scheme_id, a.id, a.amfi_code,
            rta.rta_source, rta.rta_scheme_code, rta.scheme_nav_name, a.scheme_nav_name,
            FALSE, 1.0, 90, 'AUTO_NAME_EXACT',
            now(), now()
        FROM amfi_scheme a
        WHERE lower(a.scheme_nav_name) = lower(rta.scheme_nav_name)
          -- CRITICAL FIX: Only match if ISINs align or one is missing
          AND (
              rta.isin IS NULL                          -- RTA has no ISIN
              OR a.isin_single IS NULL                  -- AMFI has no ISIN
              OR rta.isin = a.isin_single               -- ISINs match
          )
        LIMIT 1;

        IF FOUND THEN
            CONTINUE;
        END IF;

        ------------------------------------------------------------------
        -- STEP 3: Fuzzy Name Match + ISIN Validation
        ------------------------------------------------------------------
        SELECT 
            a.id,
            a.amfi_code,
            a.scheme_name,
            similarity(a.scheme_nav_name, rta.scheme_nav_name) AS sim
        INTO fuzzy
        FROM amfi_scheme a
        WHERE similarity(a.scheme_nav_name, rta.scheme_nav_name) >= 0.60
          -- CRITICAL FIX: Only match if ISINs align or one is missing
          AND (
              rta.isin IS NULL                          -- RTA has no ISIN
              OR a.isin_single IS NULL                  -- AMFI has no ISIN
              OR rta.isin = a.isin_single               -- ISINs match
          )
        ORDER BY similarity(a.scheme_nav_name, rta.scheme_nav_name) DESC
        LIMIT 1;

        IF FOUND THEN
            v_score := fuzzy.sim;

            IF v_score >= 0.60 THEN
                INSERT INTO scheme_mapping (
                    rta_id, rta_scheme_id, amfi_id, amfi_code,
                    rta_source, rta_scheme_code, rta_scheme_nav_name, amfi_scheme_nav_name,
                    isin_match, name_match_score, match_confidence, mapping_source,
                    created_at, updated_at
                )
                VALUES (
                    rta.id, rta.scheme_id,
                    fuzzy.id, fuzzy.amfi_code,
                    rta.rta_source, rta.rta_scheme_code, rta.scheme_nav_name, fuzzy.scheme_nav_name,
                    FALSE, v_score,
                    CASE 
                        WHEN v_score >= 0.85 THEN 85
                        WHEN v_score >= 0.75 THEN 75
                        WHEN v_score >= 0.65 THEN 65
                        ELSE 60
                    END,
                    'AUTO_NAME_FUZZY',
                    now(), now()
                );
                CONTINUE;
            END IF;
        END IF;

        ------------------------------------------------------------------
        -- STEP 4: No Match Found
        ------------------------------------------------------------------
        INSERT INTO scheme_mapping (
            rta_id, rta_scheme_id,
            rta_source, rta_scheme_code, rta_scheme_nav_name,
            isin_match, name_match_score, match_confidence, mapping_source,
            created_at, updated_at
        )
        VALUES (
            rta.id, rta.scheme_id,
            rta.rta_source, rta.rta_scheme_code, rta.scheme_nav_name,
            FALSE, 0, 0, 'UNMATCHED',
            now(), now()
        );

    END LOOP;

END;
$$;

CREATE OR REPLACE PROCEDURE sp_refresh_scheme_master_final()
LANGUAGE plpgsql
AS $$
DECLARE
    rta   RECORD;
    m     RECORD;
    amfi  RECORD;
    best_amfi RECORD;
    v_category TEXT;
    v_sub_category TEXT;
    v_isin TEXT;
    v_best_conf INTEGER;
BEGIN

    ------------------------------------------------------------------
    -- 1. Clear existing auto-generated final rows
    -- (keeps room for future manual override logic, if needed)
    ------------------------------------------------------------------
    DELETE FROM scheme_master_final;

    ------------------------------------------------------------------
    -- 2. Loop through RTA-combined schemes
    ------------------------------------------------------------------
    FOR rta IN
        SELECT *
        FROM rta_combined_scheme_master
    LOOP

        v_best_conf := -1;
        best_amfi := NULL;

        ------------------------------------------------------------------
        -- Fetch all AMFI matches for this scheme
        ------------------------------------------------------------------
        FOR m IN
            SELECT *
            FROM scheme_mapping
            WHERE rta_scheme_id = rta.scheme_id
        LOOP
            ------------------------------------------------------------------
            -- Get corresponding AMFI row for this mapping record
            ------------------------------------------------------------------
            IF m.amfi_id IS NOT NULL THEN
                SELECT *
                INTO amfi
                FROM amfi_scheme
                WHERE id = m.amfi_id;

                IF FOUND THEN
                    -- Pick best AMFI by match_confidence
                    IF m.match_confidence > v_best_conf THEN
                        v_best_conf := m.match_confidence;
                        best_amfi := amfi;
                    END IF;
                END IF;
            END IF;
        END LOOP;

        ------------------------------------------------------------------
        -- Extract cat/subcat from best AMFI (if exists)
        ------------------------------------------------------------------
        IF best_amfi IS NOT NULL THEN
            v_category := best_amfi.category;
            v_sub_category := best_amfi.sub_category;
            v_isin := best_amfi.isin_single;
        ELSE
            v_category := NULL;
            v_sub_category := NULL;
            v_isin := rta.isin;    -- fallback only
        END IF;

        ------------------------------------------------------------------
        -- Insert final canonical record
        ------------------------------------------------------------------
        INSERT INTO scheme_master_final (
            scheme_id,
            canonical_scheme_code,
            canonical_scheme_name,
            amc_name,
            amc_id,
            isin,
            category,
            sub_category,
            plan_type,
            option_type,

            rta_sources,
            amfi_matches,
            rta_raw,
            amfi_raw,

            latest_nav,
            latest_nav_date,
            latest_aaum,
            created_at,
            updated_at
        )
        VALUES (
            rta.scheme_id,

            -- canonical code = scheme_code if exists else scheme_id
            COALESCE(rta.scheme_code, rta.scheme_id),

            -- canonical name = RTA scheme_name
            rta.scheme_name,

            rta.amc_name,
            rta.amc_id,

            v_isin,

            v_category,
            v_sub_category,

            rta.plan_type,
            rta.option_type,

            ------------------------------------------------------------------
            -- rta_sources JSON array
            ------------------------------------------------------------------
            jsonb_build_array(
                jsonb_build_object(
                    'rta_source', rta.rta_source,
                    'scheme_code', rta.scheme_code,
                    'rta_scheme_code', rta.rta_scheme_code,
                    'scheme_nav_code', rta.scheme_nav_code,
                    'start_date', rta.start_date,
                    'close_date', rta.close_date,
                    'face_value', rta.face_value,
                    'purchase_allowed', rta.purchase_allowed,
                    'redemption_allowed', rta.redemption_allowed,
                    'sip_flag', rta.sip_flag,
                    'stp_in_allowed', rta.stp_in_allowed,
                    'stp_out_allowed', rta.stp_out_allowed,
                    'swp_in_allowed', rta.swp_in_allowed,
                    'swp_out_allowed', rta.swp_out_allowed,
                    'switch_in_allowed', rta.switch_in_allowed,
                    'switch_out_allowed', rta.switch_out_allowed,
                    'nature', rta.nature
                )
            ),

            ------------------------------------------------------------------
            -- amfi_matches JSON array (can contain 0, 1, or multiple)
            ------------------------------------------------------------------
            (
                SELECT jsonb_agg(
                    jsonb_build_object(
                        'amfi_id', a.id,
                        'amfi_code', a.amfi_code,
                        'scheme_name', a.scheme_name,
                        'category', a.category,
                        'sub_category', a.sub_category,
                        'isin', a.isin_single
                    )
                )
                FROM amfi_scheme a
                JOIN scheme_mapping sm ON sm.amfi_id = a.id
                WHERE sm.rta_scheme_id = rta.scheme_id
            ),

            ------------------------------------------------------------------
            -- rta_raw
            ------------------------------------------------------------------
            rta.raw_payload,

            ------------------------------------------------------------------
            -- amfi_raw
            ------------------------------------------------------------------
            CASE
                WHEN best_amfi IS NOT NULL THEN best_amfi.raw_payload
                ELSE NULL
            END,

            NULL,        -- latest_nav to be filled by NAV ETL
            NULL,        -- latest_nav_date
            NULL,        -- latest_aaum
            now(),
            now()
        );

    END LOOP;

END;
$$;
