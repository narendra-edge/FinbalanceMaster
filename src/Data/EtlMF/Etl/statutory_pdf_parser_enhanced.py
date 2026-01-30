"""
statutory_pdf_parser_enhanced.py
---------------------------------
Production-ready PDF parser with SEBI format validation.
Handles: Portfolio, TER, AUM documents with multiple extraction strategies.
"""

import logging
import re
from pathlib import Path
from typing import List, Dict, Optional, Tuple
from datetime import datetime
from decimal import Decimal

import pandas as pd
import pdfplumber
from PyPDF2 import PdfReader
from sqlalchemy import create_engine, text

logger = logging.getLogger("mf.etl.pdf_parser")


class SebiFormatValidator:
    """Validates extracted data against SEBI format requirements."""
    
    SEBI_FORMATS = {
        'SEBI_PORTFOLIO_MONTHLY': {
            'required_fields': ['security_name', 'isin_code', 'market_value', 'portfolio_percentage'],
            'optional_fields': ['industry_sector', 'quantity'],
            'validations': {
                'percentage_sum': lambda data: abs(sum(d.get('portfolio_percentage', 0) for d in data) - 100) < 2.0,
                'min_holdings': lambda data: len(data) >= 5
            }
        },
        'SEBI_TER': {
            'required_fields': ['total_ter', 'plan_type'],
            'optional_fields': ['base_ter', 'gst_on_advisory_fee'],
            'validations': {
                'ter_range': lambda data: all(0 < d.get('total_ter', 0) < 5.0 for d in data),
                'has_both_plans': lambda data: len(data) >= 2
            }
        },
        'SEBI_AUM': {
            'required_fields': ['total_aum', 'as_of_date'],
            'optional_fields': ['t30_cities_aum', 'b30_cities_aum'],
            'validations': {
                'positive_aum': lambda data: all(d.get('total_aum', 0) > 0 for d in data)
            }
        }
    }
    
    @staticmethod
    def validate(data: List[Dict], format_type: str) -> Dict:
        """Validate data against SEBI format."""
        if format_type not in SebiFormatValidator.SEBI_FORMATS:
            return {'is_valid': False, 'errors': [f'Unknown format: {format_type}']}
        
        format_spec = SebiFormatValidator.SEBI_FORMATS[format_type]
        errors = []
        warnings = []
        
        # Check required fields
        for item in data:
            missing = [f for f in format_spec['required_fields'] if f not in item or item[f] is None]
            if missing:
                errors.append(f'Missing required fields: {missing}')
        
        # Run validations
        for validation_name, validation_fn in format_spec['validations'].items():
            try:
                if not validation_fn(data):
                    warnings.append(f'Validation failed: {validation_name}')
            except Exception as e:
                warnings.append(f'Validation error in {validation_name}: {e}')
        
        return {
            'is_valid': len(errors) == 0,
            'errors': errors,
            'warnings': warnings,
            'format_type': format_type
        }


