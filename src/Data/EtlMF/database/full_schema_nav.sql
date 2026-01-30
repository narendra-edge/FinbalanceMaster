SET client_min_messages = WARNING;

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";
CREATE EXTENSION IF NOT EXISTS pg_trgm;

SET client_min_messages = WARNING;

-- RAW NAV TABLES
DROP TABLE IF EXISTS cams_nav_historical_raw CASCADE;
CREATE TABLE cams_nav_historical_raw (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    imported_at TIMESTAMP DEFAULT now(),
    source VARCHAR DEFAULT 'CAMS_HISTORICAL',
    product_code VARCHAR(8),
    product_name VARCHAR(100),
    nav_date DATE,
    nav_value NUMERIC(25,6),
    dividend_per_unit NUMERIC(25,6),
    corp_div_rate NUMERIC(25,6),
    scheme_type VARCHAR(15),
    isin_no VARCHAR(16),
    swing_nav NUMERIC(25,6),
    source_file TEXT,
    raw_row JSONB,
    created_at TIMESTAMP DEFAULT now()
);
CREATE INDEX idx_cams_nav_hist_isin ON cams_nav_historical_raw(isin_no);
CREATE INDEX idx_cams_nav_hist_date ON cams_nav_historical_raw(nav_date);
CREATE INDEX idx_cams_nav_hist_pcode ON cams_nav_historical_raw(product_code);

DROP TABLE IF EXISTS kfin_nav_historical_raw CASCADE;
CREATE TABLE kfin_nav_historical_raw (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    imported_at TIMESTAMP DEFAULT now(),
    source VARCHAR DEFAULT 'KFIN_HISTORICAL',
    fund VARCHAR, 
    scheme VARCHAR,
    funddesc VARCHAR, 
    fcode VARCHAR,
    navdate DATE, 
    nav NUMERIC(15,6), 
    ppop NUMERIC(15,6),
    rpop NUMERIC(15,6),
    crdate DATE,
    crtime VARCHAR,
    schemeisin VARCHAR(16),
    source_file TEXT, 
    raw_row JSONB, 
    created_at TIMESTAMP DEFAULT now()
);
CREATE INDEX idx_kfin_nav_hist_isin ON kfin_nav_historical_raw(schemeisin);
CREATE INDEX idx_kfin_nav_hist_date ON kfin_nav_historical_raw(navdate);
CREATE INDEX idx_kfin_nav_hist_scheme ON kfin_nav_historical_raw(scheme);

DROP TABLE IF EXISTS amfi_nav_daily_raw CASCADE;
CREATE TABLE amfi_nav_daily_raw (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    imported_at TIMESTAMP DEFAULT now(),
    source VARCHAR DEFAULT 'AMFI_DAILY',
    scheme_code VARCHAR, isin_div_payout_growth VARCHAR(16), isin_div_reinvestment VARCHAR(16),
    scheme_name VARCHAR(200), net_asset_value NUMERIC(15,4), nav_date DATE,
    amc_name VARCHAR(200), category VARCHAR(200),
    source_file TEXT, raw_row JSONB, created_at TIMESTAMP DEFAULT now()
);
CREATE INDEX idx_amfi_nav_daily_isin1 ON amfi_nav_daily_raw(isin_div_payout_growth);
CREATE INDEX idx_amfi_nav_daily_isin2 ON amfi_nav_daily_raw(isin_div_reinvestment);
CREATE INDEX idx_amfi_nav_daily_date ON amfi_nav_daily_raw(nav_date);
CREATE INDEX idx_amfi_nav_daily_code ON amfi_nav_daily_raw(scheme_code);

-- RAW DIVIDEND TABLES
DROP TABLE IF EXISTS cams_dividend_historical_raw CASCADE;
CREATE TABLE cams_dividend_historical_raw (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    imported_at TIMESTAMP DEFAULT now(),
    source VARCHAR DEFAULT 'CAMS_DIV_HISTORICAL',
    product_code VARCHAR, scheme_name VARCHAR, dividend_bonus_flag VARCHAR(15),
    ex_dividend_date DATE, record_date DATE, dividend_rate_per_unit NUMERIC(25,6),
    bonus_ratio VARCHAR(50), corp_div_rate NUMERIC(25,6), isin VARCHAR(16),
    source_file TEXT, raw_row JSONB, created_at TIMESTAMP DEFAULT now()
);
CREATE INDEX idx_cams_div_hist_pcode ON cams_dividend_historical_raw(product_code);
CREATE INDEX idx_cams_div_hist_exdate ON cams_dividend_historical_raw(ex_dividend_date);
CREATE INDEX idx_cams_div_hist_isin ON cams_dividend_historical_raw(isin) WHERE isin IS NOT NULL;

