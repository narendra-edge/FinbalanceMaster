"""
run_phase3_enhanced.py
----------------------
Complete Phase 3 orchestrator with:
1. CSV-based AMC links
2. YAML-driven step execution
3. Smart file organization
4. Intelligent parsing
5. Database ingestion with validation

Usage:
  python run_phase3_enhanced.py full          # Full pipeline (all AMCs)
  python run_phase3_enhanced.py test          # Test mode (2 AMCs)
  python run_phase3_enhanced.py portfolio     # Only portfolio documents
  python run_phase3_enhanced.py amc "Axis"    # Specific AMC only
"""

import logging
import sys
from pathlib import Path
from datetime import datetime
from typing import Optional, List
from sqlalchemy import text

from Etl.utils import settings, engine, ensure_dir
from Etl.universal_amc_crawler import UniversalAmcCrawler
from Etl.real_world_amc_parser import (
    PortfolioStatementParser,
    AaumReportParser,
    ExpenseRatioParser
)

logger = logging.getLogger("mf.etl.phase3_enhanced")


class Phase3EnhancedOrchestrator:
    """
    Master orchestrator for Phase 3 with enhanced AMC crawling.
    """
    
    def __init__(self):
        self.db_url = str(engine.url)
        self.settings = settings
        self.data_root = Path(settings['paths']['data_root'])
        self.downloads_dir = Path(settings['paths']['downloads_dir'])
        
        # Initialize crawler
        self.crawler = UniversalAmcCrawler(self.db_url, settings)
        
        # Initialize parsers
        self.portfolio_parser = PortfolioStatementParser(self.db_url)
        self.aaum_parser = AaumReportParser(self.db_url)
        self.ter_parser = ExpenseRatioParser(self.db_url)
        
        logger.info("📁 Phase 3 Enhanced Orchestrator initialized")
    
    # =====================================================================
    # STEP 1: DOWNLOAD DOCUMENTS
    # =====================================================================
    
    def run_download_phase(self, doc_types: List[str], limit_amcs: Optional[int] = None):
        """
        Download documents from all AMC websites.
        
        Args:
            doc_types: ['portfolio', 'aaum', 'factsheet', 'ter', 'sid']
            limit_amcs: Limit for testing
        """
        logger.info("=" * 70)
        logger.info("PHASE 1: DOWNLOAD DOCUMENTS")
        logger.info("=" * 70)
        
        self.crawler.run(doc_types=doc_types, limit_amcs=limit_amcs)
        
        logger.info("✅ Download phase complete")
    
    # =====================================================================
    # STEP 2: PARSE PORTFOLIO FILES
    # =====================================================================
    
    def run_parse_portfolio_phase(self):
        """
        Parse all portfolio files (Excel/CSV) in downloads/portfolio/.
        """
        logger.info("=" * 70)
        logger.info("PHASE 2: PARSE PORTFOLIO FILES")
        logger.info("=" * 70)
        
        portfolio_dir = self.downloads_dir / 'portfolio'
        
        if not portfolio_dir.exists():
            logger.warning("⚠️  No portfolio directory found")
            return
        
        total_files = 0
        total_holdings = 0
        
        # Process each AMC folder
        for amc_folder in portfolio_dir.iterdir():
            if not amc_folder.is_dir():
                continue
            
            amc_name = amc_folder.name
            logger.info(f"\n🏢 Processing AMC: {amc_name}")
            
            # Get all Excel/CSV files
            files = list(amc_folder.glob("*.xlsx")) + \
                    list(amc_folder.glob("*.xls")) + \
                    list(amc_folder.glob("*.csv"))
            
            logger.info(f"📁 Found {len(files)} portfolio files")
            
            for file in files:
                try:
                    logger.info(f"📊 Parsing: {file.name}")
                    
                    # Parse portfolio
                    result = self.portfolio_parser.parse_portfolio_csv(file)
                    
                    if not result or not result.get('holdings'):
                        logger.warning(f"⚠️  No holdings extracted from {file.name}")
                        continue
                    
                    holdings = result['holdings']
                    scheme_info = result['scheme_info']
                    
                    logger.info(f"✓ Extracted {len(holdings)} holdings")
                    
                    # Match to scheme_id (simplified - production would use fuzzy matching)
                    scheme_id = self._match_scheme_id(scheme_info.get('scheme_name'), amc_name)
                    
                    if scheme_id:
                        # Save to database
                        self.portfolio_parser.save_to_database(result, scheme_id)
                        logger.info(f"✅ Saved to database: {scheme_id}")
                    else:
                        logger.warning(f"⚠️  Could not match scheme: {scheme_info.get('scheme_name')}")
                    
                    total_files += 1
                    total_holdings += len(holdings)
                
                except Exception as e:
                    logger.error(f"❌ Failed to parse {file.name}: {e}")
        
        logger.info(f"\n📊 Portfolio Parsing Summary:")
        logger.info(f"   Files processed: {total_files}")
        logger.info(f"   Total holdings: {total_holdings}")
        logger.info("✅ Portfolio parsing complete")
    
    # =====================================================================
    # STEP 3: PARSE AAUM FILES
    # =====================================================================
    
    def run_parse_aaum_phase(self):
        """
        Parse AAUM grid files.
        """
        logger.info("=" * 70)
        logger.info("PHASE 3: PARSE AAUM FILES")
        logger.info("=" * 70)
        
        aaum_dir = self.downloads_dir / 'aaum'
        
        if not aaum_dir.exists():
            logger.warning("⚠️  No AAUM directory found")
            return
        
        total_files = 0
        total_schemes = 0
        
        for amc_folder in aaum_dir.iterdir():
            if not amc_folder.is_dir():
                continue
            
            amc_name = amc_folder.name
            logger.info(f"\n🏢 Processing AMC: {amc_name}")
            
            files = list(amc_folder.glob("*.xlsx")) + list(amc_folder.glob("*.xls"))
            
            for file in files:
                try:
                    logger.info(f"📊 Parsing: {file.name}")
                    
                    result = self.aaum_parser.parse_aaum_csv(file)
                    
                    if not result or not result.get('schemes'):
                        logger.warning(f"⚠️  No schemes extracted from {file.name}")
                        continue
                    
                    schemes = result['schemes']
                    logger.info(f"✓ Extracted {len(schemes)} schemes")
                    
                    # Save to database (implement save_to_db in parser)
                    # self.aaum_parser.save_to_database(result, amc_name)
                    
                    total_files += 1
                    total_schemes += len(schemes)
                
                except Exception as e:
                    logger.error(f"❌ Failed to parse {file.name}: {e}")
        
        logger.info(f"\n💰 AAUM Parsing Summary:")
        logger.info(f"   Files processed: {total_files}")
        logger.info(f"   Total schemes: {total_schemes}")
        logger.info("✅ AAUM parsing complete")
    
    # =====================================================================
    # STEP 4: PARSE TER FILES
    # =====================================================================
    
    def run_parse_ter_phase(self):
        """
        Parse TER master files.
        """
        logger.info("=" * 70)
        logger.info("PHASE 4: PARSE TER FILES")
        logger.info("=" * 70)
        
        ter_dir = self.downloads_dir / 'ter'
        
        if not ter_dir.exists():
            logger.warning("⚠️  No TER directory found")
            return
        
        total_files = 0
        total_plans = 0
        
        for amc_folder in ter_dir.iterdir():
            if not amc_folder.is_dir():
                continue
            
            amc_name = amc_folder.name
            logger.info(f"\n🏢 Processing AMC: {amc_name}")
            
            files = list(amc_folder.glob("*.xlsx")) + list(amc_folder.glob("*.xls"))
            
            for file in files:
                try:
                    logger.info(f"📊 Parsing: {file.name}")
                    
                    result = self.ter_parser.parse_ter_file(file)
                    
                    if not result or not result.get('ter_data'):
                        logger.warning(f"⚠️  No TER data extracted from {file.name}")
                        continue
                    
                    ter_data = result['ter_data']
                    logger.info(f"✓ Extracted TER for {len(ter_data)} plans")
                    
                    total_files += 1
                    total_plans += len(ter_data)
                
                except Exception as e:
                    logger.error(f"❌ Failed to parse {file.name}: {e}")
        
        logger.info(f"\n📈 TER Parsing Summary:")
        logger.info(f"   Files processed: {total_files}")
        logger.info(f"   Total plans: {total_plans}")
        logger.info("✅ TER parsing complete")
    
    # =====================================================================
    # STEP 5: PARSE FACTSHEET & SID PDFs
    # =====================================================================
    
    def run_parse_pdf_phase(self, doc_type: str):
        """
        Parse factsheet or SID PDFs.
        
        Args:
            doc_type: 'factsheet' or 'sid'
        """
        logger.info("=" * 70)
        logger.info(f"PHASE 5: PARSE {doc_type.upper()} PDFs")
        logger.info("=" * 70)
        
        pdf_dir = self.downloads_dir / doc_type
        
        if not pdf_dir.exists():
            logger.warning(f"⚠️  No {doc_type} directory found")
            return
        
        from Etl.statutory_pdf_parser_enhanced import StatutoryPdfParser
        
        parser = StatutoryPdfParser(self.db_url)
        
        # Get all pending documents from database
        with engine.connect() as conn:
            result = conn.execute(
                text("""
                    SELECT id, amc_id, scheme_id, document_type,
                           document_date, local_file_path
                    FROM statutory_document_downloads
                    WHERE document_type = :doc_type
                      AND parsing_status = 'PENDING'
                      AND local_file_path IS NOT NULL
                    ORDER BY document_date DESC
                    LIMIT 500
                """),
                {"doc_type": doc_type.upper()}
            )
            
            docs = [dict(row._mapping) for row in result.fetchall()]
        
        if not docs:
            logger.info(f"ℹ️  No pending {doc_type} documents")
            return
        
        logger.info(f"📋 Found {len(docs)} {doc_type} documents to parse")
        
        success_count = 0
        
        for doc in docs:
            try:
                success, validation = parser.parse_document(doc)
                
                if success:
                    success_count += 1
            
            except Exception as e:
                logger.error(f"❌ Parse failed: {e}")
        
        logger.info(f"✅ Parsed {success_count}/{len(docs)} {doc_type} documents")
    
    # =====================================================================
    # STEP 6: REFRESH MASTER TABLES
    # =====================================================================
    
    def run_refresh_masters(self):
        """
        Run SQL procedures to refresh master tables.
        """
        logger.info("=" * 70)
        logger.info("PHASE 6: REFRESH MASTER TABLES")
        logger.info("=" * 70)
        
        try:
            with engine.begin() as conn:
                logger.info("📊 Refreshing portfolio_holdings_master...")
                conn.execute(text("CALL sp_refresh_portfolio_holdings_master()"))
                
                logger.info("💰 Refreshing aum_master...")
                conn.execute(text("CALL sp_refresh_aum_master()"))
                
                logger.info("📈 Refreshing ter_master...")
                conn.execute(text("CALL sp_refresh_ter_master()"))
            
            logger.info("✅ Master tables refreshed successfully")
        
        except Exception as e:
            logger.error(f"❌ Master refresh failed: {e}")
            raise
    
    # =====================================================================
    # STEP 7: DATA QUALITY REPORT
    # =====================================================================
    
    def run_quality_report(self):
        """
        Generate data quality report.
        """
        logger.info("=" * 70)
        logger.info("PHASE 7: DATA QUALITY REPORT")
        logger.info("=" * 70)
        
        try:
            with engine.connect() as conn:
                # Portfolio stats
                portfolio_stats = conn.execute(text("""
                    SELECT 
                        COUNT(DISTINCT scheme_id) as schemes_count,
                        COUNT(*) as total_holdings,
                        COUNT(DISTINCT as_of_date) as unique_dates,
                        COUNT(CASE WHEN isin_code IS NOT NULL THEN 1 END) as holdings_with_isin,
                        AVG(portfolio_percentage) as avg_holding_pct
                    FROM portfolio_holdings_raw
                    WHERE created_at >= CURRENT_DATE - INTERVAL '7 days'
                """)).fetchone()
                
                if portfolio_stats:
                    logger.info(f"""
📊 Portfolio Holdings (Last 7 days):
   Schemes: {portfolio_stats[0]}
   Total Holdings: {portfolio_stats[1]}
   Unique Dates: {portfolio_stats[2]}
   Holdings with ISIN: {portfolio_stats[3]} ({portfolio_stats[3]/portfolio_stats[1]*100:.1f}%)
   Avg Holding %: {portfolio_stats[4]:.2f}%
                    """)
                
                # Download success rate
                download_stats = conn.execute(text("""
                    SELECT 
                        document_type,
                        COUNT(*) as total,
                        COUNT(CASE WHEN download_status = 'DOWNLOADED' THEN 1 END) as successful,
                        COUNT(CASE WHEN parsing_status = 'SUCCESS' THEN 1 END) as parsed
                    FROM statutory_document_downloads
                    WHERE created_at >= CURRENT_DATE - INTERVAL '7 days'
                    GROUP BY document_type
                    ORDER BY document_type
                """)).fetchall()
                
                if download_stats:
                    logger.info("\n📥 Download & Parse Statistics (Last 7 days):")
                    logger.info(f"{'Type':<15} {'Total':<8} {'Downloaded':<12} {'Parsed':<8}")
                    logger.info("-" * 45)
                    
                    for stat in download_stats:
                        doc_type, total, downloaded, parsed = stat
                        down_pct = (downloaded/total*100) if total > 0 else 0
                        parse_pct = (parsed/downloaded*100) if downloaded > 0 else 0
                        logger.info(f"{doc_type:<15} {total:<8} {downloaded:<5} ({down_pct:.0f}%)   {parsed:<5} ({parse_pct:.0f}%)")
            
            logger.info("\n✅ Quality report generated")
        
        except Exception as e:
            logger.error(f"❌ Report generation failed: {e}")
    
    # =====================================================================
    # UTILITY METHODS
    # =====================================================================
    
    def _match_scheme_id(self, scheme_name: Optional[str], amc_name: str) -> Optional[str]:
        """
        Match scheme name to scheme_master_final.scheme_id.
        Uses fuzzy matching with pg_trgm.
        """
        if not scheme_name:
            return None
        
        with engine.connect() as conn:
            # Try exact match first
            result = conn.execute(
                text("""
                    SELECT scheme_id FROM scheme_master_final
                    WHERE lower(canonical_scheme_name) = lower(:name)
                    LIMIT 1
                """),
                {"name": scheme_name}
            ).fetchone()
            
            if result:
                return result[0]
            
            # Try fuzzy match
            result = conn.execute(
                text("""
                    SELECT scheme_id, canonical_scheme_name,
                           similarity(lower(canonical_scheme_name), lower(:name)) AS sim
                    FROM scheme_master_final
                    WHERE amc_name ILIKE :amc_pattern
                      AND similarity(lower(canonical_scheme_name), lower(:name)) > 0.5
                    ORDER BY sim DESC
                    LIMIT 1
                """),
                {"name": scheme_name, "amc_pattern": f"%{amc_name}%"}
            ).fetchone()
            
            if result and result[2] > 0.6:
                logger.info(f"🔗 Fuzzy matched: '{scheme_name}' → '{result[1]}' (sim={result[2]:.2f})")
                return result[0]
        
        return None
    
    # =====================================================================
    # MAIN PIPELINES
    # =====================================================================
    
    def run_full_pipeline(self, test_mode: bool = False):
        """
        Execute complete Phase 3 pipeline.
        """
        start_time = datetime.now()
        
        logger.info("=" * 70)
        logger.info("🚀 PHASE 3 ENHANCED ETL PIPELINE START")
        logger.info(f"Start Time: {start_time}")
        logger.info(f"Test Mode: {test_mode}")
        logger.info("=" * 70)
        
        limit = 2 if test_mode else None
        
        try:
            # Step 1: Download all documents
            self.run_download_phase(
                doc_types=['portfolio', 'aaum', 'factsheet', 'ter', 'sid'],
                limit_amcs=limit
            )
            
            # Step 2: Parse Excel/CSV files
            self.run_parse_portfolio_phase()
            self.run_parse_aaum_phase()
            self.run_parse_ter_phase()
            
            # Step 3: Parse PDFs
            self.run_parse_pdf_phase('factsheet')
            self.run_parse_pdf_phase('sid')
            
            # Step 4: Refresh masters
            self.run_refresh_masters()
            
            # Step 5: Quality report
            self.run_quality_report()
            
            end_time = datetime.now()
            duration = end_time - start_time
            
            logger.info("=" * 70)
            logger.info("🎉 PHASE 3 PIPELINE COMPLETE")
            logger.info(f"Duration: {duration}")
            logger.info("=" * 70)
        
        except Exception as e:
            logger.exception(f"❌ Pipeline failed: {e}")
            raise
    
    def run_incremental_update(self):
        """
        Incremental update (for scheduled runs).
        """
        logger.info("🔄 Starting Phase 3 Incremental Update")
        
        # Only download changed documents
        self.run_download_phase(doc_types=['portfolio', 'aaum'])
        
        # Parse new files
        self.run_parse_portfolio_phase()
        self.run_parse_aaum_phase()
        
        # Refresh masters
        self.run_refresh_masters()
        
        logger.info("✅ Incremental update complete")