class StatutoryPdfParser:
    """
    Enhanced PDF parser with SEBI compliance checking.
    """
    
    def __init__(self, db_url: str):
        self.db_url = db_url
        self.engine = create_engine(db_url, pool_pre_ping=True)
        self.validator = SebiFormatValidator()
    
    def get_pending_documents(self, doc_type: Optional[str] = None) -> List[Dict]:
        """Get documents pending parsing."""
        with self.engine.connect() as conn:
            query = """
                SELECT id, amc_id, scheme_id, document_type,
                       document_date, local_file_path, file_format
                FROM statutory_document_downloads
                WHERE download_status = 'DOWNLOADED'
                  AND parsing_status IN ('PENDING', 'FAILED')
                  AND local_file_path IS NOT NULL
            """
            
            if doc_type:
                query += f" AND document_type = '{doc_type}'"
            
            query += " ORDER BY document_date DESC LIMIT 100"
            
            result = conn.execute(text(query))
            return [dict(row._mapping) for row in result.fetchall()]
    
    def detect_document_format(self, pdf_path: Path) -> str:
        """Identify SEBI format type from document content."""
        try:
            text = self.extract_text_from_pdf(pdf_path)
            text_lower = text.lower()
            
            # Portfolio detection
            if 'monthly portfolio' in text_lower or 'portfolio disclosure' in text_lower:
                if 'half' in text_lower:
                    return 'SEBI_PORTFOLIO_HALFYEARLY'
                return 'SEBI_PORTFOLIO_MONTHLY'
            
            # TER detection
            if 'total expense ratio' in text_lower or 'ter' in text_lower:
                if 'regulation 52' in text_lower:
                    return 'SEBI_TER'
                return 'SEBI_TER'
            
            # AUM detection
            if 'aaum' in text_lower or 'average assets under management' in text_lower:
                return 'SEBI_AUM'
            
            # Fact sheet
            if 'fact sheet' in text_lower or 'factsheet' in text_lower:
                return 'FACT_SHEET'
            
            return 'UNKNOWN'
            
        except Exception as e:
            logger.error(f"Format detection failed: {e}")
            return 'UNKNOWN'
    
    def extract_text_from_pdf(self, pdf_path: Path) -> str:
        """Extract all text from PDF."""
        try:
            reader = PdfReader(str(pdf_path))
            text = ""
            for page in reader.pages:
                page_text = page.extract_text()
                if page_text:
                    text += page_text + "\n"
            return text
        except Exception as e:
            logger.error(f"Text extraction failed for {pdf_path}: {e}")
            return ""
    
    def extract_tables_from_pdf(self, pdf_path: Path) -> List[pd.DataFrame]:
        """Extract tables using pdfplumber."""
        tables = []
        
        try:
            with pdfplumber.open(str(pdf_path)) as pdf:
                for page in pdf.pages:
                    page_tables = page.extract_tables()
                    for table in page_tables:
                        if not table or len(table) < 2:
                            continue
                        
                        # Convert to DataFrame
                        try:
                            df = pd.DataFrame(table[1:], columns=table[0])
                            df.columns = [str(col).strip() for col in df.columns]
                            tables.append(df)
                        except Exception as e:
                            logger.debug(f"Table conversion failed: {e}")
                            continue
        
        except Exception as e:
            logger.error(f"Table extraction failed for {pdf_path}: {e}")
        
        return tables
    
    # -------------------------------------------------------------------------
    # PORTFOLIO PARSER (SEBI-Enhanced)
    # -------------------------------------------------------------------------
    
    def parse_portfolio_holdings(self, pdf_path: Path, doc_id: str, 
                                 scheme_id: Optional[str], amc_id: str,
                                 as_of_date: Optional[str]) -> Tuple[int, Dict]:
        """
        Parse portfolio with SEBI validation.
        
        Returns:
            (holdings_count, validation_result)
        """
        logger.info(f"📊 Parsing portfolio (SEBI-compliant) from {pdf_path.name}")
        
        # Detect format
        format_type = self.detect_document_format(pdf_path)
        
        # Extract tables
        tables = self.extract_tables_from_pdf(pdf_path)
        
        if not tables:
            logger.warning("⚠️  No tables found")
            return 0, {'is_valid': False, 'errors': ['No tables found']}
        
        holdings = []
        
        # Find portfolio table (largest table with correct columns)
        for table in tables:
            if len(table) < 5:
                continue
            
            col_names_lower = [str(c).lower() for c in table.columns]
            
            # Check for portfolio indicators
            is_portfolio = any([
                'instrument' in ' '.join(col_names_lower),
                'security' in ' '.join(col_names_lower),
                'isin' in ' '.join(col_names_lower)
            ])
            
            if not is_portfolio:
                continue
            
            logger.info(f"✓ Found portfolio table with {len(table)} rows")
            
            # Parse rows
            for idx, row in table.iterrows():
                try:
                    holding = self.parse_holding_row(row, table.columns)
                    
                    if not holding or not holding.get('security_name'):
                        continue
                    
                    # Check for illiquid markers (SEBI requirement)
                    holding['is_illiquid'] = self.detect_illiquid_marker(row)
                    holding['is_non_traded'] = self.detect_non_traded_marker(row)
                    
                    # Save to database
                    self.insert_holding_to_db(
                        doc_id=doc_id,
                        scheme_id=scheme_id,
                        amc_id=amc_id,
                        as_of_date=as_of_date,
                        holding=holding,
                        source_file=str(pdf_path)
                    )
                    
                    holdings.append(holding)
                
                except Exception as e:
                    logger.debug(f"Error parsing row {idx}: {e}")
                    continue
        
        # Validate against SEBI format
        validation = self.validator.validate(holdings, format_type)
        
        logger.info(f"✅ Extracted {len(holdings)} holdings | Valid: {validation['is_valid']}")
        
        return len(holdings), validation
    
    def parse_holding_row(self, row: pd.Series, columns: List[str]) -> Optional[Dict]:
        """Parse single holding row with SEBI fields."""
        col_map = self._map_portfolio_columns(columns)
        
        if 'security_name' not in col_map:
            return None
        
        holding = {}
        
        # Security name (required)
        name = str(row.iloc[col_map['security_name']]).strip()
        if not name or name in ['', 'nan', 'None']:
            return None
        holding['security_name'] = name
        
        # ISIN (required for equity)
        if 'isin' in col_map:
            isin = str(row.iloc[col_map['isin']]).strip().upper()
            if re.match(r'^IN[A-Z0-9]{10}$', isin):
                holding['isin_code'] = isin
        
        # Industry/Rating
        if 'industry' in col_map:
            industry = str(row.iloc[col_map['industry']]).strip()
            if industry and industry != 'nan':
                holding['industry_sector'] = industry
                # Detect rating pattern
                if re.match(r'^[A-D]{1,3}[+-]?$|^AAA|^AA', industry):
                    holding['rating'] = industry
                    holding['is_below_investment_grade'] = industry.startswith('B')
        
        # Quantity
        if 'quantity' in col_map:
            try:
                qty = str(row.iloc[col_map['quantity']]).replace(',', '').strip()
                if qty and qty != 'nan':
                    holding['quantity'] = float(qty)
            except:
                pass
        
        # Market Value (SEBI: in Rs. Lakhs)
        if 'market_value' in col_map:
            try:
                mv = str(row.iloc[col_map['market_value']]).replace(',', '').strip()
                if mv and mv != 'nan':
                    holding['market_value'] = float(mv)
            except:
                pass
        
        # Portfolio % (SEBI: % to Net Assets)
        if 'percentage' in col_map:
            try:
                pct = str(row.iloc[col_map['percentage']]).replace('%', '').strip()
                if pct and pct != 'nan':
                    holding['portfolio_percentage'] = float(pct)
            except:
                pass
        
        return holding
    
    def _map_portfolio_columns(self, columns: List[str]) -> Dict:
        """Map DataFrame columns to portfolio fields."""
        col_map = {}
        
        for i, col in enumerate(columns):
            col_lower = str(col).lower()
            
            if 'instrument' in col_lower or 'security' in col_lower:
                col_map['security_name'] = i
            elif 'isin' in col_lower:
                col_map['isin'] = i
            elif 'industry' in col_lower or 'rating' in col_lower:
                col_map['industry'] = i
            elif 'quantity' in col_lower:
                col_map['quantity'] = i
            elif 'market' in col_lower and 'value' in col_lower:
                col_map['market_value'] = i
            elif '%' in col_lower or 'percent' in col_lower:
                col_map['percentage'] = i
        
        return col_map
    
    def detect_illiquid_marker(self, row: pd.Series) -> bool:
        """Detect if security is marked as illiquid (SEBI requirement)."""
        row_text = ' '.join(row.astype(str).values).lower()
        return 'illiquid' in row_text or '*' in row_text
    
    def detect_non_traded_marker(self, row: pd.Series) -> bool:
        """Detect non-traded securities."""
        row_text = ' '.join(row.astype(str).values).lower()
        return 'non-traded' in row_text or 'unlisted' in row_text
    
    def insert_holding_to_db(self, doc_id: str, scheme_id: Optional[str],
                            amc_id: str, as_of_date: Optional[str],
                            holding: Dict, source_file: str):
        """Insert holding with SEBI-compliant fields."""
        with self.engine.begin() as conn:
            conn.execute(
                text("""
                    INSERT INTO portfolio_holdings_raw (
                        document_id, scheme_id, amc_id, as_of_date,
                        security_name, isin_code, industry_sector, rating,
                        quantity, market_value, portfolio_percentage,
                        is_illiquid, is_non_traded, is_below_investment_grade,
                        extraction_confidence, source_file, created_at
                    ) VALUES (
                        :doc_id, :scheme_id, :amc_id, :as_of_date,
                        :security_name, :isin_code, :industry_sector, :rating,
                        :quantity, :market_value, :portfolio_percentage,
                        :is_illiquid, :is_non_traded, :is_below_investment_grade,
                        0.85, :source_file, now()
                    )
                """),
                {
                    "doc_id": doc_id,
                    "scheme_id": scheme_id,
                    "amc_id": amc_id,
                    "as_of_date": as_of_date,
                    "security_name": holding.get('security_name'),
                    "isin_code": holding.get('isin_code'),
                    "industry_sector": holding.get('industry_sector'),
                    "rating": holding.get('rating'),
                    "quantity": holding.get('quantity'),
                    "market_value": holding.get('market_value'),
                    "portfolio_percentage": holding.get('portfolio_percentage'),
                    "is_illiquid": holding.get('is_illiquid', False),
                    "is_non_traded": holding.get('is_non_traded', False),
                    "is_below_investment_grade": holding.get('is_below_investment_grade', False),
                    "source_file": source_file
                }
            )
    
    # -------------------------------------------------------------------------
    # TER PARSER (SEBI Reg 52 Compliant)
    # -------------------------------------------------------------------------
    
    def parse_ter_document(self, pdf_path: Path, doc_id: str,
                          scheme_id: Optional[str], amc_id: str,
                          as_of_date: Optional[str]) -> Tuple[bool, Dict]:
        """
        Parse TER with SEBI Regulation 52 compliance.
        
        Returns:
            (success, validation_result)
        """
        logger.info(f"💰 Parsing TER (SEBI Reg 52) from {pdf_path.name}")
        
        text = self.extract_text_from_pdf(pdf_path)
        tables = self.extract_tables_from_pdf(pdf_path)
        
        ter_entries = []
        
        # Pattern for TER values
        ter_patterns = [
            r'(?:total\s+)?(?:ter|expense\s+ratio)[:\s]+([0-9.]+)%?',
            r'(?:regular|direct)\s+plan[:\s]+([0-9.]+)%?'
        ]
        
        # Try extracting from text
        for plan_type in ['REGULAR', 'DIRECT']:
            plan_pattern = f'{plan_type.lower()}\\s+plan[:\\s]+([0-9.]+)%?'
            match = re.search(plan_pattern, text.lower())
            
            if match:
                try:
                    ter_value = float(match.group(1))
                    ter_entries.append({
                        'plan_type': plan_type,
                        'total_ter': ter_value
                    })
                except:
                    pass
        
        # Try extracting from tables
        if not ter_entries:
            for table in tables:
                ter_data = self._extract_ter_from_table(table)
                ter_entries.extend(ter_data)
        
        if not ter_entries:
            logger.warning("⚠️  Could not extract TER values")
            return False, {'is_valid': False, 'errors': ['No TER values found']}
        
        # Validate
        validation = self.validator.validate(ter_entries, 'SEBI_TER')
        
        # Save to database
        for ter in ter_entries:
            self._save_ter_to_db(doc_id, scheme_id, amc_id, as_of_date, ter, str(pdf_path))
        
        logger.info(f"✅ Extracted TER for {len(ter_entries)} plans | Valid: {validation['is_valid']}")
        
        return True, validation
    
    def _extract_ter_from_table(self, table: pd.DataFrame) -> List[Dict]:
        """Extract TER from table."""
        ter_data = []
        
        for idx, row in table.iterrows():
            row_text = ' '.join(row.astype(str).values).lower()
            
            if 'regular' in row_text or 'direct' in row_text:
                plan_type = 'REGULAR' if 'regular' in row_text else 'DIRECT'
                
                # Find percentage value in row
                for val in row:
                    val_str = str(val).strip()
                    try:
                        if '%' in val_str:
                            ter_value = float(val_str.replace('%', ''))
                        else:
                            ter_value = float(val_str)
                        
                        if 0 < ter_value < 5:
                            ter_data.append({
                                'plan_type': plan_type,
                                'total_ter': ter_value
                            })
                            break
                    except:
                        continue
        
        return ter_data
    
    def _save_ter_to_db(self, doc_id: str, scheme_id: Optional[str], 
                       amc_id: str, as_of_date: Optional[str],
                       ter: Dict, source_file: str):
        """Save TER to database."""
        with self.engine.begin() as conn:
            conn.execute(
                text("""
                    INSERT INTO ter_data_raw (
                        document_id, scheme_id, amc_id, as_of_date,
                        plan_type, total_ter,
                        source_file, created_at
                    ) VALUES (
                        :doc_id, :scheme_id, :amc_id, :as_of_date,
                        :plan_type, :total_ter,
                        :source_file, now()
                    )
                """),
                {
                    "doc_id": doc_id,
                    "scheme_id": scheme_id,
                    "amc_id": amc_id,
                    "as_of_date": as_of_date,
                    "plan_type": ter.get('plan_type'),
                    "total_ter": ter.get('total_ter'),
                    "source_file": source_file
                }
            )
    
    # -------------------------------------------------------------------------
    # MAIN DISPATCHER
    # -------------------------------------------------------------------------
    
    def parse_document(self, doc_info: Dict) -> Tuple[bool, Dict]:
        """Parse document based on type with SEBI validation."""
        pdf_path = Path(doc_info['local_file_path'])
        
        if not pdf_path.exists():
            logger.error(f"❌ File not found: {pdf_path}")
            return False, {'is_valid': False, 'errors': ['File not found']}
        
        doc_type = doc_info['document_type']
        doc_id = doc_info['id']
        scheme_id = doc_info.get('scheme_id')
        amc_id = doc_info['amc_id']
        as_of_date = doc_info.get('document_date')
        
        success = False
        validation = {'is_valid': False}
        
        try:
            if doc_type == 'PORTFOLIO':
                count, validation = self.parse_portfolio_holdings(
                    pdf_path, doc_id, scheme_id, amc_id, as_of_date
                )
                success = count > 0
            
            elif doc_type == 'TER':
                success, validation = self.parse_ter_document(
                    pdf_path, doc_id, scheme_id, amc_id, as_of_date
                )
            
            else:
                logger.warning(f"⚠️  Unsupported document type: {doc_type}")
        
        except Exception as e:
            logger.error(f"❌ Parsing failed: {e}")
            validation = {'is_valid': False, 'errors': [str(e)]}
        
        # Update document status
        self._update_document_status(doc_id, success, validation)
        
        return success, validation
    
    def _update_document_status(self, doc_id: str, success: bool, validation: Dict):
        """Update parsing status in database."""
        with self.engine.begin() as conn:
            conn.execute(
                text("""
                    UPDATE statutory_document_downloads
                    SET parsing_status = :status,
                        is_sebi_compliant = :is_compliant,
                        sebi_format_type = :format_type,
                        validation_errors = :errors::jsonb,
                        parsed_at = now(),
                        updated_at = now()
                    WHERE id = :doc_id
                """),
                {
                    "doc_id": doc_id,
                    "status": 'SUCCESS' if success else 'FAILED',
                    "is_compliant": validation.get('is_valid', False),
                    "format_type": validation.get('format_type'),
                    "errors": str(validation.get('errors', []))
                }
            )
    
    def run(self, doc_type: Optional[str] = None, limit: int = 100):
        """Parse all pending documents with SEBI validation."""
        logger.info("🚀 Starting PDF parser (SEBI-compliant)...")
        
        pending_docs = self.get_pending_documents(doc_type)
        
        if not pending_docs:
            logger.info("✅ No pending documents")
            return
        
        logger.info(f"📋 Found {len(pending_docs)} documents")
        
        success_count = 0
        compliant_count = 0
        
        for i, doc in enumerate(pending_docs[:limit], 1):
            logger.info(f"[{i}/{min(len(pending_docs), limit)}] Parsing {doc['document_type']}")
            
            success, validation = self.parse_document(doc)
            
            if success:
                success_count += 1
                if validation.get('is_valid'):
                    compliant_count += 1
        
        logger.info(f"""
🎉 Parsing complete!
   Total: {min(len(pending_docs), limit)}
   Success: {success_count}
   SEBI Compliant: {compliant_count}
        """)


def main():
    logging.basicConfig(
        level=logging.INFO,
        format="%(asctime)s [%(levelname)s] %(name)s - %(message)s"
    )
    
    from Etl.utils import engine
    
    parser = StatutoryPdfParser(db_url=str(engine.url))
    parser.run(limit=50)


if __name__ == "__main__":
    main()