import imaplib
import email
import re
import logging
from email.header import decode_header
from pathlib import Path
from bs4 import BeautifulSoup
from typing import Optional
from .utils import build_retry

logger = logging.getLogger("mf.etl.gmail_fetcher")


def _decode_header_safe(hdr: Optional[str]) -> str:
    if not hdr:
        return ""
    parts = decode_header(hdr)
    return "".join(
        b.decode(enc or "utf-8", errors="ignore") if isinstance(b, bytes) else str(b)
        for b, enc in parts
    )


class GmailFetcher:
    def __init__(self, settings: dict, paths: dict):
        g = settings.get("gmail", {})
        self.host = g.get("host", "imap.gmail.com")
        self.port = int(g.get("port", 993))
        self.user = g.get("user")
        self.app_password = g.get("app_password")
        self.cams_sender = g.get("cams_sender", "")
        self.cams_subj_key = g.get("cams_subject_keyword", "")
        self.kfin_otp_sender_contains = g.get("kfin_otp_sender_contains", "")
        self.paths = paths

    def _connect(self) -> imaplib.IMAP4_SSL:
        m = imaplib.IMAP4_SSL(self.host, self.port)
        m.login(self.user, self.app_password)
        return m

    @build_retry(max_attempts=5)
    def fetch_latest_cams_downloadurl(self) -> Optional[str]:
        """Find latest CAMS mailback link (DownloadURL or .zip)"""
        m = self._connect()
        try:
            m.select("INBOX")
            typ, data = m.search(None, "ALL")
            if typ != "OK" or not data or not data[0]:
                return None
            ids = data[0].split()
            for raw_id in reversed(ids[-200:]):
                uid = raw_id.decode() if isinstance(raw_id, bytes) else raw_id
                typ, msg_data = m.fetch(uid, "(RFC822)")
                if typ != "OK" or not msg_data or not msg_data[0]:
                    continue
                raw = msg_data[0][1]
                msg = email.message_from_bytes(raw)
                frm = _decode_header_safe(msg.get("From", ""))
                subj = _decode_header_safe(msg.get("Subject", ""))
                if (self.cams_sender and self.cams_sender.lower() not in frm.lower()) and (
                    self.cams_subj_key and self.cams_subj_key.lower() not in subj.lower()
                ):
                    continue

                html = None
                if msg.is_multipart():
                    for part in msg.walk():
                        if part.get_content_type() == "text/html":
                            html = part.get_payload(decode=True).decode(
                                part.get_content_charset() or "utf-8", errors="ignore"
                            )
                            break
                else:
                    if msg.get_content_type() == "text/html":
                        html = msg.get_payload(decode=True).decode(
                            msg.get_content_charset() or "utf-8", errors="ignore"
                        )

                if not html:
                    continue
                soup = BeautifulSoup(html, "lxml")
                for a in soup.find_all("a", href=True):
                    href = a["href"]
                    if "DownloadURL" in href or href.lower().endswith(".zip"):
                        logger.info("✅ Found CAMS Download link in email.")
                        return href

                # fallback plain text URL search
                urls = re.findall(r"https?://\S+", html)
                for u in urls:
                    if "DownloadURL" in u or u.lower().endswith(".zip"):
                        logger.info("✅ Found CAMS DownloadURL in plain text fallback.")
                        return u
            logger.warning("⚠️ No CAMS download link found.")
            return None
        finally:
            try:
                m.close()
            except Exception:
                pass
            try:
                m.logout()
            except Exception:
                pass

    @build_retry(max_attempts=5)
    def fetch_kfin_otp_from_mail(self) -> Optional[str]:
        """Extract latest KFin OTP from mail."""
        m = self._connect()
        try:
            m.select("INBOX")
            typ, data = m.search(None, "ALL")
            if typ != "OK" or not data or not data[0]:
                return None
            ids = data[0].split()
            for raw_id in reversed(ids[-200:]):
                uid = raw_id.decode() if isinstance(raw_id, bytes) else raw_id
                typ, msg_data = m.fetch(uid, "(RFC822)")
                if typ != "OK" or not msg_data or not msg_data[0]:
                    continue
                raw = msg_data[0][1]
                msg = email.message_from_bytes(raw)
                frm = _decode_header_safe(msg.get("From", ""))
                subj = _decode_header_safe(msg.get("Subject", ""))
                if self.kfin_otp_sender_contains.lower() not in frm.lower():
                    continue
                text = None
                if msg.is_multipart():
                    for part in msg.walk():
                        if part.get_content_type() == "text/plain":
                            text = part.get_payload(decode=True).decode(
                                part.get_content_charset() or "utf-8", errors="ignore"
                            )
                            break
                if not text:
                    continue
                mcode = re.search(r"\b(\d{4,8})\b", text)
                if mcode:
                    logger.info("✅ Found OTP in KFin email.")
                    return mcode.group(1)
            logger.warning("⚠️ No KFin OTP found in recent emails.")
            return None
        finally:
            try:
                m.close()
            except Exception:
                pass
            try:
                m.logout()
            except Exception:
                pass
