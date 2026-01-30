-- =============================================
-- PHASE 3 SCHEMA (FIXED) - SEBI-Compliant Statutory Disclosures
-- Fixed: Removed subquery from CHECK constraint, using trigger instead
-- =============================================

SET client_min_messages = WARNING;

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";
CREATE EXTENSION IF NOT EXISTS pg_trgm;

-- -------------------------
-- 1. AMC WEBSITE LINKS
-- -------------------------
DROP TABLE IF EXISTS amc_website_links CASCADE;
CREATE TABLE amc_website_links (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    amc_id UUID REFERENCES amc_master(amc_id),
    amc_name TEXT NOT NULL,
    
    portfolio_monthly_url TEXT,
    portfolio_halfyearly_url TEXT,
    portfolio_annual_url TEXT,
    factsheet_url TEXT,
    ter_url TEXT,
    aum_url TEXT,
    base_website_url TEXT,
    sebi_sid_base_url TEXT,
    
    last_scraped_at TIMESTAMP,
    scrape_status TEXT DEFAULT 'PENDING',
    scrape_notes TEXT,
    
    created_at TIMESTAMP DEFAULT now(),
    updated_at TIMESTAMP
);

CREATE INDEX idx_amc_links_amc_id ON amc_website_links(amc_id);
CREATE INDEX idx_amc_links_status ON amc_website_links(scrape_status);

-- -------------------------
-- 2. STATUTORY DOCUMENT DOWNLOADS TRACKING
-- -------------------------
DROP TABLE IF EXISTS statutory_document_downloads CASCADE;
CREATE TABLE statutory_document_downloads (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    
    amc_id UUID REFERENCES amc_master(amc_id),
    scheme_id TEXT,
    
    document_type TEXT NOT NULL,
    frequency TEXT,
    document_date DATE,
    download_url TEXT,
    local_file_path TEXT,
    file_format TEXT,
    file_size_kb INTEGER,
    
    download_status TEXT DEFAULT 'PENDING',
    download_attempts INTEGER DEFAULT 0,
    last_download_attempt TIMESTAMP,
    
    parsing_status TEXT DEFAULT 'PENDING',
    parsing_error TEXT,
    parsed_at TIMESTAMP,
    
    is_sebi_compliant BOOLEAN,
    sebi_format_type TEXT,
    validation_errors JSONB,
    
    created_at TIMESTAMP DEFAULT now(),
    updated_at TIMESTAMP
);

CREATE INDEX idx_statutory_amc ON statutory_document_downloads(amc_id);
CREATE INDEX idx_statutory_scheme ON statutory_document_downloads(scheme_id);
CREATE INDEX idx_statutory_type ON statutory_document_downloads(document_type);
CREATE INDEX idx_statutory_date ON statutory_document_downloads(document_date DESC);
CREATE INDEX idx_statutory_status ON statutory_document_downloads(download_status, parsing_status);

CREATE UNIQUE INDEX idx_statutory_unique ON statutory_document_downloads(
    amc_id, COALESCE(scheme_id, 'AMC_LEVEL'), document_type, 
    COALESCE(frequency, 'NONE'), document_date
);

-- -------------------------
-- 3. PORTFOLIO HOLDINGS RAW
-- -------------------------
DROP TABLE IF EXISTS portfolio_holdings_raw CASCADE;
CREATE TABLE portfolio_holdings_raw (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    
    document_id UUID REFERENCES statutory_document_downloads(id),
    scheme_id TEXT,
    amc_id UUID REFERENCES amc_master(amc_id),
    as_of_date DATE NOT NULL,
    
    security_name TEXT NOT NULL,
    isin_code VARCHAR(12),
    security_type TEXT,
    asset_class TEXT,
    listing_status TEXT,
    
    industry_sector TEXT,
    amfi_industry_classification TEXT,
    rating TEXT,
    
    quantity NUMERIC(20,4),
    market_value NUMERIC(20,2),
    portfolio_percentage NUMERIC(10,6),
    
    issuer_name TEXT,
    maturity_date DATE,
    residual_maturity_days INTEGER,
    coupon_rate NUMERIC(10,4),
    
    is_illiquid BOOLEAN DEFAULT false,
    is_non_traded BOOLEAN DEFAULT false,
    is_below_investment_grade BOOLEAN DEFAULT false,
    
    source_file TEXT,
    source_page INTEGER,
    extraction_confidence NUMERIC(3,2),
    section_name TEXT,
    sub_section TEXT,
    
    raw_data JSONB,
    created_at TIMESTAMP DEFAULT now()
);

