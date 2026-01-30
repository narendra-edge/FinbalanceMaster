"""
run_etl.py
-----------
Top-level orchestration for MF ETL:

1. Read settings.yaml
2. Configure logging
3. Fetch files:
   - CAMS: Gmail → DownloadURL → HTTP download (ZIP)
   - KFIN: Playwright-based scrape
   - AMFI: Direct CSV
4. Copy all final files into unified DOWNLOADS_DIR
5. Run loader:
   - Ingest raw into cams_raw / kfin_raw / amfi_raw (+ *_without_isin)
   - Run SQL stored procedures to build master tables
"""

import logging
import logging.config
import shutil
import zipfile
from pathlib import Path
from datetime import datetime, timezone
from typing import Optional

import requests
import yaml

from Etl.utils import (
    settings,
    DOWNLOADS_DIR,
    ARCHIVE_DIR,
    ensure_dir,
)
from Etl.gmail_fetcher import GmailFetcher
from Etl.kfin_fetcher import KfinFetcher
from Etl.amfi_fetcher import AmfiFetcher
from Etl.loader import EtlOrchestrator

logger = logging.getLogger("run_etl")


# -------------------------------------------------------
# LOGGING CONFIG (uses paths.logs_dir from settings.yaml)
# -------------------------------------------------------
def configure_logging(settings_dict: dict) -> Path:
    paths_cfg = settings_dict.get("paths", {})
    logs_dir = Path(paths_cfg.get("logs_dir", "C:/Data_MF/etl_logs"))
    logs_dir.mkdir(parents=True, exist_ok=True)

    ts = datetime.now(timezone.utc).strftime("%Y%m%d_%H%M%S")
    log_file = logs_dir / f"etl_{ts}.log"

    logging.basicConfig(
        level=logging.INFO,
        format="%(asctime)s [%(levelname)s] %(name)s - %(message)s",
        handlers=[
            logging.FileHandler(log_file, encoding="utf-8"),
            logging.StreamHandler(),
        ],
    )

    # Refresh module-level logger
    global logger
    logger = logging.getLogger("run_etl")
    logger.info(f"📜 Logging to → {log_file}")
    return log_file


# -------------------------------------------------------
# Helper: copy only if src != dest
# -------------------------------------------------------
def safe_copy(src: Path, dest_dir: Path) -> Path:
    dest_dir = ensure_dir(dest_dir)
    dest = dest_dir / src.name
    if src.resolve() != dest.resolve():
        shutil.copy(src, dest)
    return dest


# -------------------------------------------------------
# Helper: HTTP download for CAMS DownloadURL
# -------------------------------------------------------
def download_file(url: str, dest_dir: Path, filename: Optional[str] = None) -> Path:
    dest_dir = ensure_dir(dest_dir)
    if not filename:
        # simple name from URL
        filename = url.split("/")[-1].split("?")[0] or "download.bin"

    out_path = dest_dir / filename
    logger.info(f"⬇️ Downloading from {url} → {out_path}")

    r = requests.get(url, timeout=120)
    r.raise_for_status()

    with open(out_path, "wb") as f:
        f.write(r.content)

    logger.info(f"✅ Downloaded → {out_path}")
    return out_path


# -------------------------------------------------------
# Helper: unzip CAMS ZIP (password handled by Gmail config)
# -------------------------------------------------------
def unzip_cams_zip(zip_path: Path, password: Optional[str] = None) -> Optional[Path]:
    """
    Unzips CAMS ZIP into same folder, returns first XLS/XLSX path (or None).
    """
    if not zip_path.exists():
        logger.error(f"❌ ZIP not found: {zip_path}")
        return None

    try:
        with zipfile.ZipFile(zip_path, "r") as zf:
            if password:
                zf.setpassword(password.encode())
            zf.extractall(zip_path.parent)
        logger.info(f"📂 Unzipped → {zip_path.parent}")
    except Exception as e:
        logger.error(f"❌ Failed to unzip {zip_path}: {e}")
        return None

    for f in zip_path.parent.glob("*.xls*"):
        logger.info(f"📄 Extracted CAMS file: {f}")
        return f

    logger.warning("⚠️ No XLS/XLSX found after unzip.")
    return None