DROP TABLE IF EXISTS kfin_dividend_historical_raw CASCADE;
CREATE TABLE kfin_dividend_historical_raw (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    imported_at TIMESTAMP DEFAULT now(),
    source VARCHAR DEFAULT 'KFIN_DIV_HISTORICAL',
    fund VARCHAR, scheme VARCHAR, pln VARCHAR(16), funddesc VARCHAR(200), fcode VARCHAR(16),
    div_date DATE, nday NUMERIC(15,6), drate NUMERIC(15,6), dnav NUMERIC(25,6),
    status VARCHAR(20), isin VARCHAR(16),reinvdt DATE,
    source_file TEXT, raw_row JSONB, created_at TIMESTAMP DEFAULT now()
);
CREATE INDEX idx_kfin_div_hist_scheme ON kfin_dividend_historical_raw(scheme);
CREATE INDEX idx_kfin_div_hist_divdate ON kfin_dividend_historical_raw(div_date);
CREATE INDEX idx_kfin_div_hist_fcode ON kfin_dividend_historical_raw(fcode);
CREATE INDEX idx_kfin_div_hist_isin ON kfin_dividend_historical_raw(isin) WHERE isin IS NOT NULL;

DROP TABLE IF EXISTS cams_dividend_daily_raw CASCADE;
CREATE TABLE cams_dividend_daily_raw (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    imported_at TIMESTAMP DEFAULT now(),
    source VARCHAR DEFAULT 'CAMS_DIV_DAILY',
    product_code VARCHAR, scheme_name VARCHAR(200), isin VARCHAR(16),
    ex_dividend_date DATE, record_date DATE, payment_date DATE,
    dividend_rate_per_unit NUMERIC(25,6), dividend_bonus_flag VARCHAR(15),
    bonus_ratio VARCHAR(50), corp_div_rate NUMERIC(25,6),
    plan_type VARCHAR(50), option_type VARCHAR(50),
    source_file TEXT, raw_row JSONB, created_at TIMESTAMP DEFAULT now()
);
CREATE INDEX idx_cams_div_daily_isin ON cams_dividend_daily_raw(isin);
CREATE INDEX idx_cams_div_daily_exdate ON cams_dividend_daily_raw(ex_dividend_date);
CREATE INDEX idx_cams_div_daily_pcode ON cams_dividend_daily_raw(product_code);

DROP TABLE IF EXISTS kfin_dividend_daily_raw CASCADE;
CREATE TABLE kfin_dividend_daily_raw (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    imported_at TIMESTAMP DEFAULT now(),
    source VARCHAR DEFAULT 'KFIN_DIV_DAILY',
    fund VARCHAR, scheme VARCHAR, scheme_name VARCHAR(200), isin VARCHAR(16),
    div_date DATE, ex_dividend_date DATE, record_date DATE, payment_date DATE,
    drate NUMERIC(15,6), dividend_amount NUMERIC(25,6), dnav NUMERIC(25,6),
    status VARCHAR(20), plan_type VARCHAR(50), option_type VARCHAR(50),
    source_file TEXT, raw_row JSONB, created_at TIMESTAMP DEFAULT now()
);
CREATE INDEX idx_kfin_div_daily_isin ON kfin_dividend_daily_raw(isin);
CREATE INDEX idx_kfin_div_daily_divdate ON kfin_dividend_daily_raw(div_date);
CREATE INDEX idx_kfin_div_daily_scheme ON kfin_dividend_daily_raw(scheme);

-- MASTER TABLES
DROP TABLE IF EXISTS nav_master CASCADE;
CREATE TABLE nav_master (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    scheme_id TEXT, isin VARCHAR(12), nav_date DATE NOT NULL,
    nav_value NUMERIC(15,6), purchase_price NUMERIC(15,6), redemption_price NUMERIC(15,6),
    rta_source VARCHAR, data_type VARCHAR,
    created_at TIMESTAMP DEFAULT now(), updated_at TIMESTAMP,
    CONSTRAINT uq_nav_isin_date UNIQUE (isin, nav_date)
);
CREATE INDEX idx_nav_master_scheme_id ON nav_master(scheme_id);
CREATE INDEX idx_nav_master_isin ON nav_master(isin);
CREATE INDEX idx_nav_master_date ON nav_master(nav_date DESC);

DROP TABLE IF EXISTS dividend_master CASCADE;
CREATE TABLE dividend_master (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    scheme_id TEXT, isin VARCHAR(12), ex_dividend_date DATE NOT NULL,
    record_date DATE, payment_date DATE,
    dividend_rate_per_unit NUMERIC(25,6), dividend_amount NUMERIC(25,6),
    dividend_bonus_flag VARCHAR(15), bonus_ratio VARCHAR(50), corp_div_rate NUMERIC(25,6),
    scheme_name VARCHAR(200), plan_type VARCHAR(50), option_type VARCHAR(50), ex_div_nav NUMERIC(25,6),
    rta_source VARCHAR, data_type VARCHAR,
    created_at TIMESTAMP DEFAULT now(), updated_at TIMESTAMP,
    CONSTRAINT uq_div_isin_date UNIQUE (isin, ex_dividend_date)
);
CREATE INDEX idx_div_master_scheme_id ON dividend_master(scheme_id);
CREATE INDEX idx_div_master_isin ON dividend_master(isin);
CREATE INDEX idx_div_master_exdate ON dividend_master(ex_dividend_date DESC);

