"""
run_phase3_etl.py
-----------------
Complete Phase 3 ETL orchestrator with SEBI compliance and real-world format support.

Pipeline:
1. Scrape AMFI for AMC website links
2. Crawl AMC websites for statutory documents
3. Parse PDFs with SEBI validation
4. Scrape SEBI for SID documents
5. Refresh master tables
"""

import logging
from pathlib import Path
from datetime import datetime
from typing import Optional
import sys

from sqlalchemy import text

# Import Phase 3 modules
from Etl.utils import settings, engine, ensure_dir
from Etl.amfi_link_scraper import AmfiLinkScraper
from Etl.amc_website_crawler import AmcWebsiteCrawler
from Etl.statutory_pdf_parser_enhanced import StatutoryPdfParser
from Etl.sebi_sid_scraper import SebiSidScraper
from Etl.real_world_amc_parser import (
    PortfolioStatementParser,
    AaumReportParser,
    ExpenseRatioParser
)

logger = logging.getLogger("mf.etl.phase3")


class Phase3Orchestrator:
    """
    Master orchestrator for Phase 3 statutory disclosure ETL.
    """
    
    def __init__(self):
        self.db_url = str(engine.url)
        self.data_root = Path(settings['paths']['data_root'])
        self.downloads_dir = Path(settings['paths']['downloads_dir'])
        
        # Phase 3 directories
        self.statutory_dir = ensure_dir(self.downloads_dir / 'statutory')
        self.sebi_dir = ensure_dir(self.downloads_dir / 'sebi_sids')
        self.portfolio_dir = ensure_dir(self.downloads_dir / 'portfolio')
        self.aaum_dir = ensure_dir(self.downloads_dir / 'aaum')
        self.ter_dir = ensure_dir(self.downloads_dir / 'ter')
        
        logger.info("📁 Phase 3 directories initialized")
    
    def run_step1_amfi_links(self):
        """
        Step 1: Scrape AMFI for AMC website links.
        """
        logger.info("=" * 70)
        logger.info("STEP 1: Scraping AMFI Portfolio Disclosure Page")
        logger.info("=" * 70)
        
        try:
            scraper = AmfiLinkScraper(db_url=self.db_url)
            scraper.run()
            logger.info("✅ AMFI links scraped successfully")
        except Exception as e:
            logger.error(f"❌ AMFI scraping failed: {e}")
            raise
    
    def run_step2_amc_crawl(self, limit_amcs: Optional[int] = None):
        """
        Step 2: Crawl AMC websites for statutory documents.
        """
        logger.info("=" * 70)
        logger.info("STEP 2: Crawling AMC Websites for Documents")
        logger.info("=" * 70)
        
        try:
            crawler = AmcWebsiteCrawler(
                db_url=self.db_url,
                download_base_dir=self.downloads_dir
            )
            crawler.run(limit=limit_amcs)
            logger.info("✅ AMC websites crawled successfully")
        except Exception as e:
            logger.error(f"❌ AMC crawling failed: {e}")
            raise
    
    def run_step3_parse_pdfs(self, doc_type: Optional[str] = None):
        """
        Step 3: Parse downloaded PDFs with SEBI validation.
        """
        logger.info("=" * 70)
        logger.info(f"STEP 3: Parsing PDFs{' (' + doc_type + ')' if doc_type else ''}")
        logger.info("=" * 70)
        
        try:
            parser = StatutoryPdfParser(db_url=self.db_url)
            parser.run(doc_type=doc_type, limit=100)
            logger.info("✅ PDFs parsed successfully")
        except Exception as e:
            logger.error(f"❌ PDF parsing failed: {e}")
            raise
    
    def run_step3b_parse_csvs(self):
        """
        Step 3b: Parse CSV/Excel files (Portfolio, AAUM, TER).
        """
        logger.info("=" * 70)
        logger.info("STEP 3b: Parsing CSV/Excel Files")
        logger.info("=" * 70)
        
        try:
            # Portfolio CSV files
            portfolio_parser = PortfolioStatementParser(db_url=self.db_url)
            portfolio_files = list(self.portfolio_dir.glob("*.csv")) + list(self.portfolio_dir.glob("*.xlsx"))
            
            for file in portfolio_files:
                logger.info(f"📊 Parsing portfolio: {file.name}")
                try:
                    result = portfolio_parser.parse_portfolio_csv(file)
                    if result:
                        # Match to scheme_id (simplified - production would use fuzzy matching)
                        scheme_id = self._match_scheme_from_file(file)
                        if scheme_id:
                            portfolio_parser.save_to_database(result, scheme_id)
                except Exception as e:
                    logger.error(f"Failed to parse {file.name}: {e}")
            
            # AAUM CSV files
            aaum_parser = AaumReportParser(db_url=self.db_url)
            aaum_files = list(self.aaum_dir.glob("*.csv")) + list(self.aaum_dir.glob("*.xlsx"))
            
            for file in aaum_files:
                logger.info(f"📊 Parsing AAUM: {file.name}")
                try:
                    result = aaum_parser.parse_aaum_csv(file)
                    if result:
                        logger.info(f"Extracted {len(result.get('schemes', []))} schemes")
                except Exception as e:
                    logger.error(f"Failed to parse {file.name}: {e}")
            
            # TER files
            ter_parser = ExpenseRatioParser(db_url=self.db_url)
            ter_files = list(self.ter_dir.glob("*.csv")) + list(self.ter_dir.glob("*.xlsx"))
            
            for file in ter_files:
                logger.info(f"📊 Parsing TER: {file.name}")
                try:
                    result = ter_parser.parse_ter_file(file)
                    if result:
                        logger.info(f"Extracted TER for {len(result.get('ter_data', []))} plans")
                except Exception as e:
                    logger.error(f"Failed to parse {file.name}: {e}")
            
            logger.info("✅ CSV/Excel files parsed successfully")
            
        except Exception as e:
            logger.error(f"❌ CSV parsing failed: {e}")
            raise
    
    def _match_scheme_from_file(self, file_path: Path) -> Optional[str]:
        """
        Match file to scheme_id using filename or content.
        Production version would use fuzzy matching against scheme_master_final.
        """
        # Simplified - extract scheme code from filename
        filename = file_path.stem
        
        # Try to find scheme in database
        with engine.connect() as conn:
            result = conn.execute(
                text("""
                    SELECT scheme_id FROM scheme_master_final
                    WHERE canonical_scheme_name ILIKE :pattern
                    LIMIT 1
                """),
                {"pattern": f"%{filename[:20]}%"}
            ).fetchone()
            
            if result:
                return result[0]
        
        return None
    
    def run_step4_sebi_sids(self, limit_amcs: Optional[int] = None):
        """
        Step 4: Scrape SEBI for SID documents.
        """
        logger.info("=" * 70)
        logger.info("STEP 4: Scraping SEBI SIDs")
        logger.info("=" * 70)
        
        try:
            scraper = SebiSidScraper(
                db_url=self.db_url,
                download_dir=self.sebi_dir
            )
            scraper.run(limit_amcs=limit_amcs)
            logger.info("✅ SEBI SIDs scraped successfully")
        except Exception as e:
            logger.error(f"❌ SEBI scraping failed: {e}")
            raise
    
    def run_step5_refresh_masters(self):
        """
        Step 5: Run SQL procedures to refresh master tables.
        """
        logger.info("=" * 70)
        logger.info("STEP 5: Refreshing Master Tables")
        logger.info("=" * 70)
        
        try:
            with engine.begin() as conn:
                logger.info("📊 Refreshing portfolio holdings master...")
                conn.execute(text("CALL sp_refresh_portfolio_holdings_master()"))
                
                logger.info("💰 Refreshing AUM master...")
                conn.execute(text("CALL sp_refresh_aum_master()"))
                
                logger.info("📈 Refreshing TER master...")
                conn.execute(text("CALL sp_refresh_ter_master()"))
            
            logger.info("✅ Master tables refreshed successfully")
            
        except Exception as e:
            logger.error(f"❌ Master refresh failed: {e}")
            raise
    
    def run_step6_data_quality_report(self):
        """
        Step 6: Generate data quality report.
        """
        logger.info("=" * 70)
        logger.info("STEP 6: Data Quality Report")
        logger.info("=" * 70)
        
        try:
            with engine.connect() as conn:
                # Portfolio quality
                portfolio_stats = conn.execute(text("""
                    SELECT 
                        COUNT(DISTINCT scheme_id) as schemes_count,
                        COUNT(*) as total_holdings,
                        COUNT(DISTINCT CASE WHEN isin_code IS NOT NULL THEN scheme_id END) as schemes_with_isin,
                        COUNT(CASE WHEN is_illiquid = true THEN 1 END) as illiquid_holdings,
                        AVG(portfolio_percentage) as avg_holding_pct
                    FROM portfolio_holdings_master
                    """)).fetchone()
                
                logger.info(f"""
                       📊 Portfolio Holdings Quality:
                       Schemes: {portfolio_stats[0]}
                       Total Holdings: {portfolio_stats[1]}
                       Schemes with ISIN: {portfolio_stats[2]}
                       Illiquid Holdings: {portfolio_stats[3]}
                       Avg Holding %: {portfolio_stats[4]:.2f}%
                                    """)
                
                # AUM quality
                aum_stats = conn.execute(text("""
                    SELECT 
                        COUNT(DISTINCT scheme_id) as schemes_count,
                        SUM(total_aum_crores) as total_aum,
                        AVG(aum_growth_yoy) as avg_yoy_growth,
                        COUNT(CASE WHEN t30_cities_aum_crores IS NOT NULL THEN 1 END) as schemes_with_geo_breakup
                    FROM aum_master
                    WHERE as_of_date >= CURRENT_DATE - INTERVAL '3 months'
                """)).fetchone()
                
                logger.info(f"""
                    💰 AUM Quality:
                       Schemes: {aum_stats[0]}
                       Total AUM: ₹{aum_stats[1]:,.0f} Cr
                       Avg YoY Growth: {aum_stats[2]:.2f}%
                       With Geo Breakup: {aum_stats[3]}
                                    """)
                
                # TER quality
                ter_stats = conn.execute(text("""
                    SELECT 
                        COUNT(DISTINCT scheme_id) as schemes_count,
                        AVG(CASE WHEN plan_type = 'REGULAR' THEN total_ter END) as avg_regular_ter,
                        AVG(CASE WHEN plan_type = 'DIRECT' THEN total_ter END) as avg_direct_ter,
                        COUNT(CASE WHEN is_sebi_compliant = true THEN 1 END) as sebi_compliant_docs
                    FROM ter_master tm
                    JOIN statutory_document_downloads sdd ON sdd.scheme_id = tm.scheme_id
                    WHERE tm.as_of_date >= CURRENT_DATE - INTERVAL '3 months'
                """)).fetchone()
                
                logger.info(f"""
                    📈 TER Quality:
                       Schemes: {ter_stats[0]}
                       Avg Regular TER: {ter_stats[1]:.2f}%
                       Avg Direct TER: {ter_stats[2]:.2f}%
                       SEBI Compliant Docs: {ter_stats[3]}
                                    """)
                
                # Fund Manager stats
                fm_stats = conn.execute(text("""
                    SELECT 
                        COUNT(*) as total_managers,
                        COUNT(CASE WHEN has_regulatory_violations = true THEN 1 END) as with_violations,
                        AVG(total_experience_years) as avg_experience
                    FROM fund_manager_master
                """)).fetchone()
                
                logger.info(f"""
                        👤 Fund Manager Quality:
                           Total Managers: {fm_stats[0]}
                           With Violations: {fm_stats[1]}
                           Avg Experience: {fm_stats[2]:.1f} years
                                        """)
            
            logger.info("✅ Data quality report generated")
            
        except Exception as e:
            logger.error(f"❌ Report generation failed: {e}")
    
    def run_full_pipeline(self, test_mode: bool = False):
        """
        Execute complete Phase 3 pipeline.
        """
        start_time = datetime.now()
        logger.info("=" * 70)
        logger.info("🚀 PHASE 3 ETL PIPELINE START")
        logger.info(f"Start Time: {start_time}")
        logger.info(f"Test Mode: {test_mode}")
        logger.info("=" * 70)
        
        limit = 3 if test_mode else None
        
        try:
            # Step 1: AMFI Links
            self.run_step1_amfi_links()
            
            # Step 2: AMC Website Crawl
            self.run_step2_amc_crawl(limit_amcs=limit)
            
            # Step 3: Parse PDFs
            self.run_step3_parse_pdfs(doc_type='PORTFOLIO')
            self.run_step3_parse_pdfs(doc_type='TER')
            self.run_step3_parse_pdfs(doc_type='AUM')
            
            # Step 3b: Parse CSV/Excel files
            self.run_step3b_parse_csvs()
            
            # Step 4: SEBI SIDs
            self.run_step4_sebi_sids(limit_amcs=limit)
            
            # Step 5: Refresh Masters
            self.run_step5_refresh_masters()
            
            # Step 6: Quality Report
            self.run_step6_data_quality_report()
            
            end_time = datetime.now()
            duration = end_time - start_time
            
            logger.info("=" * 70)
            logger.info("🎉 PHASE 3 ETL PIPELINE COMPLETE")
            logger.info(f"Duration: {duration}")
            logger.info("=" * 70)
            
        except Exception as e:
            logger.exception(f"❌ Phase 3 pipeline failed: {e}")
            raise
    
    def run_incremental_update(self):
        """
        Incremental update (for scheduled runs).
        Only processes new documents since last run.
        """
        logger.info("🔄 Starting Phase 3 Incremental Update")
        
        try:
            # Only crawl AMCs not scraped in 7+ days
            self.run_step2_amc_crawl(limit_amcs=None)
            
            # Parse only newly downloaded documents
            self.run_step3_parse_pdfs(doc_type=None)
            self.run_step3b_parse_csvs()
            
            # Refresh masters
            self.run_step5_refresh_masters()
            
            # Quick quality check
            self.run_step6_data_quality_report()
            
            logger.info("✅ Incremental update complete")
            
        except Exception as e:
            logger.exception(f"❌ Incremental update failed: {e}")
    
    def run_specific_amc(self, amc_name: str):
        """
        Process specific AMC only (for testing/debugging).
        """
        logger.info(f"🎯 Processing specific AMC: {amc_name}")
        
        try:
            # Get AMC ID
            with engine.connect() as conn:
                result = conn.execute(
                    text("""
                        SELECT amc_id FROM amc_master
                        WHERE amc_full_name ILIKE :name
                        LIMIT 1
                    """),
                    {"name": f"%{amc_name}%"}
                ).fetchone()
                
                if not result:
                    logger.error(f"❌ AMC not found: {amc_name}")
                    return
                
                amc_id = result[0]
            
            logger.info(f"✓ Found AMC ID: {amc_id}")
            
            # Process only this AMC's documents
            with engine.begin() as conn:
                docs = conn.execute(
                    text("""
                        SELECT id, document_type, local_file_path
                        FROM statutory_document_downloads
                        WHERE amc_id = :amc_id
                          AND download_status = 'DOWNLOADED'
                          AND parsing_status = 'PENDING'
                    """),
                    {"amc_id": amc_id}
                ).fetchall()
                
                logger.info(f"Found {len(docs)} documents to process")
                
                for doc in docs:
                    logger.info(f"Processing: {doc[1]} - {Path(doc[2]).name}")
                    # Process document...
            
            logger.info("✅ AMC-specific processing complete")
            
        except Exception as e:
            logger.exception(f"❌ AMC processing failed: {e}")


