"""
universal_amc_crawler.py
------------------------
Universal step-based crawler for AMC websites.
Reads manual links from CSV + step definitions from YAML.
Executes complex multi-step navigation like kfin_fetcher.py

Architecture:
1. Load AMC links from Structured_portfolio.csv
2. Load step definitions from amc_crawl_steps.yaml
3. Execute steps using Playwright
4. Smart file organization
5. Track downloads in database
"""

import logging
import yaml
import pandas as pd
from pathlib import Path
from datetime import datetime, timedelta
from typing import Dict, List, Optional, Any
from calendar import monthrange
import re
import time

from playwright.sync_api import sync_playwright, TimeoutError as PlaywrightTimeout, Page
from sqlalchemy import create_engine, text

logger = logging.getLogger("mf.etl.universal_crawler")


class StepExecutor:
    """Executes individual crawl steps on webpage."""
    
    def __init__(self, page: Page):
        self.page = page
        self.downloaded_files = []
    
    def execute_step(self, step: Dict, context: Dict) -> Any:
        """
        Execute single step based on action type.
        
        Args:
            step: Step definition from YAML
            context: Runtime context (variables, state)
        """
        action = step.get('action')
        
        # Replace variables in step (e.g., {scheme_type} -> 'open_ended')
        step = self._substitute_variables(step, context)
        
        logger.debug(f"Executing: {action} - {step.get('description', '')}")
        
        try:
            if action == "navigate":
                return self._navigate(step)
            
            elif action == "click":
                return self._click(step)
            
            elif action == "click_expand":
                return self._click_expand(step)
            
            elif action == "hover_menu":
                return self._hover_menu(step)
            
            elif action == "select_dropdown":
                return self._select_dropdown(step, context)
            
            elif action == "set_date":
                return self._set_date(step, context)
            
            elif action == "click_download":
                return self._click_download(step)
            
            elif action == "get_scheme_links":
                return self._get_scheme_links(step)
            
            elif action == "get_scheme_list":
                return self._get_scheme_list(step)
            
            elif action == "open_new_tab":
                return self._open_new_tab(step)
            
            elif action == "close_tab":
                return self._close_tab(step)
            
            elif action == "wait_for_element":
                return self._wait_for_element(step)
            
            elif action == "for_each_scheme":
                return self._for_each_scheme(step, context)
            
            elif action == "for_each_type":
                return self._for_each_type(step, context)
            
            elif action == "for_each_link":
                return self._for_each_link(step, context)
            
            else:
                logger.warning(f"Unknown action: {action}")
                return None
                
        except Exception as e:
            logger.error(f"Step execution failed ({action}): {e}")
            raise
    
    def _substitute_variables(self, step: Dict, context: Dict) -> Dict:
        """Replace {variable} placeholders with context values."""
        step_str = str(step)
        
        for key, value in context.items():
            pattern = f"{{{key}}}"
            step_str = step_str.replace(pattern, str(value))
        
        return eval(step_str)  # Reconstruct dict
    
    def _navigate(self, step: Dict) -> None:
        """Navigate to URL."""
        url = step.get('url')
        if not url:
            return
        
        logger.info(f"🌐 Navigating to: {url}")
        self.page.goto(url, wait_until="domcontentloaded", timeout=60000)
        time.sleep(step.get('wait_after', 2000) / 1000)
    
    def _click(self, step: Dict) -> None:
        """Click element using multiple selector strategies."""
        selectors = step.get('selectors', [])
        if isinstance(selectors, str):
            selectors = [selectors]
        
        for selector in selectors:
            try:
                logger.debug(f"Trying selector: {selector}")
                
                if self.page.locator(selector).count() > 0:
                    self.page.locator(selector).first.click(timeout=10000)
                    logger.info(f"✅ Clicked: {selector}")
                    time.sleep(step.get('wait_after', 1000) / 1000)
                    return
            except Exception as e:
                logger.debug(f"Selector failed: {e}")
                continue
        
        raise Exception(f"Could not click any selector: {selectors}")
    
    def _click_expand(self, step: Dict) -> None:
        """Click to expand collapsible section."""
        self._click(step)
    
    def _hover_menu(self, step: Dict) -> None:
        """Hover over menu to reveal dropdown."""
        selectors = step.get('selectors', [])
        
        for selector in selectors:
            try:
                element = self.page.locator(selector).first
                element.hover(timeout=5000)
                logger.info(f"✅ Hovered: {selector}")
                time.sleep(step.get('wait_after', 1000) / 1000)
                return
            except:
                continue
        
        logger.warning(f"Could not hover: {selectors}")
    
    def _select_dropdown(self, step: Dict, context: Dict) -> None:
        """Select dropdown value with smart value detection."""
        field = step.get('field')
        value = step.get('value')
        selectors = step.get('selectors', [])
        
        # Calculate dynamic values
        if value == "latest":
            value = str(datetime.now().year)
        elif value == "previous_month":
            prev_month = datetime.now().replace(day=1) - timedelta(days=1)
            value = prev_month.strftime("%B")  # "November"
        elif value == "last_day_previous_month":
            prev_month = datetime.now().replace(day=1) - timedelta(days=1)
            value = prev_month.strftime("%d-%m-%Y")
        
        # Try value patterns if specified
        value_patterns = step.get('value_patterns', [value])
        
        for selector in selectors:
            try:
                dropdown = self.page.locator(selector).first
                
                # Try each value pattern
                for pattern in value_patterns:
                    try:
                        dropdown.select_option(label=pattern, timeout=5000)
                        logger.info(f"✅ Selected: {field} = {pattern}")
                        return
                    except:
                        try:
                            dropdown.select_option(value=pattern, timeout=5000)
                            logger.info(f"✅ Selected: {field} = {pattern}")
                            return
                        except:
                            continue
            except:
                continue
        
        logger.warning(f"Could not select: {field} = {value}")
    
    def _set_date(self, step: Dict, context: Dict) -> None:
        """Set date field."""
        field = step.get('field')
        value = step.get('value')
        format_str = step.get('format', '%d-%m-%Y')
        
        # Calculate date
        if value == "last_day_previous_month":
            prev_month = datetime.now().replace(day=1) - timedelta(days=1)
            date_value = prev_month.strftime(format_str)
        else:
            date_value = value
        
        # Try to fill date field
        selectors = step.get('selectors', [f"input[name='{field}']", f"#{field}"])
        
        for selector in selectors:
            try:
                self.page.locator(selector).first.fill(date_value, timeout=5000)
                logger.info(f"✅ Set date: {field} = {date_value}")
                return
            except:
                continue
        
        logger.warning(f"Could not set date: {field}")
    
    def _click_download(self, step: Dict) -> Optional[Path]:
        """Click download button and capture file."""
        selectors = step.get('selectors', [])
        timeout_ms = step.get('timeout_ms', 180000)
        wait_for_download = step.get('wait_for_download', True)
        
        # Exclude patterns (e.g., skip "Passive" factsheets)
        exclude_text = step.get('exclude_text', [])
        
        downloaded_file = None
        
        if wait_for_download:
            # Setup download listener
            with self.page.expect_download(timeout=timeout_ms) as download_info:
                for selector in selectors:
                    try:
                        elements = self.page.locator(selector).all()
                        
                        for elem in elements:
                            # Check exclusion patterns
                            text = elem.inner_text().lower()
                            if any(excl in text for excl in exclude_text):
                                logger.debug(f"Skipping excluded: {text}")
                                continue
                            
                            elem.click(timeout=10000)
                            logger.info(f"✅ Clicked download: {selector}")
                            break
                        break
                    except:
                        continue
                
                download = download_info.value
                downloaded_file = Path(download.path())
                logger.info(f"📥 Download complete: {downloaded_file.name}")
        
        else:
            # Just click without waiting
            self._click(step)
        
        self.downloaded_files.append(downloaded_file)
        return downloaded_file
    
    def _get_scheme_links(self, step: Dict) -> List[str]:
        """Extract all scheme detail page links."""
        container = step.get('container', 'body')
        link_selector = step.get('link_selector', 'a')
        
        links = []
        
        container_elem = self.page.locator(container).first
        link_elements = container_elem.locator(link_selector).all()
        
        for elem in link_elements:
            try:
                href = elem.get_attribute('href')
                if href:
                    links.append(href)
            except:
                continue
        
        logger.info(f"📋 Found {len(links)} scheme links")
        return links
    
    def _get_scheme_list(self, step: Dict) -> List[str]:
        """Get list of schemes from dropdown options."""
        selectors = step.get('selectors', [])
        exclude_patterns = step.get('exclude_patterns', [])
        
        schemes = []
        
        for selector in selectors:
            try:
                options = self.page.locator(selector).all()
                
                for option in options:
                    text = option.inner_text().strip()
                    value = option.get_attribute('value')
                    
                    # Skip excluded patterns
                    if any(pat.lower() in text.lower() for pat in exclude_patterns):
                        continue
                    
                    schemes.append({'text': text, 'value': value})
                
                break
            except:
                continue
        
        logger.info(f"📋 Found {len(schemes)} schemes")
        return schemes
    
    def _open_new_tab(self, step: Dict) -> None:
        """Open link in new tab."""
        # Playwright handles new tab automatically
        pass
    
    def _close_tab(self, step: Dict) -> None:
        """Close current tab."""
        self.page.close()
        time.sleep(step.get('wait_after', 1000) / 1000)
    
    def _wait_for_element(self, step: Dict) -> None:
        """Wait for element to appear."""
        selectors = step.get('selectors', [])
        timeout_ms = step.get('timeout_ms', 10000)
        
        for selector in selectors:
            try:
                self.page.wait_for_selector(selector, timeout=timeout_ms)
                logger.info(f"✅ Element appeared: {selector}")
                return
            except:
                continue
        
        logger.warning(f"Element did not appear: {selectors}")
    
    def _for_each_scheme(self, step: Dict, context: Dict) -> None:
        """Loop through schemes."""
        schemes = context.get('schemes', [])
        sub_steps = step.get('steps', [])
        
        for scheme in schemes:
            logger.info(f"📊 Processing scheme: {scheme.get('text', '')}")
            
            scheme_context = context.copy()
            scheme_context['current_scheme'] = scheme
            
            for sub_step in sub_steps:
                self.execute_step(sub_step, scheme_context)
    
    def _for_each_type(self, step: Dict, context: Dict) -> None:
        """Loop through types (e.g., scheme types for TER)."""
        variable = step.get('variable')
        types = context.get('loop_values', [])
        sub_steps = step.get('steps', [])
        
        for type_value in types:
            logger.info(f"🔄 Processing {variable}: {type_value}")
            
            type_context = context.copy()
            type_context[variable] = type_value
            
            for sub_step in sub_steps:
                self.execute_step(sub_step, type_context)
    
    def _for_each_link(self, step: Dict, context: Dict) -> None:
        """Loop through links."""
        links = context.get('scheme_links', [])
        sub_steps = step.get('steps', [])
        
        for link in links:
            logger.info(f"🔗 Processing link: {link}")
            
            link_context = context.copy()
            link_context['current_link'] = link
            
            # Open link in new tab
            new_page = self.page.context.new_page()
            new_page.goto(link, timeout=60000)
            
            # Execute sub-steps on new page
            executor = StepExecutor(new_page)
            for sub_step in sub_steps:
                executor.execute_step(sub_step, link_context)
            
            new_page.close()