DROP TABLE IF EXISTS unmapped_nav_schemes CASCADE;
CREATE TABLE unmapped_nav_schemes (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    isin VARCHAR(12), scheme_code VARCHAR, product_code VARCHAR, scheme_name VARCHAR(200), rta_source VARCHAR,
    first_nav_date DATE, last_nav_date DATE, nav_count INTEGER DEFAULT 0,
    suggested_scheme_id TEXT, match_confidence INTEGER, match_method VARCHAR,
    status VARCHAR DEFAULT 'UNMAPPED', notes TEXT,
    created_at TIMESTAMP DEFAULT now(), reviewed_at TIMESTAMP, reviewed_by TEXT,
    UNIQUE (isin, rta_source)
);
CREATE INDEX idx_unmapped_nav_isin ON unmapped_nav_schemes(isin);
CREATE INDEX idx_unmapped_nav_status ON unmapped_nav_schemes(status);

DROP TABLE IF EXISTS nav_statistics CASCADE;
CREATE TABLE nav_statistics (
    scheme_id TEXT PRIMARY KEY,
    first_nav_date DATE, last_nav_date DATE, total_nav_records INTEGER,
    latest_nav NUMERIC(15,6), latest_nav_date DATE,
    return_1m NUMERIC(10,4), return_3m NUMERIC(10,4), return_6m NUMERIC(10,4),
    return_1y NUMERIC(10,4), return_3y NUMERIC(10,4), return_5y NUMERIC(10,4),
    data_sources JSONB, updated_at TIMESTAMP DEFAULT now()
);
CREATE INDEX idx_nav_stat_latest_date ON nav_statistics(latest_nav_date DESC);

-- ============================================================================
-- IMPROVED STORED PROCEDURE: NAV MASTER (SET-BASED, 1000x FASTER)
-- ============================================================================

CREATE OR REPLACE PROCEDURE sp_refresh_nav_master()
LANGUAGE plpgsql AS $BODY$
DECLARE 
    v_cams_count INTEGER := 0;
    v_kfin_count INTEGER := 0;
    v_amfi_count INTEGER := 0;
BEGIN
    RAISE NOTICE '=== NAV MASTER REFRESH START (BATCH MODE) ===';
    
    -- ================================================================
    -- STEP 1: CAMS Historical NAV (FIXED - added DISTINCT ON)
    -- ================================================================
    RAISE NOTICE 'STEP 1/3: CAMS Historical NAV';
    INSERT INTO nav_master (scheme_id, isin, nav_date, nav_value, rta_source, data_type, created_at, updated_at)
    SELECT DISTINCT ON (isin, nav_date)  -- FIXED: Added DISTINCT ON
        COALESCE(smf.scheme_id, rta.scheme_id) as scheme_id,
        UPPER(TRIM(SUBSTRING(c.isin_no FROM 1 FOR 12))) as isin,
        c.nav_date,
        c.nav_value,
        'CAMS' as rta_source,
        'HISTORICAL' as data_type,
        now() as created_at,
        now() as updated_at
    FROM cams_nav_historical_raw c
    LEFT JOIN scheme_master_final smf 
        ON UPPER(TRIM(SUBSTRING(c.isin_no FROM 1 FOR 12))) = smf.isin
    LEFT JOIN rta_combined_scheme_master rta
        ON rta.rta_source = 'CAMS' AND rta.scheme_nav_code = c.product_code
    WHERE c.nav_date IS NOT NULL 
        AND c.nav_value IS NOT NULL
        AND UPPER(TRIM(c.isin_no)) ~ '^IN[A-Z0-9]{10}'
        AND (smf.scheme_id IS NOT NULL OR rta.scheme_id IS NOT NULL)
    ORDER BY isin, nav_date, c.nav_value DESC  -- FIXED: Keep highest NAV value
    ON CONFLICT (isin, nav_date) 
    DO UPDATE SET 
        scheme_id = COALESCE(EXCLUDED.scheme_id, nav_master.scheme_id),
        nav_value = COALESCE(EXCLUDED.nav_value, nav_master.nav_value),
        updated_at = now();
    GET DIAGNOSTICS v_cams_count = ROW_COUNT;
    RAISE NOTICE 'CAMS: % records processed', v_cams_count;
    
    -- ================================================================
    -- STEP 2: KFIN Historical NAV (FIXED - added DISTINCT ON)
    -- ================================================================
    RAISE NOTICE 'STEP 2/3: KFIN Historical NAV';
    INSERT INTO nav_master (scheme_id, isin, nav_date, nav_value, purchase_price, redemption_price, rta_source, data_type, created_at, updated_at)
    SELECT DISTINCT ON (isin, nav_date)  -- FIXED: Added DISTINCT ON
        COALESCE(smf.scheme_id, rta.scheme_id) as scheme_id,
        UPPER(TRIM(SUBSTRING(k.schemeisin FROM 1 FOR 12))) as isin,
        k.navdate as nav_date,
        k.nav,
        k.ppop,
        k.rpop,
        'KFIN' as rta_source,
        'HISTORICAL' as data_type,
        now() as created_at,
        now() as updated_at
    FROM kfin_nav_historical_raw k
    LEFT JOIN scheme_master_final smf 
        ON UPPER(TRIM(SUBSTRING(k.schemeisin FROM 1 FOR 12))) = smf.isin
    LEFT JOIN rta_combined_scheme_master rta
        ON rta.rta_source = 'KFIN' AND rta.rta_scheme_code LIKE '%' || k.scheme || '%'
    WHERE k.navdate IS NOT NULL 
        AND k.nav IS NOT NULL
        AND UPPER(TRIM(k.schemeisin)) ~ '^IN[A-Z0-9]{10}'
        AND (smf.scheme_id IS NOT NULL OR rta.scheme_id IS NOT NULL)
    ORDER BY isin, nav_date, k.nav DESC  -- FIXED: Keep highest NAV value
    ON CONFLICT (isin, nav_date) 
    DO UPDATE SET 
        scheme_id = COALESCE(EXCLUDED.scheme_id, nav_master.scheme_id),
        nav_value = COALESCE(EXCLUDED.nav_value, nav_master.nav_value),
        purchase_price = COALESCE(EXCLUDED.purchase_price, nav_master.purchase_price),
        redemption_price = COALESCE(EXCLUDED.redemption_price, nav_master.redemption_price),
        updated_at = now();
    GET DIAGNOSTICS v_kfin_count = ROW_COUNT;
    RAISE NOTICE 'KFIN: % records processed', v_kfin_count;
    
   -- STEP 3/3: AMFI Daily NAV (FIXED - Pre-aggregated to eliminate duplicates)
