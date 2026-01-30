from typing import Any, Dict, List, Optional
import pandas as pd
from sqlalchemy.engine import Engine
from sqlalchemy import create_engine, inspect
import logging
from pathlib import Path
import re
import numpy as np
import io
import csv

logger = logging.getLogger("mf.etl.excel_to_postgres")


def psql_insert_copy(table, conn, keys, data_iter):
    """Fast PostgreSQL COPY method for pandas to_sql."""
    csv_buffer = io.StringIO()
    writer = csv.writer(csv_buffer)
    writer.writerows(data_iter)
    csv_buffer.seek(0)

    raw_conn = conn.connection
    cursor = raw_conn.cursor()
    columns = ', '.join([f'"{k}"' for k in keys])
    copy_cmd = f'COPY "{table.name}" ({columns}) FROM STDIN WITH CSV'

    try:
        cursor.copy_expert(copy_cmd, csv_buffer)
        raw_conn.commit()
    except Exception as e:
        raw_conn.rollback()
        raise e
    finally:
        cursor.close()


def build_engine(settings: dict) -> Engine:
    pg = settings.get("postgres", {})
    user = pg.get("user")
    password = pg.get("password")
    host = pg.get("host", "localhost")
    port = pg.get("port", 5432)
    db = pg.get("database")
    if not (user and password and db):
        raise ValueError("Postgres settings incomplete: user/password/database required")
    conn = f"postgresql+psycopg2://{user}:{password}@{host}:{port}/{db}"
    return create_engine(conn, pool_pre_ping=True)


def _normalize_name_for_compare(name: str) -> str:
    return re.sub(r'[^a-z0-9]', '', str(name).lower())


def _map_df_columns_to_table(df: pd.DataFrame, engine: Engine, table_name: str) -> Dict[str, str]:
    inspector = inspect(engine)
    try:
        tbl_cols = [c["name"] for c in inspector.get_columns(table_name)]
    except Exception:
        return {}

    tbl_norm = { _normalize_name_for_compare(c): c for c in tbl_cols }
    mapping = {}

    for col in df.columns:
        if col in tbl_cols:
            mapping[col] = col
            continue
        n = _normalize_name_for_compare(col)
        if n in tbl_norm:
            mapping[col] = tbl_norm[n]
            continue
        alt = col.replace("_", " ")
        n_alt = _normalize_name_for_compare(alt)
        if n_alt in tbl_norm:
            mapping[col] = tbl_norm[n_alt]
            continue
        alt2 = col.replace(" ", "_")
        n_alt2 = _normalize_name_for_compare(alt2)
        if n_alt2 in tbl_norm:
            mapping[col] = tbl_norm[n_alt2]
            continue
    return mapping


def detect_date_columns(df: pd.DataFrame, sample_size: int = 100) -> List[str]:
    date_columns = []
    sample = df.head(sample_size)
    for col in df.columns:
        if sample[col].isna().all():
            continue
        non_null = sample[col].dropna().astype(str)
        if len(non_null) == 0:
            continue
        date_pattern = r'^\d{1,2}[-/]\d{1,2}[-/]\d{2,4}$|^\d{4}[-/]\d{1,2}[-/]\d{1,2}$'
        matches = non_null.str.match(date_pattern, na=False)
        if matches.sum() / len(non_null) > 0.7:
            date_columns.append(col)
    return date_columns


def parse_date_column(series: pd.Series, col_name: str, date_formats: Optional[List[str]] = None) -> pd.Series:
    if date_formats is None:
        date_formats = [
            '%m-%d-%Y', '%d-%m-%Y', '%m/%d/%Y', '%d/%m/%Y',
            '%Y-%m-%d', '%d-%b-%Y', '%d/%b/%Y', '%b %d, %Y'
        ]
    original_series = series.copy()
    best_result, best_count, best_format = None, 0, None
    for fmt in date_formats:
        try:
            parsed = pd.to_datetime(series, format=fmt, errors='coerce')
            valid_count = parsed.notna().sum()
            if valid_count > best_count:
                best_result, best_count, best_format = parsed, valid_count, fmt
        except Exception:
            continue
    if best_result is not None and best_count > 0:
        logger.info(f"Parsed {best_count}/{len(series)} dates in '{col_name}' using {best_format}")
        return best_result
    else:
        return original_series


