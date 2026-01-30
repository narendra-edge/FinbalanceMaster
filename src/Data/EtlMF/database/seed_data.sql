-- Insert AMFI normalized schemes
INSERT INTO amfi_schemes_normalized 
(amfi_code, scheme_code_rta, scheme_name, category, subcategory, plan_type, option_type, isin_dividend, isin_reinvestment)
VALUES
('100001','CAMS001', 'ABC Equity Fund - Direct Plan - Growth', 'Equity', 'Large Cap', 'Direct', 'Growth', 'INF0001DIV', 'INF0001REINV'),
('100002', 'KFIN001', 'XYZ Debt Fund - Regular Plan - IDCW', 'Debt', 'Corporate Bond', 'Regular', 'IDCW', 'INF0002DIV', 'INF0002REINV');

-- Insert daily NAV sample (added scheme_code_rta dummy values)
INSERT INTO daily_nav (amfi_code, scheme_code_rta, scheme_name, nav_date, nav)
VALUES
('100001', 'CAMS001', 'ABC Equity Fund - Direct Plan - Growth', CURRENT_DATE, 152.35),
('100002', 'KFIN001', 'XYZ Debt Fund - Regular Plan - IDCW', CURRENT_DATE, 102.78);

-- Insert historical NAV sample
INSERT INTO historical_nav (amfi_code, nav_date, nav)
VALUES
('100001', CURRENT_DATE - INTERVAL '1 day', 151.80),
('100002', CURRENT_DATE - INTERVAL '1 day', 103.10);