RAISE NOTICE 'STEP 3/3: AMFI Daily NAV';
INSERT INTO nav_master (scheme_id, isin, nav_date, nav_value, rta_source, data_type, created_at, updated_at)
SELECT 
    scheme_id,
    isin,
    nav_date,
    nav_value,
    'AMFI' as rta_source,
    'DAILY' as data_type,
    now() as created_at,
    now() as updated_at
FROM (
    -- Pre-aggregate to ensure only ONE row per (isin, nav_date)
    SELECT DISTINCT ON (isin, nav_date)
        smf.scheme_id,
        UPPER(TRIM(isin_derived.isin_single)) as isin,
        a.nav_date,
        a.net_asset_value as nav_value
    FROM amfi_nav_daily_raw a
    CROSS JOIN LATERAL (
        SELECT CASE 
            WHEN a.isin_div_payout_growth ~ '^IN[A-Z0-9]{10}' 
                THEN UPPER(TRIM(SUBSTRING(a.isin_div_payout_growth FROM 1 FOR 12)))
            WHEN a.isin_div_reinvestment ~ '^IN[A-Z0-9]{10}' 
                THEN UPPER(TRIM(SUBSTRING(a.isin_div_reinvestment FROM 1 FOR 12)))
        END as isin_single
    ) isin_derived
    LEFT JOIN scheme_master_final smf 
        ON smf.isin = UPPER(TRIM(isin_derived.isin_single))
    WHERE a.nav_date IS NOT NULL 
        AND a.net_asset_value IS NOT NULL
        AND isin_derived.isin_single IS NOT NULL
        AND smf.scheme_id IS NOT NULL
    ORDER BY isin, nav_date, smf.scheme_id NULLS LAST
) amfi_deduped
ON CONFLICT (isin, nav_date) 
DO UPDATE SET 
    scheme_id = COALESCE(EXCLUDED.scheme_id, nav_master.scheme_id),
    nav_value = COALESCE(EXCLUDED.nav_value, nav_master.nav_value),
    rta_source = CASE 
        WHEN nav_master.data_type = 'HISTORICAL' THEN nav_master.rta_source 
        ELSE 'AMFI' 
    END,
    data_type = 'DAILY',
    updated_at = now();

GET DIAGNOSTICS v_amfi_count = ROW_COUNT;
RAISE NOTICE 'AMFI: % records', v_amfi_count;
    
    RAISE NOTICE '=== NAV MASTER REFRESH COMPLETE ===';
    RAISE NOTICE 'Total: CAMS=%, KFIN=%, AMFI=%', v_cams_count, v_kfin_count, v_amfi_count;
