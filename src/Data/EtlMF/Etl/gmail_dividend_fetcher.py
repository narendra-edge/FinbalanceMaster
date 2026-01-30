# Etl/gmail_dividend_fetcher.py
"""
Gmail Dividend Fetcher
----------------------
Fetches daily dividend files from Gmail emails:
- CAMS: Subject "WBR25. IDCW/Bonus Declared"
- KFIN: From "distributorcare@kfintech.com"
"""

import logging
import imaplib
import email
from email.header import decode_header
from pathlib import Path
from typing import List, Dict
from datetime import datetime, timedelta
import os

from Etl.utils import ensure_dir, build_retry

logger = logging.getLogger("mf.etl.gmail_dividend_fetcher")


class GmailDividendFetcher:
    """
    Fetches dividend Excel files from Gmail
    """
    
    def __init__(self, config: dict, paths: dict):
        self.config = config
        self.paths = paths
        
        # Gmail configuration
        gmail_cfg = config.get("gmail", {})
        self.host = gmail_cfg.get("host", "imap.gmail.com")
        self.user = gmail_cfg.get("user")
        self.password = gmail_cfg.get("app_password")
        self.port = gmail_cfg.get("port", 993)
        
        # Dividend configuration
        dividend_cfg = config.get("dividend", {})
        
        # CAMS dividend email settings
        self.cams_sender = dividend_cfg.get("cams_sender", "CAMS Mailback Server")
        self.cams_subject = dividend_cfg.get("cams_subject_keyword", "WBR25. IDCW/Bonus Declared")
        self.cams_download_dir = Path(paths.get("cams_dividend_dir", 
                                      "C:/Data_MF/downloads/dividend/cams"))
        
        # KFIN dividend email settings
        self.kfin_sender = dividend_cfg.get("kfin_sender", "distributorcare@kfintech.com")
        self.kfin_subject = dividend_cfg.get("kfin_subject_keyword", "Dividend")
        self.kfin_download_dir = Path(paths.get("kfin_dividend_dir",
                                      "C:/Data_MF/downloads/dividend/kfin"))
        
        # Email search settings
        self.max_age_days = dividend_cfg.get("max_email_age_days", 7)
        
        # Ensure directories exist
        ensure_dir(self.cams_download_dir)
        ensure_dir(self.kfin_download_dir)
        
        logger.info(f"📧 Gmail Dividend Fetcher initialized")
        logger.info(f"   CAMS: '{self.cams_subject}' → {self.cams_download_dir}")
        logger.info(f"   KFIN: from '{self.kfin_sender}' → {self.kfin_download_dir}")
    
    @build_retry(max_attempts=3, delay=5)
    def connect_to_gmail(self) -> imaplib.IMAP4_SSL:
        """Connect to Gmail IMAP server"""
        try:
            logger.info(f"📧 Connecting to {self.host}...")
            mail = imaplib.IMAP4_SSL(self.host, self.port)
            mail.login(self.user, self.password)
            logger.info("✅ Connected to Gmail")
            return mail
        except Exception as e:
            logger.exception(f"❌ Failed to connect to Gmail: {e}")
            raise
    
    def search_emails(self, mail: imaplib.IMAP4_SSL, search_criteria: str) -> List[bytes]:
        """
        Search emails based on criteria
        Returns list of email IDs
        """
        try:
            mail.select("inbox")
            
            # Search with date filter
            since_date = (datetime.now() - timedelta(days=self.max_age_days)).strftime("%d-%b-%Y")
            search_with_date = f'(SINCE {since_date} {search_criteria})'
            
            logger.info(f"🔍 Searching: {search_with_date}")
            status, messages = mail.search(None, search_with_date)
            
            if status != "OK":
                logger.warning(f"⚠️  Search failed: {status}")
                return []
            
            email_ids = messages[0].split()
            logger.info(f"📬 Found {len(email_ids)} emails")
            return email_ids
            
        except Exception as e:
            logger.exception(f"❌ Email search failed: {e}")
            return []
    
    def download_attachments(
        self, 
        mail: imaplib.IMAP4_SSL, 
        email_ids: List[bytes], 
        download_dir: Path,
        file_extensions: List[str] = None
    ) -> List[Path]:
        """
        Download attachments from emails
        
        Args:
            mail: IMAP connection
            email_ids: List of email IDs to process
            download_dir: Directory to save attachments
            file_extensions: List of extensions to download (e.g., ['.xlsx', '.xls'])
        
        Returns:
            List of downloaded file paths
        """
        if file_extensions is None:
            file_extensions = ['.xlsx', '.xls', '.csv']
        
        downloaded_files = []
        
        for email_id in email_ids:
            try:
                # Fetch email
                status, msg_data = mail.fetch(email_id, "(RFC822)")
                
                if status != "OK":
                    logger.warning(f"⚠️  Failed to fetch email {email_id}")
                    continue
                
                # Parse email
                for response_part in msg_data:
                    if isinstance(response_part, tuple):
                        msg = email.message_from_bytes(response_part[1])
                        
                        # Get subject for logging
                        subject = decode_header(msg["Subject"])[0][0]
                        if isinstance(subject, bytes):
                            subject = subject.decode()
                        
                        logger.info(f"📧 Processing: {subject[:50]}...")
                        
                        # Process attachments
                        if msg.is_multipart():
                            for part in msg.walk():
                                if part.get_content_disposition() == "attachment":
                                    filename = part.get_filename()
                                    
                                    if not filename:
                                        continue
                                    
                                    # Check file extension
                                    file_ext = Path(filename).suffix.lower()
                                    if file_ext not in file_extensions:
                                        logger.debug(f"⏭️  Skipping {filename} (not in {file_extensions})")
                                        continue
                                    
                                    # Decode filename
                                    if isinstance(filename, bytes):
                                        filename = filename.decode()
                                    
                                    # Create unique filename with timestamp
                                    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
                                    safe_filename = f"{timestamp}_{filename}"
                                    filepath = download_dir / safe_filename
                                    
                                    # Save attachment
                                    with open(filepath, "wb") as f:
                                        f.write(part.get_payload(decode=True))
                                    
                                    logger.info(f"💾 Downloaded: {safe_filename}")
                                    downloaded_files.append(filepath)
                        else:
                            # Non-multipart email - check if it has attachment
                            filename = msg.get_filename()
                            if filename:
                                file_ext = Path(filename).suffix.lower()
                                if file_ext in file_extensions:
                                    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
                                    safe_filename = f"{timestamp}_{filename}"
                                    filepath = download_dir / safe_filename
                                    
                                    with open(filepath, "wb") as f:
                                        f.write(msg.get_payload(decode=True))
                                    
                                    logger.info(f"💾 Downloaded: {safe_filename}")
                                    downloaded_files.append(filepath)
                
            except Exception as e:
                logger.exception(f"❌ Failed to process email {email_id}: {e}")
                continue
        
        return downloaded_files
    
    def fetch_cams_dividends(self) -> List[Path]:
        """
        Fetch CAMS dividend files from Gmail
        Search: Subject contains "WBR25. IDCW/Bonus Declared"
        """
        logger.info("\n📧 Fetching CAMS dividend files from Gmail...")
        
        try:
            mail = self.connect_to_gmail()
            
            # Search for CAMS dividend emails
            search_criteria = f'SUBJECT "{self.cams_subject}"'
            email_ids = self.search_emails(mail, search_criteria)
            
            if not email_ids:
                logger.warning("⚠️  No CAMS dividend emails found")
                mail.logout()
                return []
            
            # Download attachments
            downloaded = self.download_attachments(
                mail, 
                email_ids, 
                self.cams_download_dir,
                file_extensions=['.xlsx', '.xls']
            )
            
            mail.logout()
            
            logger.info(f"✅ CAMS Dividends: Downloaded {len(downloaded)} files")
            return downloaded
            
        except Exception as e:
            logger.exception(f"❌ Failed to fetch CAMS dividend files: {e}")
            return []
    
    def fetch_kfin_dividends(self) -> List[Path]:
        """
        Fetch KFIN dividend files from Gmail
        Search: From "distributorcare@kfintech.com" AND Subject contains "Dividend"
        """
        logger.info("\n📧 Fetching KFIN dividend files from Gmail...")
        
        try:
            mail = self.connect_to_gmail()
            
            # Search for KFIN dividend emails
            search_criteria = f'FROM "{self.kfin_sender}" SUBJECT "{self.kfin_subject}"'
            email_ids = self.search_emails(mail, search_criteria)
            
            if not email_ids:
                logger.warning("⚠️  No KFIN dividend emails found")
                mail.logout()
                return []
            
            # Download attachments
            downloaded = self.download_attachments(
                mail,
                email_ids,
                self.kfin_download_dir,
                file_extensions=['.xlsx', '.xls']
            )
            
            mail.logout()
            
            logger.info(f"✅ KFIN Dividends: Downloaded {len(downloaded)} files")
            return downloaded
            
        except Exception as e:
            logger.exception(f"❌ Failed to fetch KFIN dividend files: {e}")
            return []
    
    def fetch_all_dividend_files(self) -> Dict[str, List[Path]]:
        """
        Fetch all dividend files from Gmail (CAMS + KFIN)
        Returns dict with file lists
        """
        logger.info("🚀 Starting Gmail Dividend File Fetch...")
        logger.info("=" * 60)
        
        results = {
            'cams_dividend': [],
            'kfin_dividend': [],
        }
        
        # Fetch CAMS dividends
        try:
            results['cams_dividend'] = self.fetch_cams_dividends()
        except Exception as e:
            logger.exception(f"❌ CAMS dividend fetch failed: {e}")
        
        # Fetch KFIN dividends
        try:
            results['kfin_dividend'] = self.fetch_kfin_dividends()
        except Exception as e:
            logger.exception(f"❌ KFIN dividend fetch failed: {e}")
        
        # Summary
        total_files = len(results['cams_dividend']) + len(results['kfin_dividend'])
        
        logger.info("\n" + "=" * 60)
        logger.info("📊 Gmail Dividend Fetch Summary:")
        logger.info(f"   CAMS: {len(results['cams_dividend'])} files")
        logger.info(f"   KFIN: {len(results['kfin_dividend'])} files")
        logger.info(f"   TOTAL: {total_files} files")
        logger.info("=" * 60)
        
        return results