CREATE INDEX idx_portfolio_raw_scheme ON portfolio_holdings_raw(scheme_id);
CREATE INDEX idx_portfolio_raw_date ON portfolio_holdings_raw(as_of_date DESC);
CREATE INDEX idx_portfolio_raw_isin ON portfolio_holdings_raw(isin_code);
CREATE INDEX idx_portfolio_raw_doc ON portfolio_holdings_raw(document_id);
CREATE INDEX idx_portfolio_raw_illiquid ON portfolio_holdings_raw(is_illiquid) WHERE is_illiquid = true;

-- -------------------------
-- 4. PORTFOLIO HOLDINGS MASTER
-- -------------------------
DROP TABLE IF EXISTS portfolio_holdings_master CASCADE;
CREATE TABLE portfolio_holdings_master (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    
    scheme_id TEXT NOT NULL,
    as_of_date DATE NOT NULL,
    
    security_name TEXT,
    isin_code VARCHAR(12),
    security_type TEXT,
    asset_class TEXT,
    listing_status TEXT,
    
    industry_sector TEXT,
    amfi_industry_classification TEXT,
    rating TEXT,
    
    quantity NUMERIC(20,4),
    market_value_lakhs NUMERIC(20,2),
    market_value_crores NUMERIC(20,2) GENERATED ALWAYS AS (market_value_lakhs / 100) STORED,
    portfolio_percentage NUMERIC(10,6),
    
    issuer_name TEXT,
    maturity_date DATE,
    residual_maturity_days INTEGER,
    coupon_rate NUMERIC(10,4),
    
    is_illiquid BOOLEAN DEFAULT false,
    is_non_traded BOOLEAN DEFAULT false,
    is_below_investment_grade BOOLEAN DEFAULT false,
    
    data_source TEXT,
    last_updated TIMESTAMP,
    
    created_at TIMESTAMP DEFAULT now(),
    updated_at TIMESTAMP
);

CREATE INDEX idx_portfolio_master_scheme_date ON portfolio_holdings_master(scheme_id, as_of_date DESC);
CREATE INDEX idx_portfolio_master_isin ON portfolio_holdings_master(isin_code);
CREATE INDEX idx_portfolio_master_asset_class ON portfolio_holdings_master(asset_class);

CREATE UNIQUE INDEX idx_portfolio_master_unique ON portfolio_holdings_master(
    scheme_id, as_of_date, COALESCE(isin_code, security_name)
);

-- -------------------------
-- 5. AUM DATA RAW
-- -------------------------
DROP TABLE IF EXISTS aum_data_raw CASCADE;
CREATE TABLE aum_data_raw (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    
    document_id UUID REFERENCES statutory_document_downloads(id),
    scheme_id TEXT,
    amc_id UUID REFERENCES amc_master(amc_id),
    as_of_date DATE NOT NULL,
    
    total_aum NUMERIC(20,2),
    average_aum NUMERIC(20,2),
    
    direct_plan_aum NUMERIC(20,2),
    associate_distributor_aum NUMERIC(20,2),
    non_associate_distributor_aum NUMERIC(20,2),
    
    t30_cities_aum NUMERIC(20,2),
    b30_cities_aum NUMERIC(20,2),
    
    retail_aum NUMERIC(20,2),
    corporate_aum NUMERIC(20,2),
    banks_fi_aum NUMERIC(20,2),
    fii_fpi_aum NUMERIC(20,2),
    hni_aum NUMERIC(20,2),
    
    total_investors INTEGER,
    retail_investors INTEGER,
    hni_investors INTEGER,
    institutional_investors INTEGER,
    total_units NUMERIC(20,4),
    
    currency VARCHAR(3) DEFAULT 'INR',
    aum_scale TEXT,
    category TEXT,
    
    source_file TEXT,
    raw_data JSONB,
    created_at TIMESTAMP DEFAULT now()
);