# -------------------------------------------------------
# MAIN PIPELINE
# -------------------------------------------------------
def main():
    # 1) Logging
    configure_logging(settings)
    logger.info("🚀 Starting MF ETL pipeline")

    paths_cfg = settings.get("paths", {})
    gmail_cfg = settings.get("gmail", {})
    kfin_cfg = settings.get("kfin", {})
    amfi_cfg = settings.get("amfi", {})

    # Build paths dict for fetchers
    # (they expect things like paths['kfin_dir'], paths['amfi_dir'])
    paths = {
        "downloads_dir": str(DOWNLOADS_DIR),
        "archive_dir": str(ARCHIVE_DIR),
        "cams_dir": gmail_cfg.get("download_dir", str(DOWNLOADS_DIR / "cams")),
        "kfin_dir": kfin_cfg.get("download_dir", str(DOWNLOADS_DIR / "kfin")),
        "amfi_dir": amfi_cfg.get("download_dir", str(DOWNLOADS_DIR / "amfi")),
    }

    cams_dir = Path(paths["cams_dir"])
    kfin_dir = Path(paths["kfin_dir"])
    amfi_dir = Path(paths["amfi_dir"])

    cams_dir.mkdir(parents=True, exist_ok=True)
    kfin_dir.mkdir(parents=True, exist_ok=True)
    amfi_dir.mkdir(parents=True, exist_ok=True)

    # ---------------------------------------------------
    # 1. Fetch CAMS (Gmail → DownloadURL → ZIP → XLS)
    # ---------------------------------------------------
    logger.info("📥 Fetching CAMS mailback URL...")
    gfetch = GmailFetcher(settings, paths)
    cams_url = gfetch.fetch_latest_cams_downloadurl()

    cams_file_for_loader: Optional[Path] = None

    if cams_url:
        try:
            zip_path = download_file(
                cams_url,
                cams_dir,
                filename=f"cams_{datetime.now().strftime('%Y%m%d_%H%M%S')}.zip",
            )
            cams_zip_password = gmail_cfg.get("zip_password")
            cams_xls = unzip_cams_zip(zip_path, password=cams_zip_password)
            if cams_xls:
                cams_file_for_loader = safe_copy(cams_xls, DOWNLOADS_DIR)
                logger.info(f"✅ CAMS ready for ETL → {cams_file_for_loader}")
        except Exception as e:
            logger.exception(f"❌ CAMS fetch/download failed: {e}")
    else:
        logger.warning("⚠️ No CAMS DownloadURL found in recent emails.")

    # ---------------------------------------------------
    # 2. Fetch KFIN Scheme file (Playwright)
    # ---------------------------------------------------
    logger.info("📥 Fetching KFIN scheme...")
    kfetch = KfinFetcher(settings, paths)
    kfin_file_for_loader: Optional[Path] = None

    try:
        kfin_file = kfetch.fetch_scheme_file()
        if kfin_file and kfin_file.exists():
            kfin_file_for_loader = safe_copy(kfin_file, DOWNLOADS_DIR)
            logger.info(f"✅ KFIN ready for ETL → {kfin_file_for_loader}")
        else:
            logger.warning("⚠️ KFIN file not downloaded.")
    except Exception as e:
        logger.exception(f"❌ KFIN fetch failed: {e}")

    # ---------------------------------------------------
    # 3. Fetch AMFI CSV
    # ---------------------------------------------------
    logger.info("📥 Fetching AMFI CSV...")
    afetch = AmfiFetcher(settings, paths)
    amfi_file_for_loader: Optional[Path] = None

    try:
        amfi_file = afetch.download_csv()
        if amfi_file and amfi_file.exists():
            amfi_file_for_loader = safe_copy(amfi_file, DOWNLOADS_DIR)
            logger.info(f"✅ AMFI ready for ETL → {amfi_file_for_loader}")
        else:
            logger.warning("⚠️ AMFI file not downloaded.")
    except Exception as e:
        logger.exception(f"❌ AMFI fetch failed: {e}")

    # ---------------------------------------------------
    # 4. Load raw tables from unified DOWNLOADS_DIR
    # ---------------------------------------------------
    logger.info("📦 Loading raw data into Postgres...")
    orchestrator = EtlOrchestrator()
    orchestrator.run()

    # ---------------------------------------------------
    # 5. Run SQL pipeline (stored procedures)
    # ---------------------------------------------------
    logger.info("🧠 Running stored procedures...")
    orchestrator.run_etl_sql_pipeline()

    logger.info("🎉 ETL Completed Successfully!")


if __name__ == "__main__":
    main()