def configure_logging():
    """Configure logging."""
    logs_dir = Path(settings['paths']['logs_dir'])
    logs_dir.mkdir(parents=True, exist_ok=True)
    
    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
    log_file = logs_dir / f"phase3_enhanced_{timestamp}.log"
    
    logging.basicConfig(
        level=logging.INFO,
        format="%(asctime)s [%(levelname)s] %(name)s - %(message)s",
        handlers=[
            logging.FileHandler(log_file, encoding='utf-8'),
            logging.StreamHandler(sys.stdout)
        ]
    )
    
    logger.info(f"📜 Logging to: {log_file}")


def main():
    """Main entry point."""
    configure_logging()
    
    orchestrator = Phase3EnhancedOrchestrator()
    
    if len(sys.argv) > 1:
        command = sys.argv[1].lower()
        
        if command == 'full':
            orchestrator.run_full_pipeline(test_mode=False)
        
        elif command == 'test':
            orchestrator.run_full_pipeline(test_mode=True)
        
        elif command == 'incremental':
            orchestrator.run_incremental_update()
        
        elif command in ['portfolio', 'aaum', 'ter', 'factsheet', 'sid']:
            # Download specific document type only
            orchestrator.run_download_phase([command], limit_amcs=None)
            
            # Parse based on type
            if command == 'portfolio':
                orchestrator.run_parse_portfolio_phase()
            elif command == 'aaum':
                orchestrator.run_parse_aaum_phase()
            elif command == 'ter':
                orchestrator.run_parse_ter_phase()
            else:
                orchestrator.run_parse_pdf_phase(command)
        
        elif command == 'parse':
            # Parse only (no download)
            orchestrator.run_parse_portfolio_phase()
            orchestrator.run_parse_aaum_phase()
            orchestrator.run_parse_ter_phase()
        
        elif command == 'refresh':
            orchestrator.run_refresh_masters()
        
        elif command == 'report':
            orchestrator.run_quality_report()
        
        else:
            print(f"""
Phase 3 Enhanced ETL Commands:

  full          - Run complete pipeline (production)
  test          - Run pipeline in test mode (2 AMCs)
  incremental   - Run incremental update
  
  portfolio     - Download + parse portfolio only
  aaum          - Download + parse AAUM only
  ter           - Download + parse TER only
  factsheet     - Download + parse factsheets only
  sid           - Download + parse SIDs only
  
  parse         - Parse all downloaded files (no download)
  refresh       - Refresh master tables only
  report        - Generate quality report only

Examples:
  python run_phase3_enhanced.py test
  python run_phase3_enhanced.py portfolio
  python run_phase3_enhanced.py parse
            """)
    else:
        # Default: test mode
        orchestrator.run_full_pipeline(test_mode=True)


if __name__ == "__main__":
    main()