def prepare_dates_for_postgres(df: pd.DataFrame, date_columns: Optional[List[str]] = None) -> pd.DataFrame:
    df = df.copy()
    if date_columns is None:
        date_columns = detect_date_columns(df)
        if date_columns:
            logger.info(f"Auto-detected date columns: {date_columns}")
    
    # Process each date column safely
    for col in date_columns:
        if col not in df.columns:
            continue
        try:
            parsed = parse_date_column(df[col], col)
            if pd.api.types.is_datetime64_any_dtype(parsed):
                df[col] = parsed.dt.strftime('%Y-%m-%d')
                # Use proper None replacement
                df[col] = df[col].where(pd.notna(df[col]), None)
        except Exception as e:
            logger.warning(f"Could not parse dates in column '{col}': {e}")
            continue
    
    return df


def clean_numeric_columns(df: pd.DataFrame) -> pd.DataFrame:
    """
    Convert scientific notation to proper numeric or string format.
    Special handling:
      - Identifier columns (codes, ISIN, AMC Code) are forced to stay as text.
    """
    df = df.copy()
    # Normalize for case-insensitive matching
    text_force_columns = {
        "sch_code", "sf_code", "parent_scheme_code",
        "product code", "scheme code", "isin number",
        "amc code", "plan code", "option code"
    }
    
    for col in df.columns:
        # --- Force text for special columns ---
        if col.lower() in text_force_columns:
            logger.debug(f"Forcing column '{col}' to string type (identifier field).")
            df[col] = df[col].apply(
                lambda x: format(float(x), ".0f") if isinstance(x, (int, float, np.number)) and not pd.isna(x)
                else (str(x).strip() if x is not None else None)
            )
            continue
        # Only process object dtype columns (strings)
        if df[col].dtype == object:
            # Get a sample of non-null values
            sample = df[col].dropna().astype(str)
            if len(sample) == 0:
                continue
            
            # Detect cells that look like scientific numbers (1.00E+13, 1e10, etc.)
            has_scientific = sample.str.contains(r'[eE][+-]?\d+', regex=True, na=False).any()
            
            if has_scientific:
                try:
                    logger.debug(f"Processing scientific notation in column: {col}")
                    
                    # Convert to numeric first (handles scientific notation)
                    numeric_vals = pd.to_numeric(df[col], errors='coerce')
                    
                    # Check if column has infinity values
                    if np.isinf(numeric_vals).any():
                        logger.warning(f"Column '{col}' contains infinity values - keeping as string")
                        continue
                    
                    # Get non-null numeric values to analyze
                    non_null_numeric = numeric_vals.dropna()
                    if len(non_null_numeric) == 0:
                        continue
                    
                    # Check if all values are effectively integers (no meaningful decimal parts)
                    # This handles cases like "1.00E+13" which should be integer
                    is_integer = np.allclose(non_null_numeric, non_null_numeric.round(), rtol=1e-9, atol=1e-9)
                    
                    if is_integer:
                        # All values are whole numbers - convert to Int64 (nullable integer)
                        df[col] = numeric_vals.round().astype('Int64')
                        logger.info(f"✓ Converted scientific notation in column '{col}' to integers")
                    else:
                        # Values have decimal parts - MUST use proper float64 dtype
                        # CRITICAL FIX: Use convert_dtypes to ensure proper dtype inference
                        df[col] = numeric_vals
                        # Force to float64 to prevent object dtype
                        if df[col].dtype == object:
                            df[col] = df[col].astype('float64')
                        logger.info(f"✓ Converted scientific notation in column '{col}' to decimals")
                        
                except Exception as ex:
                    logger.warning(f"Failed to normalize scientific notation in column {col}: {ex}")
            
            # Handle numeric-like strings (e.g., "1000.0", "1000.00") - only if ALL values are whole numbers
            elif sample.str.match(r'^\d+\.0+$', na=False).sum() == len(sample):
                try:
                    logger.debug(f"Converting whole number float strings in column: {col}")
                    df[col] = pd.to_numeric(df[col], errors='coerce').astype('Int64')
                    logger.info(f"✓ Converted float strings in column '{col}' to integers")
                except Exception as ex:
                    logger.debug(f"Could not convert float strings in {col}: {ex}")

    # --- Correct ISIN cleaning ---
    from Etl.cleaner import is_valid_isin

    for isin_col in ["ISIN", "isin", "isin_no", "isin number"]:
        if isin_col in df.columns:
            df[isin_col] = df[isin_col].apply(
                lambda v: v if is_valid_isin(str(v)) else None
            )

    
    return df

    
def detect_rta_source(file_path: str) -> str:
    """Detect RTA source (CAMS / KFIN / AMFI) from file name."""
    name = Path(file_path).stem.lower()
    
    # CAMS patterns (add numeric pattern detection)
    if "cams" in name or re.match(r'^\d{14}_\d+r\d+$', name):  # NEW: matches CAMS numeric pattern
        return "CAMS"
    elif "kfin" in name or "schemeinfonew" in name:
        return "KFIN"
    elif "amfi" in name:
        return "AMFI"
    else:
        logger.warning(f"⚠️ Could not detect RTA source from filename: {name}")
        return "UNKNOWN"

