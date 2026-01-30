"""
amfi_link_scraper.py (FIXED)
-----------------------------
Scrapes AMFI portfolio disclosure page (NEW FORMAT with dropdowns and cards).
Handles dynamic page with Monthly/Half Yearly/Fortnightly selection.
"""

import logging
from pathlib import Path
from datetime import datetime
from typing import Dict, List, Optional
import time

from playwright.sync_api import sync_playwright, TimeoutError as PlaywrightTimeout
from sqlalchemy import create_engine, text

logger = logging.getLogger("mf.etl.amfi_scraper")


class AmfiLinkScraper:
    """
    Scrapes AMFI portfolio disclosure page for AMC website links.
    Updated for new interactive format (Dec 2024).
    """
    
    def __init__(self, db_url: str):
        self.db_url = db_url
        self.engine = create_engine(db_url, pool_pre_ping=True)
        self.amfi_base_url = "https://www.amfiindia.com"
        self.portfolio_url = f"{self.amfi_base_url}/online-center/portfolio-disclosure"
    
    def scrape_amc_links(self) -> List[Dict]:
        """
        Scrapes AMFI portfolio disclosure page for AMC links.
        
        Returns:
            List of dicts with AMC name and URLs
        """
        logger.info("🌐 Scraping AMFI portfolio disclosure page...")
        
        amc_links = []
        
        try:
            with sync_playwright() as p:
                browser = p.chromium.launch(headless=False)  # Set True for production
                context = browser.new_context()
                page = context.new_page()
                
                # Navigate to portfolio disclosure page
                logger.info(f"Loading: {self.portfolio_url}")
                page.goto(self.portfolio_url, wait_until="domcontentloaded", timeout=30000)
                
                # Wait for page to load
                time.sleep(3)
                
                # Look for the dropdown - try multiple selectors
                dropdown_selectors = [
                    'select[name="disclosure_type"]',
                    'select#disclosureType',
                    'select',
                    '.disclosure-select'
                ]
                
                dropdown = None
                for selector in dropdown_selectors:
                    try:
                        dropdown = page.query_selector(selector)
                        if dropdown:
                            logger.info(f"✓ Found dropdown: {selector}")
                            break
                    except:
                        continue
                
                # If no dropdown, try to find AMC cards directly
                if not dropdown:
                    logger.info("No dropdown found, looking for AMC cards directly...")
                    amc_links = self._scrape_amc_cards(page)
                else:
                    # Select each disclosure type and scrape
                    disclosure_types = ['Monthly', 'Half Yearly', 'Fortnightly']
                    
                    for disc_type in disclosure_types:
                        logger.info(f"📋 Processing {disc_type} Portfolio Disclosure")
                        
                        try:
                            # Select disclosure type from dropdown
                            page.select_option(dropdown, label=f"{disc_type} Portfolio Disclosure")
                            time.sleep(2)  # Wait for cards to load
                            
                            # Scrape AMC cards
                            cards = self._scrape_amc_cards(page)
                            
                            # Add disclosure type to each card
                            for card in cards:
                                if disc_type == 'Monthly':
                                    card['portfolio_monthly_url'] = card.get('portfolio_url')
                                elif disc_type == 'Half Yearly':
                                    card['portfolio_halfyearly_url'] = card.get('portfolio_url')
                                elif disc_type == 'Fortnightly':
                                    card['portfolio_fortnightly_url'] = card.get('portfolio_url')
                            
                            # Merge with existing amc_links
                            amc_links = self._merge_amc_links(amc_links, cards)
                            
                        except Exception as e:
                            logger.warning(f"⚠️  Failed to process {disc_type}: {e}")
                
                browser.close()
        
        except Exception as e:
            logger.error(f"❌ AMFI scraping failed: {e}")
        
        logger.info(f"✅ Scraped {len(amc_links)} AMC links")
        return amc_links
    
    def _scrape_amc_cards(self, page) -> List[Dict]:
        """
        Scrapes AMC cards from the current page view.
        """
        amc_data = []
        
        # Look for AMC cards - try multiple selectors
        card_selectors = [
            '.amc-card',
            '[class*="amc"]',
            '.fund-card',
            'div[onclick*="http"]'
        ]
        
        cards = []
        for selector in card_selectors:
            try:
                cards = page.query_selector_all(selector)
                if cards and len(cards) > 5:  # Likely found the right elements
                    logger.info(f"✓ Found {len(cards)} cards using: {selector}")
                    break
            except:
                continue
        
        if not cards:
            logger.warning("⚠️  Could not find AMC cards")
            return amc_data
        
        for card in cards:
            try:
                # Extract AMC name
                amc_name = None
                
                # Try multiple ways to get AMC name
                name_selectors = ['h3', 'h4', '.amc-name', '[class*="title"]', 'strong']
                for ns in name_selectors:
                    try:
                        name_elem = card.query_selector(ns)
                        if name_elem:
                            amc_name = name_elem.inner_text().strip()
                            if amc_name:
                                break
                    except:
                        continue
                
                # If still no name, use text content
                if not amc_name:
                    amc_name = card.inner_text().strip()
                    # Take first line only
                    amc_name = amc_name.split('\n')[0] if '\n' in amc_name else amc_name
                
                if not amc_name or len(amc_name) < 3:
                    continue
                
                # Clean AMC name
                amc_name = amc_name.replace('Mutual Fund', '').strip()
                
                # Extract URL (could be onclick, href, or data attribute)
                portfolio_url = None
                
                # Try href
                link = card.query_selector('a')
                if link:
                    portfolio_url = link.get_attribute('href')
                
                # Try onclick
                if not portfolio_url:
                    onclick = card.get_attribute('onclick')
                    if onclick:
                        # Extract URL from onclick="window.open('url')"
                        import re
                        match = re.search(r"['\"]((https?://|/)[^'\"]+)['\"]", onclick)
                        if match:
                            portfolio_url = match.group(1)
                
                # Make URL absolute
                if portfolio_url and not portfolio_url.startswith('http'):
                    portfolio_url = self.amfi_base_url + portfolio_url
                
                amc_data.append({
                    'amc_name': amc_name,
                    'portfolio_url': portfolio_url,
                    'base_website_url': portfolio_url
                })
                
                logger.debug(f"✓ Found: {amc_name} -> {portfolio_url}")
            
            except Exception as e:
                logger.debug(f"Error parsing card: {e}")
                continue
        
        return amc_data
    
    def _merge_amc_links(self, existing: List[Dict], new: List[Dict]) -> List[Dict]:
        """
        Merge new AMC links with existing, combining different disclosure types.
        """
        # Create dict indexed by AMC name
        merged = {}
        
        for item in existing:
            name = item['amc_name']
            merged[name] = item
        
        for item in new:
            name = item['amc_name']
            if name in merged:
                # Update existing entry
                merged[name].update({k: v for k, v in item.items() if v})
            else:
                merged[name] = item
        
        return list(merged.values())
    
    def match_amc_to_master(self, amc_name: str) -> Optional[str]:
        """
        Match scraped AMC name to amc_master.amc_id using fuzzy matching.
        """
        with self.engine.connect() as conn:
            # Try exact match first
            result = conn.execute(
                text("SELECT amc_id FROM amc_master WHERE lower(amc_full_name) = lower(:name)"),
                {"name": amc_name}
            ).fetchone()
            
            if result:
                return str(result[0])
            
            # Try fuzzy match using pg_trgm
            result = conn.execute(
                text("""
                    SELECT amc_id, amc_full_name, 
                           similarity(lower(amc_full_name), lower(:name)) AS sim
                    FROM amc_master
                    WHERE similarity(lower(amc_full_name), lower(:name)) > 0.5
                    ORDER BY sim DESC
                    LIMIT 1
                """),
                {"name": amc_name}
            ).fetchone()
            
            if result and result[2] > 0.6:  # Similarity > 0.6
                logger.info(f"🔗 Fuzzy matched '{amc_name}' → '{result[1]}' (sim={result[2]:.2f})")
                return str(result[0])
        
        logger.warning(f"⚠️  No match found for AMC: {amc_name}")
        return None
    
    def save_to_db(self, amc_links: List[Dict]):
        """
        Save scraped AMC links to amc_website_links table.
        """
        logger.info("💾 Saving AMC links to database...")
        
        saved_count = 0
        
        with self.engine.begin() as conn:
            for amc_data in amc_links:
                try:
                    amc_id = self.match_amc_to_master(amc_data['amc_name'])
                    
                    conn.execute(
                        text("""
                            INSERT INTO amc_website_links (
                                amc_id, amc_name,
                                portfolio_monthly_url,
                                portfolio_halfyearly_url,
                                portfolio_annual_url,
                                base_website_url,
                                scrape_status,
                                created_at,
                                updated_at
                            ) VALUES (
                                :amc_id, :amc_name,
                                :portfolio_monthly_url,
                                :portfolio_halfyearly_url,
                                :portfolio_annual_url,
                                :base_website_url,
                                'SUCCESS',
                                now(),
                                now()
                            )
                            ON CONFLICT (amc_name)
                            DO UPDATE SET
                                portfolio_monthly_url = COALESCE(EXCLUDED.portfolio_monthly_url, amc_website_links.portfolio_monthly_url),
                                portfolio_halfyearly_url = COALESCE(EXCLUDED.portfolio_halfyearly_url, amc_website_links.portfolio_halfyearly_url),
                                portfolio_annual_url = COALESCE(EXCLUDED.portfolio_annual_url, amc_website_links.portfolio_annual_url),
                                updated_at = now()
                        """),
                        {
                            "amc_id": amc_id,
                            "amc_name": amc_data['amc_name'],
                            "portfolio_monthly_url": amc_data.get('portfolio_monthly_url'),
                            "portfolio_halfyearly_url": amc_data.get('portfolio_halfyearly_url'),
                            "portfolio_annual_url": amc_data.get('portfolio_annual_url'),
                            "base_website_url": amc_data.get('base_website_url')
                        }
                    )
                    saved_count += 1
                except Exception as e:
                    logger.error(f"Failed to save {amc_data['amc_name']}: {e}")
        
        logger.info(f"✅ Saved {saved_count}/{len(amc_links)} AMC website links")
    
    def run(self):
        """
        Main execution: scrape AMFI and save to DB.
        """
        logger.info("🚀 Starting AMFI link scraper...")
        
        amc_links = self.scrape_amc_links()
        
        if amc_links:
            self.save_to_db(amc_links)
            logger.info("🎉 AMFI scraping completed successfully!")
        else:
            logger.error("❌ No AMC links found!")


def main():
    logging.basicConfig(
        level=logging.INFO,
        format="%(asctime)s [%(levelname)s] %(name)s - %(message)s"
    )
    
    # Note: Update db_url with correct credentials
    from Etl.utils import engine
    
    scraper = AmfiLinkScraper(db_url=str(engine.url))
    scraper.run()


if __name__ == "__main__":
    main()