class UniversalAmcCrawler:
    """
    Universal crawler that reads CSV links + YAML steps.
    """
    
    def __init__(self, db_url: str, settings: Dict):
        self.db_url = db_url
        self.engine = create_engine(db_url, pool_pre_ping=True)
        self.settings = settings
        
        # Paths
        self.data_root = Path(settings['paths']['data_root'])
        self.downloads_dir = Path(settings['paths']['downloads_dir'])
        
        # Load configurations
        self.amc_links_df = self._load_amc_links()
        self.step_definitions = self._load_step_definitions()
    
    def _load_amc_links(self) -> pd.DataFrame:
        """Load manual AMC links from CSV."""
        csv_path = Path("Structured_portfolio.csv")
        
        if not csv_path.exists():
            raise FileNotFoundError(f"CSV not found: {csv_path}")
        
        df = pd.read_csv(csv_path)
        logger.info(f"✅ Loaded {len(df)} AMC links from CSV")
        return df
    
    def _load_step_definitions(self) -> Dict:
        """Load YAML step definitions."""
        yaml_path = Path("amc_crawl_steps.yaml")
        
        if not yaml_path.exists():
            logger.warning(f"⚠️  YAML config not found: {yaml_path}")
            return {}
        
        with open(yaml_path, 'r') as f:
            config = yaml.safe_load(f)
        
        logger.info(f"✅ Loaded step definitions for {len(config)} AMCs")
        return config
    
    def _get_amc_config(self, amc_name: str) -> Optional[Dict]:
        """Get step configuration for AMC."""
        # Normalize AMC name for matching
        amc_key = amc_name.lower().replace(' ', '_').replace('mutual_fund', '').strip('_')
        
        # Try exact match
        if amc_key in self.step_definitions:
            return self.step_definitions[amc_key]
        
        # Try partial match
        for key, config in self.step_definitions.items():
            if key in amc_key or amc_key in key:
                return config
        
        # Fallback to generic
        logger.warning(f"⚠️  No config found for {amc_name}, using generic")
        return self.step_definitions.get('generic', {})
    
    def _organize_downloaded_file(self, file_path: Path, amc_name: str, 
                                   doc_type: str) -> Path:
        """
        Organize downloaded file into proper directory structure.
        
        Structure:
        downloads/
          ├── portfolio/
          │   ├── axis_mf/
          │   │   ├── 2024_11_consolidated.xlsx
          │   │   └── 2024_11_scheme_wise.xlsx
          │   └── hdfc_mf/
          ├── aaum/
          ├── factsheet/
          ├── ter/
          └── sid/
        """
        # Create AMC-specific folder
        amc_folder_name = amc_name.lower().replace(' ', '_')
        dest_dir = self.downloads_dir / doc_type.lower() / amc_folder_name
        dest_dir.mkdir(parents=True, exist_ok=True)
        
        # Generate meaningful filename
        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        ext = file_path.suffix
        new_name = f"{doc_type.lower()}_{timestamp}{ext}"
        
        dest_path = dest_dir / new_name
        
        # Move file
        import shutil
        shutil.move(str(file_path), str(dest_path))
        
        logger.info(f"📁 Organized: {dest_path.relative_to(self.downloads_dir)}")
        return dest_path
    
    def _track_download_in_db(self, amc_id: str, amc_name: str, doc_type: str,
                             url: str, local_path: Path, status: str):
        """Track download in database."""
        with self.engine.begin() as conn:
            conn.execute(
                text("""
                    INSERT INTO statutory_document_downloads (
                        amc_id, amc_name, document_type, frequency,
                        download_url, local_file_path, file_format,
                        file_size_kb, download_status, download_attempts,
                        last_download_attempt, created_at, updated_at
                    ) VALUES (
                        :amc_id, :amc_name, :doc_type, 'MONTHLY',
                        :url, :local_path, :file_format,
                        :file_size_kb, :status, 1,
                        now(), now(), now()
                    )
                    ON CONFLICT (amc_id, document_type, frequency, document_date)
                    DO UPDATE SET
                        local_file_path = EXCLUDED.local_file_path,
                        download_status = EXCLUDED.download_status,
                        download_attempts = statutory_document_downloads.download_attempts + 1,
                        last_download_attempt = now(),
                        updated_at = now()
                """),
                {
                    "amc_id": amc_id,
                    "amc_name": amc_name,
                    "doc_type": doc_type,
                    "url": url,
                    "local_path": str(local_path) if local_path else None,
                    "file_format": local_path.suffix[1:].upper() if local_path else None,
                    "file_size_kb": (local_path.stat().st_size // 1024) if local_path and local_path.exists() else None,
                    "status": status
                }
            )
    
    def crawl_amc_document(self, amc_row: pd.Series, doc_type: str) -> bool:
        """
        Crawl single document type for one AMC.
        CSV contains DIRECT links - no redirection handling needed.
        
        For PORTFOLIO: Downloads ALL available frequencies (monthly + fortnightly + halfyearly)
        For others: Downloads latest available data
        
        Args:
            amc_row: Row from Structured_portfolio.csv
            doc_type: 'portfolio', 'aaum', 'factsheet', 'ter', or 'sid'
        
        Returns:
            Success boolean
        """
        amc_name = amc_row['AMC']
        url_column = f"{doc_type.capitalize()} Link" if doc_type != 'aaum' else "Aaum link"
        base_url = amc_row.get(url_column)
        
        if pd.isna(base_url) or not base_url:
            logger.warning(f"⚠️  No {doc_type} link for {amc_name}")
            return False
        
        logger.info(f"🏢 Crawling {doc_type} for {amc_name}")
        logger.info(f"🔗 URL: {base_url}")
        
        # Get step configuration
        amc_config = self._get_amc_config(amc_name)
        doc_config = amc_config.get(doc_type, {})
        
        if not doc_config:
            logger.warning(f"⚠️  No config found for {amc_name} {doc_type}")
            return False
        
        try:
            with sync_playwright() as p:
                browser = p.chromium.launch(headless=False)  # Set True for production
                context = browser.new_context(accept_downloads=True)
                page = context.new_page()
                
                # Navigate to DIRECT URL from CSV
                page.goto(base_url, wait_until="domcontentloaded", timeout=60000)
                time.sleep(3)
                
                logger.info(f"✅ Navigated to: {page.url}")
                
                # Execute steps
                executor = StepExecutor(page)
                
                # Build execution context
                exec_context = {
                    'amc_name': amc_name,
                    'doc_type': doc_type,
                    'base_url': base_url
                }
                
                # Handle looping scenarios (for TER)
                if doc_type == 'ter' and doc_config.get('loop_scheme_types'):
                    exec_context['loop_values'] = doc_config['loop_scheme_types']
                
                # SPECIAL HANDLING FOR PORTFOLIO: Download ALL available frequencies
                if doc_type == 'portfolio' and doc_config.get('frequency_preference'):
                    frequencies = doc_config['frequency_preference']
                    
                    logger.info(f"📋 Will attempt to download ALL available frequencies: {frequencies}")
                    
                    # Try EACH frequency (don't stop on first success)
                    for freq in frequencies:
                        freq_config = doc_config.get(freq)
                        if not freq_config or not freq_config.get('steps'):
                            logger.debug(f"No steps defined for {freq}, skipping")
                            continue
                        
                        logger.info(f"🔄 Attempting to download {freq} portfolio...")
                        
                        try:
                            # Reset page state for each frequency attempt
                            # Go back to top of page
                            page.evaluate("window.scrollTo(0, 0)")
                            time.sleep(1)
                            
                            # Track files before this frequency
                            files_before = len(executor.downloaded_files)
                            
                            # Execute steps for this frequency
                            for step in freq_config.get('steps', []):
                                result = executor.execute_step(step, exec_context)
                                
                                if result:
                                    if step.get('action') == 'get_scheme_links':
                                        exec_context['scheme_links'] = result
                                    elif step.get('action') == 'get_scheme_list':
                                        exec_context['schemes'] = result
                            
                            # Check if we got a new file
                            files_after = len(executor.downloaded_files)
                            if files_after > files_before:
                                logger.info(f"✅ Successfully downloaded {freq} portfolio")
                            else:
                                logger.info(f"ℹ️  No {freq} portfolio available (not an error)")
                        
                        except Exception as e:
                            logger.info(f"ℹ️  {freq} not available or failed: {e}")
                            # Continue to next frequency - this is NOT a failure
                            continue
                    
                    # After trying all frequencies
                    if executor.downloaded_files:
                        logger.info(f"✅ Downloaded {len(executor.downloaded_files)} portfolio file(s)")
                    else:
                        logger.warning(f"⚠️  No portfolio files downloaded for {amc_name}")
                
                else:
                    # Execute steps normally (for AAUM, factsheet, etc.)
                    steps = doc_config.get('steps', [])
                    
                    if not steps:
                        logger.warning(f"⚠️  No steps defined for {amc_name} {doc_type}")
                        return False
                    
                    for step in steps:
                        try:
                            result = executor.execute_step(step, exec_context)
                            
                            # Store results in context for next steps
                            if result:
                                if step.get('action') == 'get_scheme_links':
                                    exec_context['scheme_links'] = result
                                elif step.get('action') == 'get_scheme_list':
                                    exec_context['schemes'] = result
                        
                        except Exception as e:
                            logger.error(f"❌ Step failed: {step.get('action')} - {e}")
                            # Continue with next step
                
                # Organize downloaded files
                for downloaded_file in executor.downloaded_files:
                    if downloaded_file and downloaded_file.exists():
                        organized_path = self._organize_downloaded_file(
                            downloaded_file, amc_name, doc_type
                        )
                        
                        # Track in database (skip if DB error)
                        try:
                            self._track_download_in_db(
                                amc_id=None,  # Match in post-processing
                                amc_name=amc_name,
                                doc_type=doc_type.upper(),
                                url=base_url,
                                local_path=organized_path,
                                status='DOWNLOADED'
                            )
                        except Exception as db_err:
                            logger.warning(f"⚠️  Could not track in DB (non-critical): {db_err}")
                
                browser.close()
                
                success = len(executor.downloaded_files) > 0
                if success:
                    logger.info(f"✅ Completed {doc_type} for {amc_name} - Downloaded {len(executor.downloaded_files)} file(s)")
                else:
                    logger.warning(f"⚠️  Completed {doc_type} for {amc_name} but no files downloaded")
                
                return success
        
        except Exception as e:
            logger.exception(f"❌ Crawl failed for {amc_name} {doc_type}: {e}")
            return False
    
    def run(self, doc_types: List[str] = None, limit_amcs: int = None):
        """
        Run crawler for all AMCs.
        
        Args:
            doc_types: List of doc types to crawl (default: all)
            limit_amcs: Limit number of AMCs (for testing)
        """
        if doc_types is None:
            doc_types = ['portfolio', 'aaum', 'factsheet', 'ter', 'sid']
        
        logger.info("🚀 Starting Universal AMC Crawler")
        logger.info(f"📋 Document types: {doc_types}")
        logger.info(f"🏢 Total AMCs: {len(self.amc_links_df)}")
        
        if limit_amcs:
            amc_df = self.amc_links_df.head(limit_amcs)
            logger.info(f"⚠️  Limited to {limit_amcs} AMCs for testing")
        else:
            amc_df = self.amc_links_df
        
        # Statistics
        stats = {doc_type: {'success': 0, 'failed': 0} for doc_type in doc_types}
        
        for idx, amc_row in amc_df.iterrows():
            amc_name = amc_row['AMC']
            logger.info(f"\n{'='*70}")
            logger.info(f"[{idx+1}/{len(amc_df)}] Processing: {amc_name}")
            logger.info(f"{'='*70}\n")
            
            for doc_type in doc_types:
                success = self.crawl_amc_document(amc_row, doc_type)
                
                if success:
                    stats[doc_type]['success'] += 1
                else:
                    stats[doc_type]['failed'] += 1
                
                # Small delay between documents
                time.sleep(2)
            
            # Delay between AMCs
            time.sleep(5)
        
        # Print summary
        logger.info("\n" + "="*70)
        logger.info("📊 CRAWL SUMMARY")
        logger.info("="*70)
        
        for doc_type, counts in stats.items():
            total = counts['success'] + counts['failed']
            pct = (counts['success'] / total * 100) if total > 0 else 0
            logger.info(f"{doc_type.upper():15} → Success: {counts['success']:3}/{total:3} ({pct:.1f}%)")
        
        logger.info("="*70)
        logger.info("🎉 Universal AMC Crawler Complete!")


def main():
    import logging
    logging.basicConfig(
        level=logging.INFO,
        format="%(asctime)s [%(levelname)s] %(name)s - %(message)s"
    )
    
    from Etl.utils import settings, engine
    
    crawler = UniversalAmcCrawler(
        db_url=str(engine.url),
        settings=settings
    )
    
    # Test with 2 AMCs, only portfolio
    crawler.run(doc_types=['portfolio'], limit_amcs=2)


if __name__ == "__main__":
    main()