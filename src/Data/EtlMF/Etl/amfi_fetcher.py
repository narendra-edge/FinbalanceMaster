import logging
import requests
from pathlib import Path
from .utils import build_retry

logger = logging.getLogger("mf.etl.amfi_fetcher")

class AmfiFetcher:
    def __init__(self, settings: dict, paths: dict):
        self.settings = settings
        self.paths = paths
        self.acfg = settings.get("amfi", {})
        # FIX: Changed from self.url to self.download_url to match usage
        self.download_url = self.acfg.get("download_url", "")
        self.download_dir = Path(paths.get("amfi_dir", "./downloads/amfi"))
        self.download_dir.mkdir(parents=True, exist_ok=True)
    
    @build_retry(max_attempts=3)
    def download_csv(self) -> Path:
        """Download AMFI Scheme CSV"""
        logger.info("📡 Downloading AMFI data...")
        
        if not self.download_url:
            raise ValueError("AMFI download URL not configured")
        
        r = requests.get(self.download_url, timeout=30)
        
        if r.status_code != 200:
            raise RuntimeError(f"Failed to fetch AMFI data ({r.status_code})")
        
        out_path = self.download_dir / "amfi_scheme.csv"
        
        with open(out_path, "wb") as f:
            f.write(r.content)
        
        logger.info(f"✅ AMFI data saved → {out_path}")
        return out_path