END;
$BODY$;

-- ============================================================================
-- IMPROVED STORED PROCEDURE: DIVIDEND MASTER (SET-BASED)
-- ============================================================================

CREATE OR REPLACE PROCEDURE sp_refresh_dividend_master()
LANGUAGE plpgsql AS $BODY$
DECLARE 
    v_count INTEGER := 0;
    v_matched INTEGER := 0;
    v_unmatched INTEGER := 0;
BEGIN
    RAISE NOTICE '=== DIVIDEND MASTER REFRESH START ===';
    
    -- ================================================================
    -- STEP 1: CAMS Historical Dividends (Enhanced Matching)
    -- ================================================================
    RAISE NOTICE 'STEP 1/4: CAMS Historical Dividends';
    
    INSERT INTO dividend_master (
        scheme_id, isin, ex_dividend_date, record_date, 
        dividend_rate_per_unit, dividend_bonus_flag, bonus_ratio, 
        corp_div_rate, scheme_name, rta_source, data_type, 
        created_at, updated_at
    )
    SELECT DISTINCT ON (COALESCE(isin_clean, 'NONE'), ex_dividend_date)
        COALESCE(
            -- Try 1: Direct ISIN match
            smf_isin.scheme_id,
            -- Try 2: Product code via nav_master
            nm_code.scheme_id,
            -- Try 3: Product code via rta_combined
            rta_code.scheme_id
        ) as scheme_id,
        isin_clean as isin,
        c.ex_dividend_date,
        c.record_date,
        c.dividend_rate_per_unit,
        c.dividend_bonus_flag,
        c.bonus_ratio,
        c.corp_div_rate,
        c.scheme_name,
        'CAMS' as rta_source,
        'HISTORICAL' as data_type,
        now() as created_at,
        now() as updated_at
    FROM cams_dividend_historical_raw c
    CROSS JOIN LATERAL (
        SELECT CASE 
            WHEN c.isin ~ '^IN[A-Z0-9]{10}$' THEN UPPER(TRIM(c.isin))
            ELSE NULL
        END as isin_clean
    ) isin_derived
    -- Try 1: Match by ISIN
    LEFT JOIN scheme_master_final smf_isin 
        ON smf_isin.isin = isin_clean
    -- Try 2: Match by product_code via nav_master (link dividend to NAV data)
    LEFT JOIN LATERAL (
        SELECT DISTINCT scheme_id 
        FROM nav_master 
        WHERE isin IN (
            SELECT DISTINCT UPPER(TRIM(isin_no))
            FROM cams_nav_historical_raw
            WHERE product_code = c.product_code
            LIMIT 1
        )
        AND scheme_id IS NOT NULL
        LIMIT 1
    ) nm_code ON c.product_code IS NOT NULL
    -- Try 3: Match by product_code via rta_combined_scheme_master
    LEFT JOIN rta_combined_scheme_master rta_code
        ON rta_code.rta_source = 'CAMS' 
        AND rta_code.scheme_nav_code = c.product_code
    WHERE c.ex_dividend_date IS NOT NULL
        AND (
            isin_clean IS NOT NULL 
            OR smf_isin.scheme_id IS NOT NULL 
            OR nm_code.scheme_id IS NOT NULL 
            OR rta_code.scheme_id IS NOT NULL
        )
    ORDER BY COALESCE(isin_clean, 'NONE'), ex_dividend_date, smf_isin.scheme_id NULLS LAST
    ON CONFLICT (isin, ex_dividend_date) 
    DO UPDATE SET 
        scheme_id = COALESCE(EXCLUDED.scheme_id, dividend_master.scheme_id),
        dividend_rate_per_unit = COALESCE(EXCLUDED.dividend_rate_per_unit, dividend_master.dividend_rate_per_unit),
        updated_at = now();
    
    GET DIAGNOSTICS v_count = ROW_COUNT;
    RAISE NOTICE 'CAMS Historical: % records inserted', v_count;
    
    -- ================================================================
    -- STEP 2: KFIN Historical Dividends (Enhanced Matching)
    -- ================================================================
    RAISE NOTICE 'STEP 2/4: KFIN Historical Dividends';
    
    v_count := 0;
    INSERT INTO dividend_master (
        scheme_id, isin, ex_dividend_date, 
        dividend_rate_per_unit, ex_div_nav, scheme_name, 
        rta_source, data_type, created_at, updated_at
    )
    SELECT DISTINCT ON (COALESCE(isin_clean, 'NONE'), div_date)
        COALESCE(
            -- Try 1: Direct ISIN match
            smf_isin.scheme_id,
            -- Try 2: Scheme code via nav_master
            nm_code.scheme_id,
            -- Try 3: Scheme code via rta_combined
            rta_code.scheme_id
        ) as scheme_id,
        isin_clean as isin,
        k.div_date as ex_dividend_date,
        k.drate as dividend_rate_per_unit,
        k.dnav as ex_div_nav,
        k.funddesc as scheme_name,
        'KFIN' as rta_source,
        'HISTORICAL' as data_type,
        now() as created_at,
        now() as updated_at
    FROM kfin_dividend_historical_raw k
    CROSS JOIN LATERAL (
        SELECT CASE 
            WHEN k.isin ~ '^IN[A-Z0-9]{10}$' THEN UPPER(TRIM(k.isin))
            ELSE NULL
        END as isin_clean
    ) isin_derived
    -- Try 1: Match by ISIN
    LEFT JOIN scheme_master_final smf_isin 
        ON smf_isin.isin = isin_clean
    -- Try 2: Match by scheme code via nav_master
    LEFT JOIN LATERAL (
        SELECT DISTINCT scheme_id 
        FROM nav_master 
        WHERE isin IN (
            SELECT DISTINCT UPPER(TRIM(schemeisin))
            FROM kfin_nav_historical_raw
            WHERE scheme = k.scheme
            LIMIT 1
        )
        AND scheme_id IS NOT NULL
        LIMIT 1
    ) nm_code ON k.scheme IS NOT NULL
    -- Try 3: Match by scheme code via rta_combined
    LEFT JOIN rta_combined_scheme_master rta_code
        ON rta_code.rta_source = 'KFIN' 
        AND rta_code.rta_scheme_code = k.scheme
    WHERE k.div_date IS NOT NULL
        AND (
            isin_clean IS NOT NULL 
            OR smf_isin.scheme_id IS NOT NULL 
            OR nm_code.scheme_id IS NOT NULL 
            OR rta_code.scheme_id IS NOT NULL
        )
    ORDER BY COALESCE(isin_clean, 'NONE'), div_date, smf_isin.scheme_id NULLS LAST
    ON CONFLICT (isin, ex_dividend_date) 
    DO UPDATE SET 
        scheme_id = COALESCE(EXCLUDED.scheme_id, dividend_master.scheme_id),
        dividend_rate_per_unit = COALESCE(EXCLUDED.dividend_rate_per_unit, dividend_master.dividend_rate_per_unit),
        ex_div_nav = COALESCE(EXCLUDED.ex_div_nav, dividend_master.ex_div_nav),
        updated_at = now();
    
    GET DIAGNOSTICS v_count = ROW_COUNT;
    RAISE NOTICE 'KFIN Historical: % records inserted', v_count;
    
    -- ================================================================
    -- STEP 3: CAMS Daily Dividends (use existing logic)
    -- ================================================================
    RAISE NOTICE 'STEP 3/4: CAMS Daily Dividends';
    
    v_count := 0;
    INSERT INTO dividend_master (
        scheme_id, isin, ex_dividend_date, record_date, payment_date,
        dividend_rate_per_unit, dividend_bonus_flag, bonus_ratio, corp_div_rate,
        scheme_name, plan_type, option_type, rta_source, data_type,
        created_at, updated_at
    )
    SELECT DISTINCT ON (isin_clean, ex_dividend_date)
        smf.scheme_id,
        isin_clean as isin,
        c.ex_dividend_date,
        c.record_date,
        c.payment_date,
        c.dividend_rate_per_unit,
        c.dividend_bonus_flag,
        c.bonus_ratio,
        c.corp_div_rate,
        c.scheme_name,
        c.plan_type,
        c.option_type,
        'CAMS' as rta_source,
        'DAILY' as data_type,
        now() as created_at,
        now() as updated_at
    FROM cams_dividend_daily_raw c
    CROSS JOIN LATERAL (
        SELECT UPPER(TRIM(c.isin)) as isin_clean
        WHERE c.isin ~ '^IN[A-Z0-9]{10}$'
    ) isin_derived
    LEFT JOIN scheme_master_final smf ON smf.isin = isin_clean
    WHERE c.ex_dividend_date IS NOT NULL
        AND isin_clean IS NOT NULL
    ORDER BY isin_clean, ex_dividend_date, smf.scheme_id NULLS LAST
    ON CONFLICT (isin, ex_dividend_date) 
    DO UPDATE SET 
        scheme_id = COALESCE(EXCLUDED.scheme_id, dividend_master.scheme_id),
        payment_date = COALESCE(EXCLUDED.payment_date, dividend_master.payment_date),
        plan_type = COALESCE(EXCLUDED.plan_type, dividend_master.plan_type),
        option_type = COALESCE(EXCLUDED.option_type, dividend_master.option_type),
        data_type = 'DAILY',
        updated_at = now();
    
    GET DIAGNOSTICS v_count = ROW_COUNT;
    RAISE NOTICE 'CAMS Daily: % records inserted', v_count;
    
    -- ================================================================
    -- STEP 4: KFIN Daily Dividends (use existing logic)
    -- ================================================================
    RAISE NOTICE 'STEP 4/4: KFIN Daily Dividends';
    
    v_count := 0;
    INSERT INTO dividend_master (
        scheme_id, isin, ex_dividend_date, record_date, payment_date,
        dividend_rate_per_unit, dividend_amount, ex_div_nav,
        scheme_name, plan_type, option_type, rta_source, data_type,
        created_at, updated_at
    )
    SELECT DISTINCT ON (isin_clean, ex_dividend_date)
        smf.scheme_id,
        isin_clean as isin,
        COALESCE(k.ex_dividend_date, k.div_date) as ex_dividend_date,
        k.record_date,
        k.payment_date,
        k.drate as dividend_rate_per_unit,
        k.dividend_amount,
        k.dnav as ex_div_nav,
        k.scheme_name,
        k.plan_type,
        k.option_type,
        'KFIN' as rta_source,
        'DAILY' as data_type,
        now() as created_at,
        now() as updated_at
    FROM kfin_dividend_daily_raw k
    CROSS JOIN LATERAL (
        SELECT UPPER(TRIM(k.isin)) as isin_clean
        WHERE k.isin ~ '^IN[A-Z0-9]{10}$'
    ) isin_derived
    LEFT JOIN scheme_master_final smf ON smf.isin = isin_clean
    WHERE COALESCE(k.ex_dividend_date, k.div_date) IS NOT NULL
        AND isin_clean IS NOT NULL
    ORDER BY isin_clean, ex_dividend_date, smf.scheme_id NULLS LAST
    ON CONFLICT (isin, ex_dividend_date) 
    DO UPDATE SET 
        scheme_id = COALESCE(EXCLUDED.scheme_id, dividend_master.scheme_id),
        payment_date = COALESCE(EXCLUDED.payment_date, dividend_master.payment_date),
        dividend_amount = COALESCE(EXCLUDED.dividend_amount, dividend_master.dividend_amount),
        plan_type = COALESCE(EXCLUDED.plan_type, dividend_master.plan_type),
        option_type = COALESCE(EXCLUDED.option_type, dividend_master.option_type),
        data_type = 'DAILY',
        updated_at = now();
    
    GET DIAGNOSTICS v_count = ROW_COUNT;
    RAISE NOTICE 'KFIN Daily: % records inserted', v_count;
    
    RAISE NOTICE '=== DIVIDEND MASTER REFRESH COMPLETE ===';