CREATE INDEX idx_aum_raw_scheme ON aum_data_raw(scheme_id);
CREATE INDEX idx_aum_raw_date ON aum_data_raw(as_of_date DESC);
CREATE INDEX idx_aum_raw_amc ON aum_data_raw(amc_id);

-- -------------------------
-- 6. AUM MASTER
-- -------------------------
DROP TABLE IF EXISTS aum_master CASCADE;
CREATE TABLE aum_master (
    scheme_id TEXT NOT NULL,
    as_of_date DATE NOT NULL,
    
    total_aum_crores NUMERIC(20,2),
    average_aum_crores NUMERIC(20,2),
    
    direct_plan_aum_crores NUMERIC(20,2),
    associate_distributor_aum_crores NUMERIC(20,2),
    non_associate_distributor_aum_crores NUMERIC(20,2),
    
    t30_cities_aum_crores NUMERIC(20,2),
    b30_cities_aum_crores NUMERIC(20,2),
    
    retail_aum_crores NUMERIC(20,2),
    corporate_aum_crores NUMERIC(20,2),
    banks_fi_aum_crores NUMERIC(20,2),
    fii_fpi_aum_crores NUMERIC(20,2),
    hni_aum_crores NUMERIC(20,2),
    
    total_investors INTEGER,
    retail_investors INTEGER,
    hni_investors INTEGER,
    institutional_investors INTEGER,
    total_units NUMERIC(20,4),
    
    aum_growth_mom NUMERIC(10,4),
    aum_growth_yoy NUMERIC(10,4),
    
    created_at TIMESTAMP DEFAULT now(),
    updated_at TIMESTAMP,
    
    PRIMARY KEY (scheme_id, as_of_date)
);

CREATE INDEX idx_aum_master_scheme ON aum_master(scheme_id);
CREATE INDEX idx_aum_master_date ON aum_master(as_of_date DESC);

-- -------------------------
-- 7. TER DATA RAW
-- -------------------------
DROP TABLE IF EXISTS ter_data_raw CASCADE;
CREATE TABLE ter_data_raw (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    
    document_id UUID REFERENCES statutory_document_downloads(id),
    scheme_id TEXT,
    amc_id UUID REFERENCES amc_master(amc_id),
    as_of_date DATE NOT NULL,
    plan_type TEXT NOT NULL,
    
    base_ter NUMERIC(10,6),
    base_ter_excluding_gst NUMERIC(10,6),
    additional_expense_52_6a_b NUMERIC(10,6),
    additional_expense_52_6a_c NUMERIC(10,6),
    gst_on_advisory_fee NUMERIC(10,6),
    gst_percentage NUMERIC(5,2) DEFAULT 18.00,
    gst_amount NUMERIC(10,6),
    
    investment_management_fee NUMERIC(10,6),
    registrar_fee NUMERIC(10,6),
    custodian_fee NUMERIC(10,6),
    audit_fee NUMERIC(10,6),
    distribution_expense NUMERIC(10,6),
    other_expenses NUMERIC(10,6),
    
    total_ter NUMERIC(10,6),
    ter_limit NUMERIC(10,6),
    
    source_file TEXT,
    raw_data JSONB,
    created_at TIMESTAMP DEFAULT now()
);

CREATE INDEX idx_ter_raw_scheme ON ter_data_raw(scheme_id);
CREATE INDEX idx_ter_raw_date ON ter_data_raw(as_of_date DESC);
CREATE INDEX idx_ter_raw_plan ON ter_data_raw(plan_type);

