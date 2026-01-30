"""
sebi_sid_scraper.py
-------------------
Scrapes SEBI website for Scheme Information Documents (SID) to extract:
1. Fund manager details (name, qualification, experience)
2. Investment objectives
3. Asset allocation patterns
4. Risk factors
5. Exit load structure
6. Benchmark information

SEBI URL: https://www.sebi.gov.in/sebiweb/other/OtherAction.do?doMutualFund=yes&mftype=2
"""

import logging
import re
from pathlib import Path
from typing import List, Dict, Optional
from datetime import datetime
from urllib.parse import urljoin

import requests
from bs4 import BeautifulSoup
from playwright.sync_api import sync_playwright, TimeoutError as PlaywrightTimeout
from sqlalchemy import create_engine, text
import pdfplumber
from PyPDF2 import PdfReader

logger = logging.getLogger("mf.etl.sebi_scraper")


class SebiSidScraper:
    """
    Scrapes SEBI website for Scheme Information Documents (SIDs).
    """
    
    def __init__(self, db_url: str, download_dir: Path):
        self.db_url = db_url
        self.engine = create_engine(db_url, pool_pre_ping=True)
        self.download_dir = Path(download_dir)
        self.download_dir.mkdir(parents=True, exist_ok=True)
        
        self.sebi_base_url = "https://www.sebi.gov.in"
        self.sebi_mf_url = f"{self.sebi_base_url}/sebiweb/other/OtherAction.do?doMutualFund=yes&mftype=2"
    
    def get_amc_list_from_sebi(self) -> List[Dict]:
        """
        Scrape list of AMCs from SEBI website.
        
        Returns:
            List of AMC info with SEBI scheme links
        """
        logger.info("🌐 Fetching AMC list from SEBI...")
        
        amc_list = []
        
        try:
            with sync_playwright() as p:
                browser = p.chromium.launch(headless=True)
                context = browser.new_context()
                page = context.new_page()
                
                page.goto(self.sebi_mf_url, wait_until="networkidle", timeout=60000)
                page.wait_for_timeout(3000)
                
                # SEBI page typically has dropdown or table with AMCs
                # Look for AMC selection dropdown
                amc_dropdown = page.query_selector('select#mutualfundcompanyName')
                
                if amc_dropdown:
                    options = amc_dropdown.query_selector_all('option')
                    
                    for option in options:
                        value = option.get_attribute('value')
                        text = option.inner_text().strip()
                        
                        if not value or not text:
                            continue
                        
                        amc_list.append({
                            'amc_name': text,
                            'sebi_amc_code': value
                        })
                        
                        logger.debug(f"✓ Found AMC: {text}")
                
                # Alternative: Look for direct links to SID documents
                else:
                    links = page.query_selector_all('a[href*="SID"], a[href*="sid"]')
                    
                    for link in links:
                        href = link.get_attribute('href')
                        text = link.inner_text().strip()
                        
                        if not href:
                            continue
                        
                        full_url = urljoin(self.sebi_base_url, href)
                        
                        # Extract AMC name from text
                        amc_name = text.split('-')[0].strip() if '-' in text else text
                        
                        amc_list.append({
                            'amc_name': amc_name,
                            'sid_url': full_url
                        })
                
                browser.close()
        
        except Exception as e:
            logger.error(f"❌ Failed to fetch AMC list from SEBI: {e}")
        
        logger.info(f"✅ Found {len(amc_list)} AMCs on SEBI")
        return amc_list
    
    def get_scheme_sids_for_amc(self, sebi_amc_code: str) -> List[Dict]:
        """
        Get list of scheme SID documents for a specific AMC.
        
        Args:
            sebi_amc_code: AMC code from SEBI dropdown
            
        Returns:
            List of scheme info with SID URLs
        """
        logger.info(f"📄 Fetching SID documents for AMC code {sebi_amc_code}")
        
        schemes = []
        
        try:
            with sync_playwright() as p:
                browser = p.chromium.launch(headless=True)
                context = browser.new_context()
                page = context.new_page()
                
                page.goto(self.sebi_mf_url, wait_until="networkidle", timeout=60000)
                
                # Select AMC from dropdown
                page.select_option('select#mutualfundcompanyName', sebi_amc_code)
                page.wait_for_timeout(2000)
                
                # Wait for schemes table to load
                page.wait_for_selector('table', timeout=10000)
                
                # Extract scheme information
                rows = page.query_selector_all('table tr')
                
                for row in rows[1:]:  # Skip header
                    cols = row.query_selector_all('td')
                    
                    if len(cols) < 3:
                        continue
                    
                    scheme_name = cols[0].inner_text().strip()
                    
                    # Look for SID link in row
                    sid_link = row.query_selector('a[href*="SID"], a[href*="sid"]')
                    
                    if sid_link:
                        href = sid_link.get_attribute('href')
                        full_url = urljoin(self.sebi_base_url, href)
                        
                        schemes.append({
                            'scheme_name': scheme_name,
                            'sid_url': full_url,
                            'document_type': 'SID'
                        })
                        
                        logger.debug(f"✓ Found SID for: {scheme_name}")
                
                browser.close()
        
        except Exception as e:
            logger.error(f"❌ Failed to get SID documents: {e}")
        
        logger.info(f"✅ Found {len(schemes)} SID documents")
        return schemes
    
    def download_sid_document(self, sid_url: str, amc_name: str, scheme_name: str) -> Optional[Path]:
        """
        Download SID PDF document.
        
        Returns:
            Path to downloaded file or None
        """
        try:
            # Create AMC-specific folder
            amc_dir = self.download_dir / "sebi_sids" / amc_name.replace(' ', '_')
            amc_dir.mkdir(parents=True, exist_ok=True)
            
            # Generate filename
            safe_scheme_name = re.sub(r'[^\w\s-]', '', scheme_name)[:50]
            filename = f"SID_{safe_scheme_name}_{datetime.now().strftime('%Y%m%d')}.pdf"
            filepath = amc_dir / filename
            
            logger.info(f"⬇️  Downloading SID from {sid_url}")
            
            response = requests.get(sid_url, timeout=120, stream=True)
            response.raise_for_status()
            
            with open(filepath, 'wb') as f:
                for chunk in response.iter_content(chunk_size=8192):
                    f.write(chunk)
            
            file_size_kb = filepath.stat().st_size // 1024
            logger.info(f"✅ Downloaded {file_size_kb} KB → {filepath.name}")
            
            return filepath
        
        except Exception as e:
            logger.error(f"❌ Download failed: {e}")
            return None
    
    def extract_fund_manager_info(self, pdf_path: Path) -> List[Dict]:
        """
        Extract fund manager information from SID PDF.
        
        Returns:
            List of fund manager details
        """
        logger.info("👤 Extracting fund manager info...")
        
        try:
            # Extract text
            with pdfplumber.open(str(pdf_path)) as pdf:
                text = ""
                for page in pdf.pages[:20]:  # Usually in first 20 pages
                    text += page.extract_text() + "\n"
        except:
            # Fallback to PyPDF2
            reader = PdfReader(str(pdf_path))
            text = ""
            for page in reader.pages[:20]:
                text += page.extract_text() + "\n"
        
        managers = []
        
        # Search for fund manager section
        fm_section_pattern = r'fund\s+manager[s]?[\s:]+(.{0,500})'
        match = re.search(fm_section_pattern, text.lower())
        
        if match:
            fm_text = match.group(1)
            
            # Extract manager names (typically in format "Name: XXX" or "Mr./Ms. XXX")
            name_patterns = [
                r'(?:mr\.|ms\.|mrs\.|dr\.)\s+([A-Z][a-z]+(?:\s+[A-Z][a-z]+)*)',
                r'name[:\s]+([A-Z][a-z]+(?:\s+[A-Z][a-z]+)*)',
            ]
            
            for pattern in name_patterns:
                names = re.findall(pattern, fm_text)
                for name in names:
                    name = name.strip()
                    if len(name) > 3:  # Filter out initials
                        # Look for qualification and experience near the name
                        name_idx = fm_text.find(name)
                        context = fm_text[max(0, name_idx-100):min(len(fm_text), name_idx+300)]
                        
                        qualification = None
                        experience = None
                        
                        # Extract qualification
                        qual_match = re.search(r'(MBA|CFA|CA|B\.?Com|M\.?Com|B\.?E|M\.?Sc|PhD|PGDM)', context, re.IGNORECASE)
                        if qual_match:
                            qualification = qual_match.group(1)
                        
                        # Extract experience (years)
                        exp_match = re.search(r'(\d+)\s*(?:years?|yrs?).*experience', context, re.IGNORECASE)
                        if exp_match:
                            experience = float(exp_match.group(1))
                        
                        managers.append({
                            'manager_name': name,
                            'qualification': qualification,
                            'experience_years': experience
                        })
        
        # Deduplicate managers
        unique_managers = []
        seen_names = set()
        
        for mgr in managers:
            name_lower = mgr['manager_name'].lower()
            if name_lower not in seen_names:
                seen_names.add(name_lower)
                unique_managers.append(mgr)
        
        logger.info(f"✓ Extracted {len(unique_managers)} fund managers")
        return unique_managers
    
    def extract_key_info_from_sid(self, pdf_path: Path) -> Dict:
        """
        Extract key information from SID:
        - Investment objective
        - Asset allocation
        - Benchmark
        - Exit load
        """
        logger.info("📋 Extracting key information from SID...")
        
        try:
            with pdfplumber.open(str(pdf_path)) as pdf:
                text = ""
                for page in pdf.pages[:30]:  # Key info usually in first 30 pages
                    text += page.extract_text() + "\n"
        except:
            reader = PdfReader(str(pdf_path))
            text = ""
            for page in reader.pages[:30]:
                text += page.extract_text() + "\n"
        
        info = {}
        
        # Investment objective
        obj_pattern = r'investment\s+objective[s]?[:\s]+(.{0,500})'
        obj_match = re.search(obj_pattern, text.lower())
        if obj_match:
            info['investment_objective'] = obj_match.group(1)[:500].strip()
        
        # Benchmark
        bench_pattern = r'benchmark[:\s]+(.{0,200})'
        bench_match = re.search(bench_pattern, text.lower())
        if bench_match:
            info['benchmark'] = bench_match.group(1)[:200].strip()
        
        # Exit load
        exit_pattern = r'exit\s+load[:\s]+(.{0,300})'
        exit_match = re.search(exit_pattern, text.lower())
        if exit_match:
            info['exit_load'] = exit_match.group(1)[:300].strip()
        
        logger.info("✓ Extracted key information")
        return info
    
    def save_sid_to_db(self, scheme_id: Optional[str], amc_id: Optional[str],
                       sid_url: str, local_path: Path, managers: List[Dict],
                       key_info: Dict):
        """
        Save SID document info to database.
        """
        with self.engine.begin() as conn:
            result = conn.execute(
                text("""
                    INSERT INTO sebi_sid_documents (
                        scheme_id, amc_id, document_type, sebi_url, local_file_path,
                        file_size_kb, download_status, parsing_status,
                        investment_objective, benchmark_index, exit_load_structure,
                        fund_manager_info, created_at, updated_at
                    ) VALUES (
                        :scheme_id, :amc_id, 'SID', :sebi_url, :local_path,
                        :file_size_kb, 'DOWNLOADED', 'SUCCESS',
                        :investment_objective, :benchmark, :exit_load,
                        :fund_manager_info, now(), now()
                    )
                    RETURNING id
                """),
                {
                    "scheme_id": scheme_id,
                    "amc_id": amc_id,
                    "sebi_url": sid_url,
                    "local_path": str(local_path),
                    "file_size_kb": local_path.stat().st_size // 1024,
                    "investment_objective": key_info.get('investment_objective'),
                    "benchmark": key_info.get('benchmark'),
                    "exit_load": key_info.get('exit_load'),
                    "fund_manager_info": str(managers)  # Store as JSON in production
                }
            )
            
            sid_id = result.fetchone()[0]
        
        # Save fund managers to fund_manager_master
        for mgr in managers:
            self.save_fund_manager(mgr, amc_id, scheme_id)
    
    def save_fund_manager(self, manager: Dict, amc_id: Optional[str], scheme_id: Optional[str]):
        """
        Save/update fund manager in fund_manager_master table.
        """
        with self.engine.begin() as conn:
            # Check if manager exists
            result = conn.execute(
                text("""
                    SELECT manager_id FROM fund_manager_master
                    WHERE manager_name_normalized = lower(:name)
                """),
                {"name": manager['manager_name']}
            ).fetchone()
            
            if result:
                manager_id = result[0]
                
                # Update existing manager
                conn.execute(
                    text("""
                        UPDATE fund_manager_master
                        SET qualification = COALESCE(:qualification, qualification),
                            total_experience_years = COALESCE(:experience, total_experience_years),
                            updated_at = now()
                        WHERE manager_id = :manager_id
                    """),
                    {
                        "manager_id": manager_id,
                        "qualification": manager.get('qualification'),
                        "experience": manager.get('experience_years')
                    }
                )
            else:
                # Insert new manager
                result = conn.execute(
                    text("""
                        INSERT INTO fund_manager_master (
                            manager_name, manager_name_normalized,
                            qualification, total_experience_years,
                            current_amc_id, created_at, updated_at
                        ) VALUES (
                            :name, lower(:name),
                            :qualification, :experience,
                            :amc_id, now(), now()
                        )
                        RETURNING manager_id
                    """),
                    {
                        "name": manager['manager_name'],
                        "qualification": manager.get('qualification'),
                        "experience": manager.get('experience_years'),
                        "amc_id": amc_id
                    }
                )
                
                manager_id = result.fetchone()[0]
            
            # Link manager to scheme
            if scheme_id:
                conn.execute(
                    text("""
                        INSERT INTO scheme_fund_manager_mapping (
                            scheme_id, manager_id, start_date, is_primary_manager,
                            created_at, updated_at
                        ) VALUES (
                            :scheme_id, :manager_id, CURRENT_DATE, true,
                            now(), now()
                        )
                        ON CONFLICT DO NOTHING
                    """),
                    {
                        "scheme_id": scheme_id,
                        "manager_id": manager_id
                    }
                )
    
    def run(self, limit_amcs: Optional[int] = None):
        """
        Main execution: scrape SEBI SIDs for all AMCs.
        """
        logger.info("🚀 Starting SEBI SID scraper...")
        
        # Get AMC list from SEBI
        amc_list = self.get_amc_list_from_sebi()
        
        if limit_amcs:
            amc_list = amc_list[:limit_amcs]
        
        logger.info(f"📋 Processing {len(amc_list)} AMCs")
        
        for i, amc_info in enumerate(amc_list, 1):
            amc_name = amc_info['amc_name']
            logger.info(f"[{i}/{len(amc_list)}] Processing {amc_name}")
            
            # Match to amc_master
            with self.engine.connect() as conn:
                result = conn.execute(
                    text("""
                        SELECT amc_id FROM amc_master
                        WHERE similarity(lower(amc_full_name), lower(:name)) > 0.6
                        ORDER BY similarity(lower(amc_full_name), lower(:name)) DESC
                        LIMIT 1
                    """),
                    {"name": amc_name}
                ).fetchone()
                
                amc_id = str(result[0]) if result else None
            
            # Get scheme SIDs
            if 'sebi_amc_code' in amc_info:
                schemes = self.get_scheme_sids_for_amc(amc_info['sebi_amc_code'])
            elif 'sid_url' in amc_info:
                schemes = [{'scheme_name': 'General', 'sid_url': amc_info['sid_url']}]
            else:
                continue
            
            # Download and parse SIDs
            for scheme in schemes:
                sid_url = scheme['sid_url']
                scheme_name = scheme['scheme_name']
                
                # Download SID
                local_path = self.download_sid_document(sid_url, amc_name, scheme_name)
                
                if not local_path:
                    continue
                
                # Parse SID
                managers = self.extract_fund_manager_info(local_path)
                key_info = self.extract_key_info_from_sid(local_path)
                
                # Save to DB
                self.save_sid_to_db(None, amc_id, sid_url, local_path, managers, key_info)
        
        logger.info("🎉 SEBI SID scraping complete!")


def main():
    logging.basicConfig(
        level=logging.INFO,
        format="%(asctime)s [%(levelname)s] %(name)s - %(message)s"
    )
    
    from Etl.utils import settings, engine
    
    download_dir = Path(settings['paths']['data_root']) / 'downloads' / 'sebi_sids'
    
    scraper = SebiSidScraper(
        db_url=str(engine.url),
        download_dir=download_dir
    )
    
    # Scrape first 3 AMCs as test
    scraper.run(limit_amcs=3)


if __name__ == "__main__":
    main()
