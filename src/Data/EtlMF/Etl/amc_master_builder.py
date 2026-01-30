# Etl/amc_master_builder.py
"""
AMC Master Builder Module
--------------------------
Automated extraction and maintenance of AMC master data
from CAMS, KFIN, and future sources (BSE, NSE)
"""

import logging
import pandas as pd
from pathlib import Path
from sqlalchemy import text
from typing import Optional

from Etl.utils import engine

logger = logging.getLogger("mf.etl.amc_master")


class AmcMasterBuilder:
    """
    Manages AMC master data extraction, matching, and approval workflow
    """
    
    def __init__(self):
        self.engine = engine
    
    # ========================================================
    # EXTRACTION & GENERATION
    # ========================================================
    
    def build_amc_master(self, auto_approve_threshold: float = 0.85):
        """
        Complete AMC master build workflow:
        1. Extract from raw sources
        2. Match to existing master
        3. Auto-generate new AMCs
        """
        logger.info("🚀 Starting AMC master build...")
        
        with self.engine.begin() as conn:
            # Step 1: Extract
            logger.info("📤 Extracting AMCs from raw sources...")
            conn.execute(text("CALL sp_extract_amcs_from_raw()"))
            
            # Step 2: Match
            logger.info("🔗 Matching to existing master...")
            conn.execute(text("CALL sp_match_staging_to_master()"))
            
            # Step 3: Generate
            logger.info("🏗️  Generating master data...")
            conn.execute(text(
                "CALL sp_generate_amc_master(:threshold)"
            ), {"threshold": auto_approve_threshold})
        
        logger.info("✅ AMC master build complete!")
        
        # Return summary
        return self.get_build_summary()
    
    def extract_from_raw(self):
        """Extract AMCs from raw tables only (incremental update)"""
        logger.info("📤 Extracting AMCs from raw sources...")
        with self.engine.begin() as conn:
            conn.execute(text("CALL sp_extract_amcs_from_raw()"))
        logger.info("✅ Extraction complete")
    
    # ========================================================
    # REPORTING & REVIEW
    # ========================================================
    
    def get_build_summary(self) -> dict:
        """Get summary statistics of AMC master build"""
        with self.engine.connect() as conn:
            # Total AMCs in master
            master_count = conn.execute(text(
                "SELECT COUNT(*) FROM amc_master"
            )).scalar()
            
            # Pending staging records
            pending_count = conn.execute(text(
                "SELECT COUNT(*) FROM amc_staging WHERE status = 'PENDING'"
            )).scalar()
            
            # Approved staging records
            approved_count = conn.execute(text(
                "SELECT COUNT(*) FROM amc_staging WHERE status = 'APPROVED'"
            )).scalar()
            
            # Coverage
            coverage = conn.execute(text("""
                SELECT 
                    COUNT(*) FILTER (WHERE cams_amc_code IS NOT NULL) as has_cams,
                    COUNT(*) FILTER (WHERE kfin_amc_code IS NOT NULL) as has_kfin,
                    COUNT(*) FILTER (WHERE bse_amc_code IS NOT NULL) as has_bse,
                    COUNT(*) as total
                FROM amc_master
            """)).fetchone()
        
        summary = {
            "master_count": master_count,
            "pending_review": pending_count,
            "approved": approved_count,
            "coverage": {
                "cams": coverage.has_cams,
                "kfin": coverage.has_kfin,
                "bse": coverage.has_bse,
                "total": coverage.total
            }
        }
        
        logger.info(f"📊 Summary: {summary}")
        return summary
    
    def get_pending_reviews(self) -> pd.DataFrame:
        """Get staging records that need manual review"""
        query = "SELECT * FROM v_amc_staging_pending"
        df = pd.read_sql(query, self.engine)
        logger.info(f"📋 Found {len(df)} pending reviews")
        return df
    
    def get_master_coverage(self) -> pd.DataFrame:
        """Get AMC master with code coverage details"""
        query = "SELECT * FROM v_amc_master_coverage"
        df = pd.read_sql(query, self.engine)
        return df
    
    def export_pending_reviews(self, output_path: str = "amc_pending_review.xlsx"):
        """Export pending reviews to Excel for manual review"""
        df = self.get_pending_reviews()
        
        if df.empty:
            logger.info("✅ No pending reviews!")
            return None
        
        out_path = Path(output_path)
        df.to_excel(out_path, index=False, engine='openpyxl')
        logger.info(f"📄 Exported pending reviews to {out_path}")
        return out_path
    
    # ========================================================
    # APPROVAL & UPDATES
    # ========================================================
    
    def approve_staging(
        self, 
        staging_id: str, 
        master_amc_id: str, 
        reviewer: str = "system"
    ):
        """Approve a staging record and merge with master"""
        logger.info(f"✓ Approving staging {staging_id} → master {master_amc_id}")
        
        with self.engine.begin() as conn:
            conn.execute(
                text("CALL sp_approve_staging_amc(:sid, :mid, :rev)"),
                {
                    "sid": staging_id,
                    "mid": master_amc_id,
                    "rev": reviewer
                }
            )
        
        logger.info("✅ Approval complete")
    
    def bulk_approve_high_confidence(self, threshold: float = 0.85):
        """Auto-approve all staging records above threshold"""
        logger.info(f"🔄 Bulk approving matches with confidence >= {threshold}")
        
        with self.engine.begin() as conn:
            result = conn.execute(text("""
                UPDATE amc_staging
                SET 
                    status = 'APPROVED',
                    reviewed_by = 'system',
                    reviewed_at = now()
                WHERE status = 'PENDING'
                  AND suggested_amc_id IS NOT NULL
                  AND match_confidence >= :threshold
                RETURNING id
            """), {"threshold": threshold})
            
            count = result.rowcount
        
        logger.info(f"✅ Auto-approved {count} records")
        return count
    
    def update_amc_code(
        self, 
        source: str,          # 'bse' / 'nse' / 'cams' / 'kfin'
        amc_code: str,        # e.g., 'BSE001'
        amc_id: Optional[str] = None,
        amc_name: Optional[str] = None
    ):
        """Add source-specific code to existing AMC"""
        if not amc_id and not amc_name:
            raise ValueError("Either amc_id or amc_name must be provided")
        
        logger.info(f"📝 Updating {source.upper()} code: {amc_code}")
        
        with self.engine.begin() as conn:
            conn.execute(
                text("CALL sp_update_amc_codes(:src, :code, :id, :name)"),
                {
                    "src": source,
                    "code": amc_code,
                    "id": amc_id,
                    "name": amc_name
                }
            )
        
        logger.info("✅ Code updated")
    
    # ========================================================
    # BATCH OPERATIONS (for BSE/NSE integration)
    # ========================================================
    
    def bulk_update_codes_from_csv(
        self,
        csv_path: str,
        source: str,         # 'bse' or 'nse'
        amc_name_col: str = "amc_name",
        amc_code_col: str = "amc_code"
    ):
        """
        Bulk update AMC codes from CSV (for BSE/NSE integration)
        
        CSV format:
        amc_name,amc_code
        HDFC Asset Management,BSE001
        ICICI Prudential AMC,BSE002
        """
        logger.info(f"📥 Bulk updating {source.upper()} codes from {csv_path}")
        
        df = pd.read_csv(csv_path)
        
        if amc_name_col not in df.columns or amc_code_col not in df.columns:
            raise ValueError(f"CSV must contain '{amc_name_col}' and '{amc_code_col}' columns")
        
        success_count = 0
        error_count = 0
        
        for idx, row in df.iterrows():
            try:
                self.update_amc_code(
                    source=source,
                    amc_code=row[amc_code_col],
                    amc_name=row[amc_name_col]
                )
                success_count += 1
            except Exception as e:
                logger.warning(f"⚠️ Failed to update {row[amc_name_col]}: {e}")
                error_count += 1
        
        logger.info(f"✅ Updated {success_count} codes, {error_count} errors")
        return {"success": success_count, "errors": error_count}
    
    # ========================================================
    # INTEGRATION WITH ETL PIPELINE
    # ========================================================
    
    def integrate_with_etl(self):
        """
        Call this after raw data load to update AMC master
        This is the entry point for the ETL pipeline
        """
        logger.info("🔄 Running AMC master integration...")
        
        # Extract new AMCs from latest raw data
        self.extract_from_raw()
        
        # Match to existing master
        with self.engine.begin() as conn:
            conn.execute(text("CALL sp_match_staging_to_master()"))
        
        # Auto-approve high confidence
        approved = self.bulk_approve_high_confidence(threshold=0.85)
        
        # Create new AMCs for unmatched
        with self.engine.begin() as conn:
            conn.execute(text("CALL sp_generate_amc_master(0.85)"))
        
        # Get summary
        summary = self.get_build_summary()
        
        # Export pending if any
        if summary["pending_review"] > 0:
            self.export_pending_reviews()
            logger.warning(
                f"⚠️ {summary['pending_review']} AMCs need manual review. "
                "Check 'amc_pending_review.xlsx'"
            )
        
        return summary


# ========================================================
# COMMAND-LINE INTERFACE
# ========================================================

if __name__ == "__main__":
    import argparse
    
    parser = argparse.ArgumentParser(description="AMC Master Builder")
    parser.add_argument(
        "action",
        choices=["build", "extract", "approve", "export", "update-codes"],
        help="Action to perform"
    )
    parser.add_argument("--threshold", type=float, default=0.85)
    parser.add_argument("--csv", type=str, help="CSV file for bulk code update")
    parser.add_argument("--source", type=str, choices=["bse", "nse", "cams", "kfin"])
    
    args = parser.parse_args()
    
    builder = AmcMasterBuilder()
    
    if args.action == "build":
        builder.build_amc_master(args.threshold)
    
    elif args.action == "extract":
        builder.extract_from_raw()
    
    elif args.action == "approve":
        builder.bulk_approve_high_confidence(args.threshold)
    
    elif args.action == "export":
        builder.export_pending_reviews()
    
    elif args.action == "update-codes":
        if not args.csv or not args.source:
            print("Error: --csv and --source required for update-codes")
        else:
            builder.bulk_update_codes_from_csv(args.csv, args.source)