-- -------------------------
-- 8. TER MASTER (FIXED - No subquery in constraint)
-- -------------------------
DROP TABLE IF EXISTS ter_master CASCADE;
CREATE TABLE ter_master (
    scheme_id TEXT NOT NULL,
    as_of_date DATE NOT NULL,
    plan_type TEXT NOT NULL,
    
    base_ter NUMERIC(10,6),
    base_ter_excluding_gst NUMERIC(10,6),
    additional_expense_52_6a_b NUMERIC(10,6),
    additional_expense_52_6a_c NUMERIC(10,6),
    gst_on_advisory_fee NUMERIC(10,6),
    
    investment_management_fee NUMERIC(10,6),
    registrar_fee NUMERIC(10,6),
    custodian_fee NUMERIC(10,6),
    audit_fee NUMERIC(10,6),
    distribution_expense NUMERIC(10,6),
    other_expenses NUMERIC(10,6),
    
    total_ter NUMERIC(10,6),
    ter_limit NUMERIC(10,6),
    
    created_at TIMESTAMP DEFAULT now(),
    updated_at TIMESTAMP,
    
    PRIMARY KEY (scheme_id, as_of_date, plan_type)
);

CREATE INDEX idx_ter_master_scheme ON ter_master(scheme_id);
CREATE INDEX idx_ter_master_date ON ter_master(as_of_date DESC);

-- Trigger function to enforce Direct < Regular (SEBI requirement)
CREATE OR REPLACE FUNCTION check_ter_direct_less_than_regular()
RETURNS TRIGGER AS $$
DECLARE
    regular_ter NUMERIC(10,6);
BEGIN
    IF NEW.plan_type = 'DIRECT' THEN
        SELECT total_ter INTO regular_ter
        FROM ter_master
        WHERE scheme_id = NEW.scheme_id
          AND as_of_date = NEW.as_of_date
          AND plan_type = 'REGULAR';
        
        IF regular_ter IS NOT NULL AND NEW.total_ter >= regular_ter THEN
            RAISE EXCEPTION 'SEBI Violation: Direct plan TER (%) must be less than Regular plan TER (%) for scheme % on %',
                NEW.total_ter, regular_ter, NEW.scheme_id, NEW.as_of_date;
        END IF;
    END IF;
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_check_ter_direct
    BEFORE INSERT OR UPDATE ON ter_master
    FOR EACH ROW
    EXECUTE FUNCTION check_ter_direct_less_than_regular();

-- -------------------------
-- 9. FUND MANAGER MASTER
-- -------------------------
DROP TABLE IF EXISTS fund_manager_master CASCADE;
CREATE TABLE fund_manager_master (
    manager_id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    
    manager_name TEXT NOT NULL,
    manager_name_normalized TEXT,
    father_name TEXT,
    date_of_birth DATE,
    gender VARCHAR(10),
    
    qualification TEXT,
    total_experience_years NUMERIC(5,2),
    experience_last_10_years JSONB,
    
    has_regulatory_violations BOOLEAN DEFAULT false,
    violation_details TEXT,
    
    current_amc_id UUID REFERENCES amc_master(amc_id),
    designation TEXT,
    joining_date DATE,
    
    email TEXT,
    linkedin_url TEXT,
    
    created_at TIMESTAMP DEFAULT now(),
    updated_at TIMESTAMP
);

CREATE INDEX idx_fund_mgr_name ON fund_manager_master(manager_name_normalized);
CREATE INDEX idx_fund_mgr_amc ON fund_manager_master(current_amc_id);
CREATE INDEX idx_fund_mgr_violations ON fund_manager_master(has_regulatory_violations) 
    WHERE has_regulatory_violations = true;