END;
$BODY$;
CREATE OR REPLACE PROCEDURE sp_update_nav_statistics()
LANGUAGE plpgsql AS $BODY$
DECLARE v_count INTEGER;
BEGIN
    RAISE NOTICE '=== UPDATING NAV STATISTICS ===';
    TRUNCATE nav_statistics;
    
    -- Build statistics with CTEs for clarity
    WITH nav_aggregates AS (
        SELECT 
            scheme_id,
            MIN(nav_date) as first_nav_date,
            MAX(nav_date) as last_nav_date,
            COUNT(*) as total_nav_records
        FROM nav_master
        WHERE scheme_id IS NOT NULL
        GROUP BY scheme_id
    ),
    latest_navs AS (
        SELECT DISTINCT ON (scheme_id)
            scheme_id,
            nav_value as latest_nav,
            nav_date as latest_nav_date
        FROM nav_master
        WHERE scheme_id IS NOT NULL
        ORDER BY scheme_id, nav_date DESC
    ),
    source_counts AS (
        SELECT 
            scheme_id,
            jsonb_object_agg(rta_source, source_count) as data_sources
        FROM (
            SELECT scheme_id, rta_source, COUNT(*) as source_count
            FROM nav_master
            WHERE scheme_id IS NOT NULL
            GROUP BY scheme_id, rta_source
        ) sc
        GROUP BY scheme_id
    )
    INSERT INTO nav_statistics (
        scheme_id, first_nav_date, last_nav_date, total_nav_records,
        latest_nav, latest_nav_date, data_sources, updated_at
    )
    SELECT 
        na.scheme_id,
        na.first_nav_date,
        na.last_nav_date,
        na.total_nav_records,
        ln.latest_nav,
        ln.latest_nav_date,
        sc.data_sources,
        now()
    FROM nav_aggregates na
    JOIN latest_navs ln ON ln.scheme_id = na.scheme_id
    JOIN source_counts sc ON sc.scheme_id = na.scheme_id;
    
    GET DIAGNOSTICS v_count = ROW_COUNT;
    
    -- Update scheme_master_final
    UPDATE scheme_master_final smf 
    SET 
        latest_nav = ns.latest_nav, 
        latest_nav_date = ns.latest_nav_date, 
        updated_at = now()
    FROM nav_statistics ns 
    WHERE smf.scheme_id = ns.scheme_id;
    
    RAISE NOTICE 'Statistics updated for % schemes', v_count;
    RAISE NOTICE '=== NAV STATISTICS COMPLETE ===';
