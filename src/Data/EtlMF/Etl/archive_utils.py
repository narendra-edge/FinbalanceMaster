# Etl/archive_utils.py
from pathlib import Path
import shutil
from datetime import datetime
from Etl.utils import ensure_dir

def archive_file(src_path: str, archive_dir: str, subdir: str = None):
    """
    Archive a file with optional subdirectory organization.
    
    Args:
        src_path: Path to source file
        archive_dir: Base archive directory
        subdir: Optional subdirectory (e.g., "CAMS", "KFIN", "AMFI")
    
    Returns:
        str: Path to archived file
    """
    src = Path(src_path)
    
    # Create subdir if specified (e.g., "CAMS", "KFIN", "AMFI")
    if subdir:
        archive_base = Path(archive_dir) / subdir
    else:
        archive_base = Path(archive_dir)
    
    ensure_dir(archive_base)
    
    ts = datetime.now().strftime("%Y%m%d_%H%M%S")
    dst = archive_base / f"{src.stem}_{ts}{src.suffix}"
    
    shutil.move(str(src), str(dst))
    return str(dst)