-- -------------------------
-- 10. SCHEME FUND MANAGER MAPPING
-- -------------------------
DROP TABLE IF EXISTS scheme_fund_manager_mapping CASCADE;
CREATE TABLE scheme_fund_manager_mapping (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    
    scheme_id TEXT NOT NULL,
    manager_id UUID REFERENCES fund_manager_master(manager_id),
    
    start_date DATE NOT NULL,
    end_date DATE,
    is_primary_manager BOOLEAN DEFAULT true,
    
    tenure_return NUMERIC(10,4),
    benchmark_return NUMERIC(10,4),
    alpha_generated NUMERIC(10,4),
    
    created_at TIMESTAMP DEFAULT now(),
    updated_at TIMESTAMP
);

CREATE INDEX idx_scheme_mgr_scheme ON scheme_fund_manager_mapping(scheme_id);
CREATE INDEX idx_scheme_mgr_manager ON scheme_fund_manager_mapping(manager_id);
CREATE INDEX idx_scheme_mgr_current ON scheme_fund_manager_mapping(scheme_id, end_date) 
    WHERE end_date IS NULL;

-- -------------------------
-- 11. FACT SHEET DATA
-- -------------------------
DROP TABLE IF EXISTS fact_sheet_data CASCADE;
CREATE TABLE fact_sheet_data (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    
    document_id UUID REFERENCES statutory_document_downloads(id),
    scheme_id TEXT NOT NULL,
    as_of_date DATE NOT NULL,
    
    nav_open NUMERIC(15,4),
    nav_high NUMERIC(15,4),
    nav_low NUMERIC(15,4),
    nav_close NUMERIC(15,4),
    
    closing_aum_lakhs NUMERIC(20,2),
    average_aum_lakhs NUMERIC(20,2),
    
    gross_income_pct_aaum NUMERIC(10,6),
    total_expense_pct_aaum NUMERIC(10,6),
    mgmt_fee_pct_aaum NUMERIC(10,6),
    net_income_pct_aaum NUMERIC(10,6),
    
    portfolio_turnover_ratio NUMERIC(10,4),
    
    return_1m NUMERIC(10,4),
    return_3m NUMERIC(10,4),
    return_6m NUMERIC(10,4),
    return_1y NUMERIC(10,4),
    return_3y NUMERIC(10,4),
    return_5y NUMERIC(10,4),
    return_since_inception NUMERIC(10,4),
    
    sharpe_ratio NUMERIC(10,6),
    sortino_ratio NUMERIC(10,6),
    beta NUMERIC(10,6),
    alpha NUMERIC(10,6),
    standard_deviation NUMERIC(10,6),
    
    equity_allocation NUMERIC(10,4),
    debt_allocation NUMERIC(10,4),
    cash_allocation NUMERIC(10,4),
    other_allocation NUMERIC(10,4),
    
    top_10_holdings_pct NUMERIC(10,4),
    
    benchmark_name TEXT,
    benchmark_return_1y NUMERIC(10,4),
    fund_manager_name TEXT,
    
    source_file TEXT,
    raw_data JSONB,
    
    created_at TIMESTAMP DEFAULT now(),
    updated_at TIMESTAMP
);

CREATE INDEX idx_factsheet_scheme_date ON fact_sheet_data(scheme_id, as_of_date DESC);
CREATE UNIQUE INDEX idx_factsheet_unique ON fact_sheet_data(scheme_id, as_of_date);

-- -------------------------
-- 12. SEBI SID DOCUMENTS
-- -------------------------
DROP TABLE IF EXISTS sebi_sid_documents CASCADE;
CREATE TABLE sebi_sid_documents (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    
    scheme_id TEXT,
    amc_id UUID REFERENCES amc_master(amc_id),
    
    sebi_scheme_code TEXT,
    document_type TEXT,
    document_title TEXT,
    document_date DATE,
    effective_date DATE,
    
    sebi_url TEXT,
    local_file_path TEXT,
    file_size_kb INTEGER,
    
    download_status TEXT DEFAULT 'PENDING',
    parsing_status TEXT DEFAULT 'PENDING',
    
    investment_objective TEXT,
    asset_allocation_pattern TEXT,
    risk_factors TEXT,
    exit_load_structure TEXT,
    benchmark_index TEXT,
    fund_manager_info JSONB,
    
    extracted_text TEXT,
    
    created_at TIMESTAMP DEFAULT now(),
    updated_at TIMESTAMP
);

