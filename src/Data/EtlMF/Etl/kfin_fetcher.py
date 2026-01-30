import logging
from pathlib import Path
from playwright.sync_api import sync_playwright, TimeoutError
from time import sleep
from datetime import datetime

logger = logging.getLogger("mf.etl.kfin")

class KfinFetcher:
    """
    Fetches the KFinTech mutual fund scheme information XLS file
    from the public DSS website (no login required).
    """
    def __init__(self, config: dict, paths: dict):
        self.config = config
        self.paths = paths
        kfin_cfg = config["kfin"]
        
        # Use new public URL by default, fallback to old login URL if specified
        self.base_url = kfin_cfg.get("base_url", "https://dss.kfintech.com/dssweb/")
        self.scheme_filename = kfin_cfg.get("scheme_filename", "SchemeInfoNew.xls")
        self.download_dir = Path(paths["kfin_dir"])
        self.download_dir.mkdir(parents=True, exist_ok=True)

    def fetch_scheme_file(self) -> Path | None:
        """
        Downloads SchemeInfoNew.xls from KFin public DSS website
        via Information Center → Scheme Information (no login required).
        """
        logger.info("🔑 Starting KFin fetch process (public site)...")
        with sync_playwright() as p:
            browser = p.chromium.launch(headless=False)
            context = browser.new_context(accept_downloads=True)
            
            # Set reasonable timeout
            context.set_default_timeout(60000)  # 1 minute
            
            page = context.new_page()
            
            try:
                logger.info("🌐 Navigating to KFin DSS public website...")
                
                # Load the public page
                page.goto(self.base_url, wait_until="domcontentloaded", timeout=60000)
                logger.info("✅ Page loaded successfully")
                
                # Wait for page to be fully interactive
                sleep(3)
                
                # Click "Information Center" dropdown from top navigation
                logger.info("🧭 Clicking 'Information Center' menu...")
                
                info_center_selectors = [
                    "text=Information Center",
                    "//a[contains(text(), 'Information Center')]",
                    "a:has-text('Information Center')",
                ]
                
                clicked_info_center = False
                for selector in info_center_selectors:
                    try:
                        logger.info(f"Trying selector: {selector}")
                        page.click(selector, timeout=10000)
                        sleep(2)  # Wait for dropdown to appear
                        clicked_info_center = True
                        logger.info("✅ Clicked 'Information Center'")
                        break
                    except TimeoutError:
                        logger.warning(f"Selector failed: {selector}")
                        continue
                
                if not clicked_info_center:
                    logger.error("❌ Could not find 'Information Center' menu")
                    screenshot_path = self.download_dir / f"kfin_error_{datetime.now().strftime('%Y%m%d_%H%M%S')}.png"
                    page.screenshot(path=str(screenshot_path))
                    logger.info(f"📸 Screenshot saved: {screenshot_path}")
                    return None
                
                # Wait for dropdown to be visible
                logger.info("⏳ Waiting for dropdown menu to appear...")
                sleep(2)
                
                # Now click on "Scheme Information" - this triggers direct download
                logger.info("⬇️  Clicking 'Scheme Information' to start download...")
                
                # Wait for the dropdown menu to be fully visible and interactive
                try:
                    page.wait_for_selector(".MuiMenu-root", state="visible", timeout=5000)
                    logger.info("✅ Dropdown menu is visible")
                    sleep(1)  # Allow animations to complete
                except TimeoutError:
                    logger.warning("⚠️  Menu visibility timeout - proceeding anyway")
                
                # Click the menu item to trigger download
                scheme_info_selectors = [
                    "text=/^Scheme Information$/",  # Exact text match
                    "a:has-text('Scheme Information')",
                    "//a[normalize-space()='Scheme Information']",
                    "li:has-text('Scheme Information')",  # Added li selector for Material-UI
                    ".MuiMenuItem-root:has-text('Scheme Information')",
                ]
                
                clicked = False
                download = None
                
                # Setup download listener and click in the context
                with page.expect_download(timeout=180000) as download_info:
                    for selector in scheme_info_selectors:
                        try:
                            if page.locator(selector).count() > 0:
                                logger.info(f"✓ Found element with selector: {selector}")
                                
                                # Method 1: Try force click first (bypasses intercept check)
                                try:
                                    page.locator(selector).first.click(timeout=5000, force=True)
                                    clicked = True
                                    logger.info("✅ Clicked 'Scheme Information' (forced)")
                                    break
                                except Exception as e1:
                                    logger.info(f"Force click failed, trying JavaScript click: {e1}")
                                    
                                    # Method 2: JavaScript click (most reliable for Material-UI)
                                    try:
                                        element = page.locator(selector).first
                                        page.evaluate("el => el.click()", element)
                                        clicked = True
                                        logger.info("✅ Clicked 'Scheme Information' (JavaScript)")
                                        break
                                    except Exception as e2:
                                        logger.warning(f"JavaScript click also failed: {e2}")
                                        continue
                                        
                        except Exception as e:
                            logger.warning(f"Failed to process selector {selector}: {e}")
                            continue
                
                if not clicked:
                    logger.error("❌ Could not click 'Scheme Information'")
                    screenshot_path = self.download_dir / f"kfin_click_error_{datetime.now().strftime('%Y%m%d_%H%M%S')}.png"
                    page.screenshot(path=str(screenshot_path))
                    logger.info(f"📸 Screenshot saved: {screenshot_path}")
                    
                    # Additional debug info
                    logger.info("🔍 Dumping available menu items:")
                    menu_items = page.locator(".MuiMenuItem-root").all()
                    for idx, item in enumerate(menu_items):
                        try:
                            text = item.text_content()
                            logger.info(f"  Menu item {idx}: {text}")
                        except:
                            pass
                    
                    return None
                
                # Wait for download to complete
                logger.info("⏳ Waiting for download to start...")
                download = download_info.value
                logger.info("✅ Download started successfully")
                
                # Save with timestamp
                timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
                safe_name = f"{self.scheme_filename.replace('.xls', '')}_{timestamp}.xls"
                downloaded_path = self.download_dir / safe_name
                
                logger.info(f"💾 Saving file as: {safe_name}")
                download.save_as(downloaded_path)
                
                logger.info(f"✅ KFin scheme file saved: {downloaded_path}")
                return downloaded_path
                
            except TimeoutError as e:
                logger.error(f"⏱ Timeout during KFin fetch: {e}")
                logger.info("💡 Tip: Website may be slow. Check debug screenshots if available.")
                return None
            except Exception as e:
                logger.exception(f"💥 Unexpected error in KFin fetcher: {e}")
                return None
            finally:
                browser.close()
                logger.info("🧹 Browser closed — KFin fetch complete.")