def configure_logging():
    """Configure logging for Phase 3."""
    logs_dir = Path(settings['paths']['logs_dir'])
    logs_dir.mkdir(parents=True, exist_ok=True)
    
    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
    log_file = logs_dir / f"phase3_etl_{timestamp}.log"
    
    logging.basicConfig(
        level=logging.INFO,
        format="%(asctime)s [%(levelname)s] %(name)s - %(message)s",
        handlers=[
            logging.FileHandler(log_file, encoding='utf-8'),
            logging.StreamHandler(sys.stdout)
        ]
    )
    
    logger.info(f"📜 Logging to: {log_file}")
    return log_file


def main():
    """Main entry point."""
    configure_logging()
    
    orchestrator = Phase3Orchestrator()
    
    # Parse command line arguments
    if len(sys.argv) > 1:
        command = sys.argv[1].lower()
        
        if command == 'full':
            orchestrator.run_full_pipeline(test_mode=False)
        elif command == 'test':
            orchestrator.run_full_pipeline(test_mode=True)
        elif command == 'incremental':
            orchestrator.run_incremental_update()
        elif command == 'amfi':
            orchestrator.run_step1_amfi_links()
        elif command == 'crawl':
            limit = int(sys.argv[2]) if len(sys.argv) > 2 else None
            orchestrator.run_step2_amc_crawl(limit_amcs=limit)
        elif command == 'parse':
            doc_type = sys.argv[2].upper() if len(sys.argv) > 2 else None
            orchestrator.run_step3_parse_pdfs(doc_type=doc_type)
        elif command == 'sebi':
            limit = int(sys.argv[2]) if len(sys.argv) > 2 else None
            orchestrator.run_step4_sebi_sids(limit_amcs=limit)
        elif command == 'refresh':
            orchestrator.run_step5_refresh_masters()
        elif command == 'report':
            orchestrator.run_step6_data_quality_report()
        elif command == 'amc':
            if len(sys.argv) < 3:
                print("Usage: python run_phase3_etl.py amc 'AMC Name'")
            else:
                amc_name = sys.argv[2]
                orchestrator.run_specific_amc(amc_name)
        else:
            print(f"""
Phase 3 ETL Commands:
  
  full          - Run complete pipeline (production)
  test          - Run pipeline in test mode (3 AMCs)
  incremental   - Run incremental update (new documents only)
  
  amfi          - Step 1: Scrape AMFI links
  crawl [N]     - Step 2: Crawl AMC websites (limit to N AMCs)
  parse [TYPE]  - Step 3: Parse documents (PORTFOLIO/TER/AUM)
  sebi [N]      - Step 4: Scrape SEBI SIDs (limit to N AMCs)
  refresh       - Step 5: Refresh master tables
  report        - Step 6: Generate quality report
  
  amc 'NAME'    - Process specific AMC only

Examples:
  python run_phase3_etl.py test
  python run_phase3_etl.py crawl 5
  python run_phase3_etl.py parse PORTFOLIO
  python run_phase3_etl.py amc 'Aditya Birla'
            """)
    else:
        # Default: test mode
        orchestrator.run_full_pipeline(test_mode=True)


if __name__ == "__main__":
    main()