CREATE INDEX idx_sebi_sid_scheme ON sebi_sid_documents(scheme_id);
CREATE INDEX idx_sebi_sid_amc ON sebi_sid_documents(amc_id);
CREATE INDEX idx_sebi_sid_date ON sebi_sid_documents(document_date DESC);

-- =============================================
-- STORED PROCEDURES
-- =============================================

CREATE OR REPLACE PROCEDURE sp_refresh_portfolio_holdings_master()
LANGUAGE plpgsql
AS $$
BEGIN
    RAISE NOTICE '📊 Refreshing Portfolio Holdings Master (SEBI-compliant)...';
    
    INSERT INTO portfolio_holdings_master (
        scheme_id, as_of_date, security_name, isin_code, 
        security_type, asset_class, listing_status,
        industry_sector, amfi_industry_classification, rating,
        quantity, market_value_lakhs, portfolio_percentage,
        issuer_name, maturity_date, residual_maturity_days, coupon_rate,
        is_illiquid, is_non_traded, is_below_investment_grade,
        data_source, last_updated, created_at, updated_at
    )
    SELECT DISTINCT ON (scheme_id, as_of_date, COALESCE(isin_code, security_name))
        scheme_id, as_of_date, security_name, isin_code,
        security_type, asset_class, listing_status,
        industry_sector, amfi_industry_classification, rating,
        quantity, market_value, portfolio_percentage,
        issuer_name, maturity_date, residual_maturity_days, coupon_rate,
        is_illiquid, is_non_traded, is_below_investment_grade,
        'STATUTORY', now(), now(), now()
    FROM portfolio_holdings_raw
    WHERE scheme_id IS NOT NULL AND as_of_date IS NOT NULL
    ORDER BY scheme_id, as_of_date, COALESCE(isin_code, security_name), 
             extraction_confidence DESC NULLS LAST
    ON CONFLICT (scheme_id, as_of_date, COALESCE(isin_code, security_name))
    DO UPDATE SET
        quantity = EXCLUDED.quantity,
        market_value_lakhs = EXCLUDED.market_value_lakhs,
        portfolio_percentage = EXCLUDED.portfolio_percentage,
        is_illiquid = EXCLUDED.is_illiquid,
        updated_at = now();
    
    RAISE NOTICE '✅ Portfolio Holdings Master refreshed';
END;
$$;

CREATE OR REPLACE PROCEDURE sp_refresh_aum_master()
LANGUAGE plpgsql
AS $$
BEGIN
    RAISE NOTICE '📊 Refreshing AUM Master (SEBI grid format)...';
    
    INSERT INTO aum_master (
        scheme_id, as_of_date, 
        total_aum_crores, average_aum_crores,
        direct_plan_aum_crores, associate_distributor_aum_crores,
        non_associate_distributor_aum_crores,
        t30_cities_aum_crores, b30_cities_aum_crores,
        retail_aum_crores, corporate_aum_crores,
        banks_fi_aum_crores, fii_fpi_aum_crores, hni_aum_crores,
        total_investors, retail_investors, hni_investors, institutional_investors,
        total_units, created_at, updated_at
    )
    SELECT DISTINCT ON (scheme_id, as_of_date)
        scheme_id, as_of_date,
        CASE WHEN aum_scale = 'LAKHS' THEN total_aum / 100 ELSE total_aum END,
        CASE WHEN aum_scale = 'LAKHS' THEN average_aum / 100 ELSE average_aum END,
        direct_plan_aum, associate_distributor_aum, non_associate_distributor_aum,
        t30_cities_aum, b30_cities_aum,
        retail_aum, corporate_aum, banks_fi_aum, fii_fpi_aum, hni_aum,
        total_investors, retail_investors, hni_investors, institutional_investors,
        total_units, now(), now()
    FROM aum_data_raw
    WHERE scheme_id IS NOT NULL
    ORDER BY scheme_id, as_of_date, id DESC
    ON CONFLICT (scheme_id, as_of_date)
    DO UPDATE SET
        total_aum_crores = EXCLUDED.total_aum_crores,
        average_aum_crores = EXCLUDED.average_aum_crores,
        direct_plan_aum_crores = COALESCE(EXCLUDED.direct_plan_aum_crores, aum_master.direct_plan_aum_crores),
        updated_at = now();
    
    UPDATE aum_master am SET 
        aum_growth_mom = (
            SELECT ((am.total_aum_crores - prev.total_aum_crores) / NULLIF(prev.total_aum_crores, 0)) * 100
            FROM aum_master prev
            WHERE prev.scheme_id = am.scheme_id
              AND prev.as_of_date = (am.as_of_date - INTERVAL '1 month')::date
        ),
        aum_growth_yoy = (
            SELECT ((am.total_aum_crores - prev.total_aum_crores) / NULLIF(prev.total_aum_crores, 0)) * 100
            FROM aum_master prev
            WHERE prev.scheme_id = am.scheme_id
              AND prev.as_of_date = (am.as_of_date - INTERVAL '1 year')::date
        );
    
    RAISE NOTICE '✅ AUM Master refreshed';
