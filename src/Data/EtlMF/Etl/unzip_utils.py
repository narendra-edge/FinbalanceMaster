import logging
from pathlib import Path
import zipfile
import pyzipper
from .utils import ensure_dir

logger = logging.getLogger("mf.etl.unzip")

def extract_zip(zip_path: str, dest_dir: str, password: str | None = None) -> list[str]:
    """
    Extracts ZIP files, supporting both normal and password-protected archives.

    Args:
        zip_path (str): Path to ZIP file
        dest_dir (str): Destination folder for extraction
        password (str | None): Optional password (plain text)

    Returns:
        list[str]: List of extracted file paths
    """
    zip_path = Path(zip_path)
    dest = Path(dest_dir)
    ensure_dir(dest)

    extracted_files: list[str] = []

    # Try standard zipfile first (for non-encrypted ZIPs)
    try:
        with zipfile.ZipFile(zip_path, 'r') as z:
            if z.testzip() is None:
                z.extractall(dest)
                extracted_files = [str(p) for p in dest.iterdir() if p.is_file()]
                logger.info("✅ Extracted %s to %s (no password needed)", zip_path, dest)
                return extracted_files
    except RuntimeError as ex:
        logger.warning("⚠️ Standard extraction failed for %s: %s", zip_path, ex)

    # Try password-protected with pyzipper
    if password:
        try:
            with pyzipper.AESZipFile(zip_path, 'r') as z:
                z.pwd = password.encode('utf-8')
                z.extractall(dest)
                extracted_files = [str(p) for p in dest.iterdir() if p.is_file()]
                logger.info("✅ Extracted %s with password", zip_path)
                return extracted_files
        except RuntimeError as ex:
            logger.error("❌ Invalid password for %s: %s", zip_path, ex)
        except Exception as e:
            logger.exception("❌ Failed to extract %s with pyzipper: %s", zip_path, e)
    else:
        logger.warning("⚠️ ZIP file %s requires a password but none was provided", zip_path)

    raise RuntimeError(f"Failed to extract {zip_path}. Check password or file integrity.")