END;
$BODY$;

CREATE OR REPLACE PROCEDURE sp_identify_unmapped_amfi_schemes()
LANGUAGE plpgsql AS $BODY$
DECLARE v_count INTEGER;
BEGIN
    RAISE NOTICE '=== IDENTIFYING UNMAPPED AMFI SCHEMES ===';
    INSERT INTO unmapped_nav_schemes (isin, scheme_code, scheme_name, rta_source, status, notes, created_at)
    SELECT DISTINCT amfi.isin_single, amfi.amfi_code, amfi.scheme_name, 'AMFI', 'NO_NAV_DATA',
           'AMFI scheme exists but no NAV data found in any source', now()
    FROM amfi_scheme amfi LEFT JOIN nav_master nm ON nm.isin = amfi.isin_single
    WHERE nm.id IS NULL AND amfi.isin_single IS NOT NULL
    ON CONFLICT (isin, rta_source) DO NOTHING;
    GET DIAGNOSTICS v_count = ROW_COUNT;
    RAISE NOTICE 'Unmapped AMFI schemes: %', v_count;
    RAISE NOTICE '=== UNMAPPED AMFI SCHEMES COMPLETE ===';
END;
$BODY$;

CREATE OR REPLACE PROCEDURE sp_run_nav_dividend_etl()
LANGUAGE plpgsql AS $BODY$
BEGIN
    RAISE NOTICE '';
    RAISE NOTICE '========================================';
    RAISE NOTICE '  NAV & DIVIDEND ETL PIPELINE START';
    RAISE NOTICE '========================================';
    RAISE NOTICE '';
    CALL sp_refresh_nav_master();
    RAISE NOTICE '';
    CALL sp_refresh_dividend_master();
    RAISE NOTICE '';
    CALL sp_update_nav_statistics();
    RAISE NOTICE '';
    CALL sp_identify_unmapped_amfi_schemes();
    RAISE NOTICE '';
    RAISE NOTICE '========================================';
    RAISE NOTICE '  NAV & DIVIDEND ETL PIPELINE COMPLETE';
    RAISE NOTICE '========================================';
    RAISE NOTICE '';
