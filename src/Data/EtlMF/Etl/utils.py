# Etl/utils.py
"""
utils.py
---------
Shared utilities for the ETL pipeline:
- Directory creation
- YAML configuration loading
- Retry helper
- Global settings + engine + common dirs
"""

import os
import time
import yaml
import logging
from pathlib import Path

logger = logging.getLogger("mf.etl.utils")

# -------------------------------------------------------------------------
# BASIC HELPERS
# -------------------------------------------------------------------------
def ensure_dir(path: str | Path) -> Path:
    """Ensure directory exists and return Path object."""
    p = Path(path)
    if not p.exists():
        p.mkdir(parents=True, exist_ok=True)
        logger.debug(f"📁 Created directory: {p}")
    return p


def load_yaml_config(path: str | Path) -> dict:
    """Load a YAML configuration file safely."""
    p = Path(path)
    if not p.exists():
        raise FileNotFoundError(f"YAML config not found: {p}")
    with open(p, "r", encoding="utf-8") as fh:
        cfg = yaml.safe_load(fh)
    logger.info(f"✅ Parsed YAML keys: {list(cfg.keys())}")
    return cfg


def build_retry(max_attempts: int = None, delay: int = 5, max_retries: int = None):
    """
    Decorator for retrying a function call with delay and logging.
    Accepts either `max_attempts` or legacy `max_retries`.
    Used by: GmailFetcher, AmfiFetcher, etc.
    """
    if max_retries is not None:
        max_attempts = max_retries
    if max_attempts is None:
        max_attempts = 3

    def decorator(func):
        def wrapper(*args, **kwargs):
            for attempt in range(1, max_attempts + 1):
                try:
                    return func(*args, **kwargs)
                except Exception as e:
                    if attempt == max_attempts:
                        logger.error(
                            f"❌ {func.__name__} failed after {max_attempts} attempts: {e}"
                        )
                        raise
                    logger.warning(
                        f"⚠️ Attempt {attempt}/{max_attempts} failed: {e}. "
                        f"Retrying in {delay}s..."
                    )
                    time.sleep(delay)
        return wrapper

    return decorator


def env_or_default(env_name: str, default=None):
    """Fetch environment variable or default value."""
    return os.getenv(env_name, default)


# -------------------------------------------------------------------------
# LOAD GLOBAL SETTINGS (using new settings.yaml structure, v4)
# -------------------------------------------------------------------------
# Etl/  -> parent -> EtlMF/
SETTINGS_PATH = Path(__file__).parent.parent / "Config" / "settings.yaml"
settings = load_yaml_config(SETTINGS_PATH)

# ----- Directory setup (NEW: uses settings['paths']) -----
paths_cfg = settings.get("paths", {})

# Base data root (C:/Data_MF by default)
BASE_DIR: Path = Path(paths_cfg.get("data_root", "C:/Data_MF"))

# Unified downloads dir (e.g. C:/Data_MF/downloads)
_downloads_raw = paths_cfg.get("downloads_dir", BASE_DIR / "downloads")
DOWNLOADS_DIR: Path = ensure_dir(_downloads_raw)

# Temp dir (not in YAML, just under data_root)
TMP_DIR: Path = ensure_dir(BASE_DIR / "tmp")

# Archive dir from YAML (e.g. C:/Data_MF/archive)
_archive_raw = paths_cfg.get("archive_dir", BASE_DIR / "archive")
ARCHIVE_DIR: Path = ensure_dir(_archive_raw)

logger.info(f"📁 DOWNLOADS_DIR: {DOWNLOADS_DIR}")
logger.info(f"📁 TMP_DIR: {TMP_DIR}")
logger.info(f"📁 ARCHIVE_DIR: {ARCHIVE_DIR}")

# -------------------------------------------------------------------------
# POSTGRES DATABASE ENGINE (used by loader.py & excel_to_postgres.py)
# -------------------------------------------------------------------------
from sqlalchemy import create_engine  # noqa: E402

pg = settings.get("postgres", {})
user = pg.get("user")
password = pg.get("password")
host = pg.get("host", "localhost")
port = pg.get("port", 5432)
database = pg.get("database")

if not (user and password and database):
    raise ValueError("❌ Postgres configuration missing in settings.yaml")

DB_URL = pg.get(
    "url",
    f"postgresql+psycopg2://{user}:{password}@{host}:{port}/{database}",
)

engine = create_engine(DB_URL, pool_pre_ping=True)
logger.info(f"🚀 Connected SQLAlchemy engine to {DB_URL}")
