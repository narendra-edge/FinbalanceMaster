"""
amc_website_crawler.py
----------------------
Crawls individual AMC websites to download statutory documents:
- Portfolio disclosures (Monthly/Halfyearly/Annual)
- TER (Total Expense Ratio)
- AUM/AAUM data
- Fact sheets

Uses Playwright for JavaScript-heavy sites.
"""

import logging
import re
from pathlib import Path
from datetime import datetime, timedelta
from typing import List, Dict, Optional
from urllib.parse import urljoin, urlparse

from playwright.sync_api import sync_playwright, TimeoutError as PlaywrightTimeout
from sqlalchemy import create_engine, text
import requests

logger = logging.getLogger("mf.etl.amc_crawler")


class AmcWebsiteCrawler:
    """
    Intelligent crawler for AMC websites to find and download statutory documents.
    """
    
    def __init__(self, db_url: str, download_base_dir: Path):
        self.db_url = db_url
        self.engine = create_engine(db_url, pool_pre_ping=True)
        self.download_base_dir = Path(download_base_dir)
        self.download_base_dir.mkdir(parents=True, exist_ok=True)
        
        # Search patterns for different document types
        self.patterns = {
            'PORTFOLIO': [
                r'portfolio.*disclosure',
                r'scheme.*portfolio',
                r'holdings?.*disclosure'
            ],
            'TER': [
                r'total.*expense.*ratio',
                r'ter.*disclosure',
                r'expense.*ratio',
                r'scheme.*expenses'
            ],
            'AUM': [
                r'aum.*disclosure',
                r'assets?.*under.*management',
                r'aaum',
                r'average.*aum'
            ],
            'FACTSHEET': [
                r'fact.*sheet',
                r'factsheet',
                r'scheme.*fact',
                r'monthly.*fact'
            ]
        }
    
    def get_pending_amcs(self) -> List[Dict]:
        """
        Get list of AMCs that need document downloads.
        """
        with self.engine.connect() as conn:
            result = conn.execute(
                text("""
                    SELECT id, amc_id, amc_name,
                           portfolio_monthly_url,
                           portfolio_halfyearly_url,
                           portfolio_annual_url,
                           base_website_url
                    FROM amc_website_links
                    WHERE scrape_status IN ('PENDING', 'SUCCESS')
                      AND (last_scraped_at IS NULL 
                           OR last_scraped_at < now() - INTERVAL '7 days')
                    ORDER BY amc_name
                """)
            )
            return [dict(row._mapping) for row in result.fetchall()]
    
    def find_documents_on_page(self, page_url: str, doc_type: str) -> List[Dict]:
        """
        Find downloadable documents on a webpage using pattern matching.
        
        Args:
            page_url: URL to crawl
            doc_type: PORTFOLIO / TER / AUM / FACTSHEET
            
        Returns:
            List of found documents with URLs and metadata
        """
        logger.info(f"🔍 Searching {doc_type} documents at {page_url}")
        
        found_docs = []
        
        try:
            with sync_playwright() as p:
                browser = p.chromium.launch(headless=True)
                context = browser.new_context()
                page = context.new_page()
                
                page.goto(page_url, wait_until="networkidle", timeout=60000)
                
                # Wait for dynamic content
                page.wait_for_timeout(3000)
                
                # Get all links on the page
                links = page.query_selector_all('a[href]')
                
                for link in links:
                    try:
                        href = link.get_attribute('href')
                        text = link.inner_text().strip().lower()
                        
                        if not href:
                            continue
                        
                        # Make absolute URL
                        full_url = urljoin(page_url, href)
                        
                        # Check if link matches document type patterns
                        matches = False
                        for pattern in self.patterns.get(doc_type, []):
                            if re.search(pattern, text, re.IGNORECASE):
                                matches = True
                                break
                        
                        if not matches:
                            continue
                        
                        # Check if it's a downloadable file
                        file_ext = Path(urlparse(full_url).path).suffix.lower()
                        if file_ext not in ['.pdf', '.xlsx', '.xls', '.csv', '.zip']:
                            # Might be a link to another page with documents
                            # Add for recursive crawl
                            continue
                        
                        # Extract date from text or URL if possible
                        doc_date = self.extract_date_from_text(text + ' ' + full_url)
                        
                        found_docs.append({
                            'url': full_url,
                            'title': text,
                            'file_type': file_ext[1:],  # Remove dot
                            'doc_date': doc_date
                        })
                        
                        logger.debug(f"✓ Found: {text[:50]}... ({file_ext})")
                    
                    except Exception as e:
                        logger.debug(f"Error processing link: {e}")
                        continue
                
                browser.close()
        
        except PlaywrightTimeout:
            logger.warning(f"⏱ Timeout loading {page_url}")
        except Exception as e:
            logger.error(f"❌ Error crawling {page_url}: {e}")
        
        logger.info(f"✅ Found {len(found_docs)} {doc_type} documents")
        return found_docs
    
    def extract_date_from_text(self, text: str) -> Optional[str]:
        """
        Extract date from text using regex patterns.
        Tries to find: Mar 2024, March 2024, 03-2024, etc.
        """
        patterns = [
            r'(jan|feb|mar|apr|may|jun|jul|aug|sep|oct|nov|dec)[a-z]*[\s-]*20\d{2}',
            r'(0?[1-9]|1[0-2])[\s/-]20\d{2}',
            r'20\d{2}[\s/-](0?[1-9]|1[0-2])'
        ]
        
        text_lower = text.lower()
        
        for pattern in patterns:
            match = re.search(pattern, text_lower)
            if match:
                date_str = match.group(0)
                # Normalize to YYYY-MM format
                try:
                    # Convert month names
                    month_map = {
                        'jan': '01', 'feb': '02', 'mar': '03', 'apr': '04',
                        'may': '05', 'jun': '06', 'jul': '07', 'aug': '08',
                        'sep': '09', 'oct': '10', 'nov': '11', 'dec': '12'
                    }
                    
                    for month_name, month_num in month_map.items():
                        if month_name in date_str:
                            year = re.search(r'20\d{2}', date_str).group(0)
                            return f"{year}-{month_num}-01"
                    
                    # Try numeric format
                    nums = re.findall(r'\d+', date_str)
                    if len(nums) == 2:
                        if len(nums[0]) == 4:  # YYYY-MM
                            return f"{nums[0]}-{nums[1].zfill(2)}-01"
                        else:  # MM-YYYY
                            return f"{nums[1]}-{nums[0].zfill(2)}-01"
                
                except:
                    pass
        
        return None
    
    def download_document(self, url: str, amc_id: str, doc_type: str) -> Optional[Path]:
        """
        Download document from URL and save locally.
        
        Returns:
            Path to downloaded file or None if failed
        """
        try:
            # Create AMC-specific folder
            amc_dir = self.download_base_dir / "statutory" / str(amc_id) / doc_type.lower()
            amc_dir.mkdir(parents=True, exist_ok=True)
            
            # Generate filename
            filename = Path(urlparse(url).path).name
            if not filename or len(filename) < 3:
                filename = f"{doc_type}_{datetime.now().strftime('%Y%m%d_%H%M%S')}.pdf"
            
            filepath = amc_dir / filename
            
            logger.info(f"⬇️  Downloading {url}")
            
            response = requests.get(url, timeout=120, stream=True)
            response.raise_for_status()
            
            with open(filepath, 'wb') as f:
                for chunk in response.iter_content(chunk_size=8192):
                    f.write(chunk)
            
            file_size_kb = filepath.stat().st_size // 1024
            logger.info(f"✅ Downloaded {file_size_kb} KB → {filepath.name}")
            
            return filepath
        
        except Exception as e:
            logger.error(f"❌ Download failed for {url}: {e}")
            return None
    
    def track_download_in_db(self, amc_id: str, scheme_id: Optional[str],
                             doc_type: str, frequency: Optional[str],
                             doc_date: Optional[str], download_url: str,
                             local_path: Optional[Path], status: str):
        """
        Track downloaded document in statutory_document_downloads table.
        """
        with self.engine.begin() as conn:
            conn.execute(
                text("""
                    INSERT INTO statutory_document_downloads (
                        amc_id, scheme_id, document_type, frequency,
                        document_date, download_url, local_file_path,
                        file_format, file_size_kb, download_status,
                        download_attempts, last_download_attempt,
                        created_at, updated_at
                    ) VALUES (
                        :amc_id, :scheme_id, :doc_type, :frequency,
                        :doc_date, :download_url, :local_path,
                        :file_format, :file_size_kb, :status,
                        1, now(), now(), now()
                    )
                    ON CONFLICT (amc_id, COALESCE(scheme_id, 'AMC_LEVEL'), 
                                 document_type, frequency, document_date)
                    DO UPDATE SET
                        download_url = EXCLUDED.download_url,
                        local_file_path = COALESCE(EXCLUDED.local_file_path, 
                                                   statutory_document_downloads.local_file_path),
                        download_status = EXCLUDED.download_status,
                        download_attempts = statutory_document_downloads.download_attempts + 1,
                        last_download_attempt = now(),
                        updated_at = now()
                """),
                {
                    "amc_id": amc_id,
                    "scheme_id": scheme_id,
                    "doc_type": doc_type,
                    "frequency": frequency,
                    "doc_date": doc_date,
                    "download_url": download_url,
                    "local_path": str(local_path) if local_path else None,
                    "file_format": local_path.suffix[1:].upper() if local_path else None,
                    "file_size_kb": (local_path.stat().st_size // 1024) if local_path else None,
                    "status": status
                }
            )
    
    def crawl_amc(self, amc_info: Dict):
        """
        Crawl single AMC website for all document types.
        """
        amc_name = amc_info['amc_name']
        amc_id = amc_info['amc_id']
        
        logger.info(f"🏢 Crawling {amc_name}...")
        
        # Determine which URLs to crawl
        urls_to_crawl = []
        
        # Portfolio URLs
        if amc_info.get('portfolio_monthly_url'):
            urls_to_crawl.append(('PORTFOLIO', 'MONTHLY', amc_info['portfolio_monthly_url']))
        if amc_info.get('portfolio_halfyearly_url'):
            urls_to_crawl.append(('PORTFOLIO', 'HALFYEARLY', amc_info['portfolio_halfyearly_url']))
        if amc_info.get('portfolio_annual_url'):
            urls_to_crawl.append(('PORTFOLIO', 'ANNUAL', amc_info['portfolio_annual_url']))
        
        # Base website - search for TER, AUM, Factsheet
        if amc_info.get('base_website_url'):
            for doc_type in ['TER', 'AUM', 'FACTSHEET']:
                urls_to_crawl.append((doc_type, None, amc_info['base_website_url']))
        
        # Crawl each URL
        for doc_type, frequency, url in urls_to_crawl:
            if not url:
                continue
            
            try:
                found_docs = self.find_documents_on_page(url, doc_type)
                
                for doc in found_docs:
                    # Download document
                    local_path = self.download_document(doc['url'], amc_id, doc_type)
                    
                    # Track in database
                    status = 'DOWNLOADED' if local_path else 'FAILED'
                    self.track_download_in_db(
                        amc_id=amc_id,
                        scheme_id=None,  # AMC-level document
                        doc_type=doc_type,
                        frequency=frequency,
                        doc_date=doc.get('doc_date'),
                        download_url=doc['url'],
                        local_path=local_path,
                        status=status
                    )
            
            except Exception as e:
                logger.error(f"❌ Error crawling {doc_type} for {amc_name}: {e}")
        
        # Update last_scraped_at
        with self.engine.begin() as conn:
            conn.execute(
                text("""
                    UPDATE amc_website_links
                    SET last_scraped_at = now(),
                        scrape_status = 'SUCCESS'
                    WHERE id = :id
                """),
                {"id": amc_info['id']}
            )
        
        logger.info(f"✅ Completed crawl for {amc_name}")
    
    def run(self, limit: Optional[int] = None):
        """
        Run crawler for all pending AMCs.
        
        Args:
            limit: Maximum number of AMCs to crawl (None = all)
        """
        logger.info("🚀 Starting AMC website crawler...")
        
        pending_amcs = self.get_pending_amcs()
        
        if limit:
            pending_amcs = pending_amcs[:limit]
        
        logger.info(f"📋 Found {len(pending_amcs)} AMCs to crawl")
        
        for i, amc_info in enumerate(pending_amcs, 1):
            logger.info(f"[{i}/{len(pending_amcs)}] Processing {amc_info['amc_name']}")
            self.crawl_amc(amc_info)
        
        logger.info("🎉 AMC crawler completed!")


def main():
    logging.basicConfig(
        level=logging.INFO,
        format="%(asctime)s [%(levelname)s] %(name)s - %(message)s"
    )
    
    from Etl.utils import settings, engine
    
    download_dir = Path(settings['paths']['data_root']) / 'downloads'
    
    crawler = AmcWebsiteCrawler(
        db_url=str(engine.url),
        download_base_dir=download_dir
    )
    
    # Crawl first 5 AMCs as test
    crawler.run(limit=5)


if __name__ == "__main__":
    main()