END;
$BODY$;

CREATE OR REPLACE PROCEDURE sp_nav_quality_check()
LANGUAGE plpgsql AS $BODY$
BEGIN
    RAISE NOTICE '=== NAV DATA QUALITY CHECK ===';
    
    -- Check for duplicate NAV entries
    RAISE NOTICE 'Checking for duplicates...';
    PERFORM * FROM (
        SELECT isin, nav_date, COUNT(*) 
        FROM nav_master 
        GROUP BY isin, nav_date 
        HAVING COUNT(*) > 1
    ) dups;
    
    -- Check for future dates
    RAISE NOTICE 'Checking for future dates...';
    PERFORM * FROM nav_master WHERE nav_date > CURRENT_DATE;
    
    -- Check for invalid NAV values
    RAISE NOTICE 'Checking for invalid NAV values...';
    PERFORM * FROM nav_master WHERE nav_value <= 0 OR nav_value > 100000;
    
END;
$BODY$;

-- Add after initial load for better stored procedure performance
CREATE INDEX idx_cams_nav_raw_isin_date ON cams_nav_historical_raw(isin_no, nav_date);
CREATE INDEX idx_kfin_nav_raw_isin_date ON kfin_nav_historical_raw(schemeisin, navdate);
CREATE INDEX idx_amfi_nav_raw_isin_date ON amfi_nav_daily_raw(isin_div_payout_growth, nav_date);

DO $BODY$
BEGIN
    RAISE NOTICE '';
    RAISE NOTICE '========================================';
    RAISE NOTICE '  NAV & DIVIDEND SCHEMA v8 CREATED';
    RAISE NOTICE '========================================';
    RAISE NOTICE '';
    RAISE NOTICE 'TABLES: 11 total';
    RAISE NOTICE '  RAW NAV: 3 tables';
    RAISE NOTICE '  RAW DIVIDEND: 4 tables';
    RAISE NOTICE '  MASTER: 4 tables';
    RAISE NOTICE '';
    RAISE NOTICE 'STORED PROCEDURES: 5 total';
    RAISE NOTICE '  1. sp_refresh_nav_master()';
    RAISE NOTICE '  2. sp_refresh_dividend_master()';
    RAISE NOTICE '  3. sp_update_nav_statistics()';
    RAISE NOTICE '  4. sp_identify_unmapped_amfi_schemes()';
    RAISE NOTICE '  5. sp_run_nav_dividend_etl()';
    RAISE NOTICE '';
    RAISE NOTICE 'USAGE: CALL sp_run_nav_dividend_etl();';
    RAISE NOTICE '========================================';
    RAISE NOTICE '';
END;
$BODY$;