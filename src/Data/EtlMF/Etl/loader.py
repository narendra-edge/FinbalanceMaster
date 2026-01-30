# Etl/loader.py
import logging
from pathlib import Path

import pandas as pd
from sqlalchemy import text

from Etl.utils import engine, DOWNLOADS_DIR, ARCHIVE_DIR
from Etl.excel_to_postgres import (
    safe_read_excel,
    load_dataframe_to_table,
    detect_rta_source,
)
from Etl.archive_utils import archive_file
from Etl.cleaner import is_valid_isin

logger = logging.getLogger("mf.etl.loader")


class EtlOrchestrator:
    def __init__(self):
        self.downloads_dir = Path(DOWNLOADS_DIR)
        self.downloads_dir.mkdir(parents=True, exist_ok=True)

    # ---------------------------------------------------
    # INGEST SINGLE FILE → raw tables
    # ---------------------------------------------------
    def ingest_file(self, path: str):
        p = Path(path)
        
        # CRITICAL FIX: Skip directories
        if p.is_dir():
            logger.info(f"📁 Skipping directory → {p}")
            return

        # Read Excel/CSV
        df = safe_read_excel(path)

        if df is None or df.empty:
            logger.warning(f"⚠️ No data in {path}")
            archive_file(path, archive_dir=str(ARCHIVE_DIR), subdir="EMPTY")
            return

        # Detect RTA source
        source = detect_rta_source(path)
        logger.info(f"🔎 Detected source = {source}")

        # ------------------------------------------------
        # CAMS processing
        # ------------------------------------------------
        if source == "CAMS":
            # Split valid vs invalid ISIN rows into cams_raw / cams_without_isin
            isin_cols = [c for c in df.columns if c.lower() in ("isin_no", "isin")]
            if isin_cols:
                col = isin_cols[0]
                # FIX: Handle NaN values in ISIN column
                df_valid = df[df[col].apply(lambda x: is_valid_isin(x) if pd.notna(x) else False)]
                df_invalid = df[~df[col].apply(lambda x: is_valid_isin(x) if pd.notna(x) else False)]
            else:
                df_valid = df
                df_invalid = pd.DataFrame()

            if not df_valid.empty:
                load_dataframe_to_table(
                    df_valid,
                    engine,
                    table_name="cams_raw",
                    source_file=path,
                )

            if not df_invalid.empty:
                load_dataframe_to_table(
                    df_invalid,
                    engine,
                    table_name="cams_without_isin",
                    source_file=path,
                )

        # ------------------------------------------------
        # KFIN processing
        # ------------------------------------------------
        elif source == "KFIN":
            # KFIN ISIN column is usually "ISIN Number"
            isin_cols = [c for c in df.columns if c.lower() in ("isin number", "isin")]
            if isin_cols:
                col = isin_cols[0]
                # FIX: Handle NaN values in ISIN column
                df_valid = df[df[col].apply(lambda x: is_valid_isin(x) if pd.notna(x) else False)]
                df_invalid = df[~df[col].apply(lambda x: is_valid_isin(x) if pd.notna(x) else False)]
            else:
                df_valid = df
                df_invalid = pd.DataFrame()

            if not df_valid.empty:
                load_dataframe_to_table(
                    df_valid,
                    engine,
                    table_name="kfin_raw",
                    source_file=path,
                    skip_amc_columns=True,  # KFIN has "AMC Code" and "AMC Name" already
                )

            if not df_invalid.empty:
                load_dataframe_to_table(
                    df_invalid,
                    engine,
                    table_name="kfin_without_isin",
                    source_file=path,
                    skip_amc_columns=True,  # KFIN has "AMC Code" and "AMC Name" already
                )

        # ------------------------------------------------
        # AMFI processing
        # ------------------------------------------------
        elif source == "AMFI":
            # AMFI schema has "AMC" column (uppercase) but NOT amc_code
            # Don't let load_dataframe_to_table add amc/amc_code columns
            load_dataframe_to_table(
                df,
                engine,
                table_name="amfi_raw",
                source_file=path,
                skip_amc_columns=True,  # Critical: AMFI doesn't need these
            )

        # ------------------------------------------------
        # Unknown source
        # ------------------------------------------------
        else:
            logger.warning(f"⚠️ Unknown RTA type for {path}, archiving only")

        # Archive original file (by source) - FIX: Pass archive_dir parameter
        archive_file(path, archive_dir=str(ARCHIVE_DIR), subdir=source)

    # ---------------------------------------------------
    # BUILD AMC MASTER CANDIDATES FOR MANUAL REVIEW
    # ---------------------------------------------------
    def build_amc_master_candidates(self) -> Path:
        logger.info("🔎 Extracting AMC candidates from CAMS + KFIN raw.")

        sql = """
        SELECT DISTINCT amc AS amc_name, amc_code
        FROM cams_raw
        WHERE amc IS NOT NULL AND amc <> ''

        UNION

        SELECT DISTINCT "AMC Name" AS amc_name, "AMC Code" AS amc_code
        FROM kfin_raw
        WHERE "AMC Name" IS NOT NULL AND "AMC Name" <> ''
        """

        df = pd.read_sql(sql, engine)
        out = Path("amc_master_candidates.csv")
        df.to_csv(out, index=False)

        logger.info(f"📄 AMC Master candidates saved → {out}")
        return out

    # ---------------------------------------------------
    # RUN ALL STORED PROCEDURES
    # ---------------------------------------------------
    def run_etl_sql_pipeline(self):
        logger.info("🧠 Executing SQL stored procedures.")

        with engine.begin() as conn:
            conn.execute(text("CALL sp_refresh_cams_scheme_master()"))
            conn.execute(text("CALL sp_refresh_kfin_scheme_master()"))
            conn.execute(text("CALL sp_refresh_amfi_scheme()"))
            conn.execute(text("CALL sp_refresh_rta_combined_scheme_master()"))
            conn.execute(text("CALL sp_refresh_scheme_mapping()"))
            conn.execute(text("CALL sp_refresh_scheme_master_final()"))

        logger.info("✅ SQL ETL pipeline completed.")

    # ---------------------------------------------------
    # MAIN EXECUTION — PROCESS ALL DOWNLOADED FILES
    # ---------------------------------------------------
    def run(self):
        files = list(self.downloads_dir.glob("*"))
        if not files:
            logger.warning(f"⚠️ No files found in {self.downloads_dir}")
            return

        for f in files:
            # CRITICAL FIX: Skip directories at the loop level
            if f.is_dir():
                logger.info(f"📁 Skipping directory → {f}")
                continue
                
            try:
                self.ingest_file(str(f))
            except Exception as e:
                logger.exception(f"❌ Failed to ingest {f}: {e}")

        # Optional manual review step
        self.build_amc_master_candidates()

        logger.info("📦 Raw ingestion complete.")