def safe_read_excel(file_path: str) -> pd.DataFrame:
    """
    Safely load Excel or CSV files with full fallback support.
    This wrapper is used by loader.py.
    """

    p = Path(file_path)
    suffix = p.suffix.lower()

    # Excel formats
    if suffix in [".xls", ".xlsx"]:
        try:
            return read_excel(file_path)
        except Exception as ex:
            logger.warning(f"Excel read failed for {file_path}: {ex}")

    # CSV formats (primary)
    if suffix in [".csv"]:
        try:
            return read_csv(file_path)
        except Exception as ex:
            logger.warning(f"CSV read failed for {file_path}: {ex}")

    # Unknown extension → try both
    try:
        return read_excel(file_path)
    except:
        try:
            return read_csv(file_path)
        except:
            raise RuntimeError(f"❌ Could not read file (Excel or CSV): {file_path}")


def load_dataframe_to_table(
    df: pd.DataFrame,
    engine: Engine,
    table_name: str,
    if_exists: str = "append",
    chunksize: int = 2000,
    date_columns: Optional[List[str]] = None,
    auto_detect_dates: bool = True,
    use_fast_copy: bool = True,
    source_file: Optional[str] = None,
    skip_amc_columns: bool = False  # NEW parameter
) -> None:
    """Load cleaned DataFrame into PostgreSQL."""
    if df is None or not isinstance(df, pd.DataFrame):
        raise ValueError("Invalid DataFrame passed for loading")
    if not table_name:
        raise ValueError("Table name must be provided")

    df = df.copy()
    
    # CRITICAL FIX: Remove duplicate columns FIRST (before any other operations)
    # Also ensure we're working with clean column names
    original_cols = len(df.columns)
    df = df.loc[:, ~df.columns.duplicated(keep='first')]
    
    if len(df.columns) < original_cols:
        logger.warning(f"⚠️  Removed {original_cols - len(df.columns)} duplicate columns")
    
    logger.info(f"📋 DataFrame has {len(df.columns)} unique columns after deduplication")
    
    # Additional safety: remove columns if they appear in both original case and modified case
    # (e.g., "AMC Code" and "amc_code" both present)
    cols_lower = {}
    cols_to_drop = []
    for col in df.columns:
        col_lower = str(col).lower().replace(' ', '_')
        if col_lower in cols_lower:
            cols_to_drop.append(col)
            logger.warning(f"⚠️  Dropping duplicate column '{col}' (conflicts with '{cols_lower[col_lower]}')")
        else:
            cols_lower[col_lower] = col
    
    if cols_to_drop:
        df = df.drop(columns=cols_to_drop)
        logger.info(f"📋 Cleaned to {len(df.columns)} columns after case-insensitive deduplication")
    
    # --- Ensure AMC columns exist (after deduplication) ---
    # Skip for AMFI table which doesn't have these columns
    if not skip_amc_columns:
        # Check if similar columns already exist (case-insensitive, space/underscore variants)
        existing_amc_variants = [c for c in df.columns if str(c).lower().replace(' ', '_').replace('-', '_') == 'amc']
        existing_amc_code_variants = [c for c in df.columns if str(c).lower().replace(' ', '_').replace('-', '_') == 'amc_code']
        
        # Only add if no variant exists
        if not existing_amc_variants and "amc" not in df.columns:
            df["amc"] = None
            logger.debug("Added 'amc' column")
        else:
            logger.debug(f"Skipping 'amc' addition - found existing variants: {existing_amc_variants}")

        if not existing_amc_code_variants and "amc_code" not in df.columns:
            df["amc_code"] = None
            logger.debug("Added 'amc_code' column")
        else:
            logger.debug(f"Skipping 'amc_code' addition - found existing variants: {existing_amc_code_variants}")

    # Column mapping
    try:
        mapping = _map_df_columns_to_table(df, engine, table_name)
        if mapping:
            df.rename(columns=mapping, inplace=True)
        else:
            df.columns = [str(c).strip().replace(" ", "_") for c in df.columns]
    except Exception as ex:
        logger.exception("Column mapping failed: %s", ex)
        df.columns = [str(c).strip().replace(" ", "_") for c in df.columns]

    # Data cleaning
    try:
        df = clean_numeric_columns(df)
    except Exception as ex:
        logger.warning(f"Numeric cleaning failed: {ex}")

    if auto_detect_dates or date_columns:
        try:
            df = prepare_dates_for_postgres(df, date_columns)
        except Exception as ex:
            logger.warning(f"Date conversion failed: {ex}")

    # Detect and assign RTA source
    try:
        source_name = detect_rta_source(source_file or table_name)
        df["source"] = source_name
        logger.info(f"Added column source='{source_name}' for table {table_name}")
    except Exception as ex:
        logger.warning(f"Could not set source: {ex}")

    # Load to Postgres
    logger.info("Loading dataframe with %d rows into table %s", len(df), table_name)
    try:
        if use_fast_copy:
            df.to_sql(
                table_name,
                engine,
                if_exists=if_exists,
                index=False,
                method=psql_insert_copy,
                chunksize=chunksize
            )
        else:
            df.to_sql(
                table_name,
                engine,
                if_exists=if_exists,
                index=False,
                method="multi",
                chunksize=chunksize
            )
        logger.info("✓ Successfully loaded %d rows into %s", len(df), table_name)
    except Exception as ex:
        logger.exception("Failed to load dataframe into %s: %s", table_name, ex)
        raise


