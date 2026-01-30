import re
import pandas as pd
from typing import Tuple, List, Optional

# Correct ISIN pattern: IN + 10 alphanumeric = total 12 chars
ISIN_PATTERN = re.compile(r"(IN[A-Z0-9]{10})", flags=re.IGNORECASE)


def is_valid_isin(txt: str) -> bool:
    """
    True only if txt is exactly 12 chars, starts with IN, followed by 10 alnum.
    Handles NaN, None, and non-string values.
    """
    if not txt or pd.isna(txt):  # Handle NaN/None
        return False
    
    txt = str(txt).strip().upper()  # Convert to string first
    
    if not txt:  # Empty after strip
        return False
        
    return bool(re.fullmatch(r"IN[A-Z0-9]{10}", txt))


def extract_isins_from_field(isin_field: Optional[str]) -> Tuple[Optional[str], Optional[str], Optional[List[str]]]:
    """
    Extract one or more ISINs from any text field.
    Returns: (isin1, isin2, [all_isins])
    """
    if not isin_field or pd.isna(isin_field):
        return None, None, None

    text = str(isin_field).upper().replace(" ", "")
    matches = ISIN_PATTERN.findall(text)

    if not matches:
        return None, None, None

    primary = matches[0]
    secondary = matches[1] if len(matches) > 1 else None
    return primary, secondary, matches


def normalize_scheme_name(name: str) -> str:
    """
    Very light normalization — no aggressive cleaning.
    Final pipeline does not normalize scheme names.
    """
    if not name or pd.isna(name):
        return ""
    s = str(name)
    return re.sub(r"\s+", " ", s).strip()


def normalize_amc_name(name: str) -> str:
    """
    Normalizes AMC name for AMC master matching.
    Removes Mutual Fund suffix, special characters, extra spaces.
    """
    if not name or pd.isna(name):
        return ""
    s = str(name)
    s = re.sub(r"(?i)\bmutual\s*fund\b", "", s)
    s = re.sub(r"[^A-Za-z0-9 &]", " ", s)
    s = re.sub(r"\s+", " ", s)
    return s.strip().title()