END;
$$;

CREATE OR REPLACE PROCEDURE sp_refresh_ter_master()
LANGUAGE plpgsql
AS $$
BEGIN
    RAISE NOTICE '📊 Refreshing TER Master (SEBI Reg 52 compliant)...';
    
    INSERT INTO ter_master (
        scheme_id, as_of_date, plan_type,
        base_ter, base_ter_excluding_gst,
        additional_expense_52_6a_b, additional_expense_52_6a_c,
        gst_on_advisory_fee,
        investment_management_fee, registrar_fee, custodian_fee,
        audit_fee, distribution_expense, other_expenses,
        total_ter, ter_limit, created_at, updated_at
    )
    SELECT DISTINCT ON (scheme_id, as_of_date, plan_type)
        scheme_id, as_of_date, COALESCE(plan_type, 'REGULAR'),
        base_ter, base_ter_excluding_gst,
        additional_expense_52_6a_b, additional_expense_52_6a_c,
        gst_on_advisory_fee,
        investment_management_fee, registrar_fee, custodian_fee,
        audit_fee, distribution_expense, other_expenses,
        total_ter, ter_limit, now(), now()
    FROM ter_data_raw
    WHERE scheme_id IS NOT NULL
    ORDER BY scheme_id, as_of_date, plan_type, id DESC
    ON CONFLICT (scheme_id, as_of_date, plan_type)
    DO UPDATE SET
        total_ter = EXCLUDED.total_ter,
        base_ter = COALESCE(EXCLUDED.base_ter, ter_master.base_ter),
        updated_at = now();
    
    RAISE NOTICE '✅ TER Master refreshed';
END;
$$;

COMMENT ON TABLE portfolio_holdings_master IS 'SEBI-compliant portfolio holdings with illiquid flagging';
COMMENT ON TABLE aum_master IS 'SEBI grid format: T30/B30, Direct/Distributor breakup';
COMMENT ON TABLE ter_master IS 'SEBI Regulation 52 compliant TER disclosure';
COMMENT ON TRIGGER trigger_check_ter_direct ON ter_master IS 'Enforces SEBI requirement: Direct plan TER must be less than Regular plan TER';

-- Success message via DO block
DO $
BEGIN
    RAISE NOTICE 'Phase 3 Schema Created Successfully!';
    RAISE NOTICE 'Tables: 12 main tables + tracking tables';
    RAISE NOTICE 'Procedures: 3 refresh procedures';
    RAISE NOTICE 'Trigger: TER validation (Direct < Regular)';
    RAISE NOTICE 'Ready for Phase 3 ETL!';
END $;