def read_excel(file_path: str, sheet_name: Any = 0) -> pd.DataFrame:
    p = Path(file_path)
    if not p.exists():
        raise FileNotFoundError(f"Excel file not found: {file_path}")

    suffix = p.suffix.lower()
    logger.info("Reading excel file: %s", file_path)
    
    df = None
    
    if suffix == ".xls":
        try:
            df = pd.read_excel(file_path, sheet_name=sheet_name, engine="xlrd", dtype=str)
            logger.info("Read %s rows from %s using xlrd", len(df), file_path)
        except Exception as ex:
            logger.warning("xlrd read failed for %s: %s. Falling back to pandas auto engine.", file_path, ex)
    else:
        try:
            df = pd.read_excel(file_path, sheet_name=sheet_name, engine="openpyxl", dtype=str)
            logger.info("Read %s rows from %s using openpyxl", len(df), file_path)
        except Exception as ex:
            logger.warning("openpyxl read failed for %s: %s. Falling back to pandas auto engine.", file_path, ex)

    if df is None:
        df = pd.read_excel(file_path, sheet_name=sheet_name, dtype=str)
        logger.info("Read %s rows from %s using pandas default engine", len(df), file_path)
    
    # CRITICAL FIX: Handle duplicate column names immediately after reading
    if df.columns.duplicated().any():
        logger.warning(f"⚠️  Found duplicate columns in {file_path}, renaming...")
        # Rename duplicates by adding suffix
        cols = pd.Series(df.columns)
        for dup in cols[cols.duplicated()].unique():
            indices = cols[cols == dup].index.tolist()
            for i, idx in enumerate(indices[1:], start=2):  # Keep first, rename others
                cols.iloc[idx] = f"{dup}_dup{i}"
        df.columns = cols.tolist()
        logger.info(f"✅ Renamed duplicate columns: {df.columns.tolist()}")
    
    return df



def read_csv(file_path: str) -> pd.DataFrame:
    """
    Read CSV with multiple encoding fallbacks.
    Tries UTF-8 first, then common alternatives used in Indian financial data.
    """
    p = Path(file_path)
    if not p.exists():
        raise FileNotFoundError(f"CSV file not found: {file_path}")
    
    # Common encodings for Indian financial/CAMS data
    encodings = ['utf-8', 'latin1', 'iso-8859-1', 'cp1252', 'windows-1252']
    
    for encoding in encodings:
        try:
            logger.debug(f"Attempting to read {file_path} with encoding: {encoding}")
            df = pd.read_csv(file_path, dtype=str, encoding=encoding)
            logger.info(f"Read {len(df)} rows from {file_path} using encoding: {encoding}")
            return df
        except UnicodeDecodeError:
            logger.debug(f"Encoding {encoding} failed for {file_path}")
            continue
        except Exception as ex:
            # If it's not an encoding error, log and try next
            logger.debug(f"Read with {encoding} failed: {ex}")
            continue
    
    # If all encodings fail, try with errors='replace' as last resort
    try:
        logger.warning(f"⚠️ All standard encodings failed. Using latin1 with error replacement for {file_path}")
        with open(file_path, "r", encoding="latin1", errors="replace") as f:
            df = pd.read_csv(f, dtype=str)
        logger.info(f"Read {len(df)} rows from {file_path} using latin1 with error replacement")
        return df
    except Exception as ex:
        logger.error(f"❌ Could not read {file_path} with any encoding method: {ex}")
        raise