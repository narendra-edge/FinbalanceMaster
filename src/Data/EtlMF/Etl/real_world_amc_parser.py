"""
real_world_amc_parser.py
------------------------
Production-ready parsers for actual AMC statutory disclosure formats.
Handles: Portfolio Statements, AAUM Reports, TER Disclosures

Based on real samples from Aditya Birla Sun Life and SEBI-compliant formats.
"""

import pandas as pd
import numpy as np
from pathlib import Path
from typing import Dict, List, Optional, Tuple
from datetime import datetime
import re
from sqlalchemy import create_engine, text
import logging

logger = logging.getLogger("mf.etl.real_world_parser")


class PortfolioStatementParser:
    """
    Parses monthly portfolio statements in standard AMC format.
    
    Format expectations:
    - Header: Scheme code, name, and "as on" date
    - Sections: Equity, Debt, Money Market, Others
    - Columns: Instrument Name, ISIN, Industry/Rating, Quantity, Market Value, %
    - Footer: Sub-totals, Grand Total, Notes
    """
    
    def __init__(self, db_url: str):
        self.engine = create_engine(db_url, pool_pre_ping=True)
        self.section_keywords = {
            'EQUITY': ['equity', 'equity & equity related', 'shares'],
            'DEBT': ['debt', 'bonds', 'debentures'],
            'MONEY_MARKET': ['money market', 'treasury', 'commercial paper'],
            'CASH': ['treps', 'reverse repo', 'others', 'cash']
        }
    
    def parse_portfolio_csv(self, file_path: Path, scheme_id: Optional[str] = None) -> Dict:
        """Main entry point for portfolio parsing."""
        logger.info(f"📊 Parsing portfolio from {file_path}")
        
        # Auto-detect encoding
        encodings = ['utf-8', 'cp1252', 'latin1', 'iso-8859-1']
        df = None
        
        for encoding in encodings:
            try:
                if file_path.suffix.lower() == '.csv':
                    df = pd.read_csv(file_path, encoding=encoding, dtype=str)
                else:
                    df = pd.read_excel(file_path, dtype=str)
                break
            except UnicodeDecodeError:
                continue
            except Exception as e:
                logger.error(f"Failed to read {file_path}: {e}")
                return None
        
        if df is None:
            logger.error(f"Could not read file with any encoding")
            return None
        
        # Extract metadata
        scheme_info = self.extract_scheme_metadata(df)
        
        # Extract holdings
        holdings = self.extract_holdings(df, scheme_info)
        
        # Validate
        validation = self.validate_portfolio_data(holdings)
        
        result = {
            'scheme_info': scheme_info,
            'holdings': holdings,
            'validation': validation,
            'summary': {
                'total_holdings': len(holdings),
                'total_market_value_lakhs': sum(h.get('market_value', 0) or 0 for h in holdings),
                'equity_count': len([h for h in holdings if h.get('asset_class') == 'EQUITY']),
                'debt_count': len([h for h in holdings if h.get('asset_class') == 'DEBT']),
                'cash_count': len([h for h in holdings if h.get('asset_class') == 'CASH'])
            }
        }
        
        logger.info(f"✅ Extracted {len(holdings)} holdings | Validation: {validation['is_valid']}")
        return result
    
    def extract_scheme_metadata(self, df: pd.DataFrame) -> Dict:
        """Extract scheme code, name, and date from header rows."""
        metadata = {
            'scheme_code': None,
            'scheme_name': None,
            'as_of_date': None
        }
        
        # Combine first 5 rows into searchable text
        header_text = ' '.join(df.iloc[0:5].fillna('').astype(str).values.flatten())
        
        # Scheme code (3-20 uppercase letters at start)
        scheme_code_match = re.search(r'^([A-Z]{3,20})\s', header_text)
        if scheme_code_match:
            metadata['scheme_code'] = scheme_code_match.group(1)
        
        # Scheme name (after code or containing MF name)
        name_patterns = [
            r'(ADITYA BIRLA SUN LIFE [A-Z\s]+FUND)',
            r'(HDFC [A-Z\s]+FUND)',
            r'(ICICI PRUDENTIAL [A-Z\s]+FUND)',
            r'(SBI [A-Z\s]+FUND)',
            r'(AXIS [A-Z\s]+FUND)'
        ]
        for pattern in name_patterns:
            name_match = re.search(pattern, header_text, re.IGNORECASE)
            if name_match:
                metadata['scheme_name'] = name_match.group(1).strip()
                break
        
        # Date extraction (various formats)
        date_patterns = [
            r'as on\s+(\w+\s+\d{1,2},?\s*\d{4})',
            r'as on\s+(\d{1,2}[-/]\w+[-/]\d{4})',
            r'as on\s+(\d{1,2}[-/]\d{1,2}[-/]\d{4})'
        ]
        for pattern in date_patterns:
            date_match = re.search(pattern, header_text, re.IGNORECASE)
            if date_match:
                date_str = date_match.group(1)
                try:
                    metadata['as_of_date'] = pd.to_datetime(date_str, dayfirst=True).date()
                    break
                except:
                    logger.debug(f"Could not parse date: {date_str}")
        
        return metadata
    
    def extract_holdings(self, df: pd.DataFrame, scheme_info: Dict) -> List[Dict]:
        """Extract security holdings with section tracking."""
        holdings = []
        
        # Find header row
        header_row_idx = None
        for idx, row in df.iterrows():
            row_str = ' '.join(row.fillna('').astype(str).values)
            if re.search(r'name\s+of\s+(the\s+)?instrument', row_str, re.IGNORECASE):
                header_row_idx = idx
                break
        
        if header_row_idx is None:
            logger.error("❌ Could not find 'Name of the Instrument' header")
            return holdings
        
        # Set column names
        df.columns = df.iloc[header_row_idx].fillna('').values
        data_start = header_row_idx + 1
        
        # Map columns
        col_map = self._map_columns(df.columns)
        
        if not col_map['security_name']:
            logger.error("❌ Could not identify security name column")
            return holdings
        
        # Track current section
        current_asset_class = None
        current_sub_category = None
        
        for idx in range(data_start, len(df)):
            row = df.iloc[idx]
            first_col = str(row.iloc[0]).strip()
            
            # Skip empty rows
            if not first_col or first_col == 'nan':
                continue
            
            # Detect section changes
            detected_class = self._detect_asset_class(first_col)
            if detected_class:
                current_asset_class = detected_class
                continue
            
            # Detect sub-categories
            if re.search(r'listed|awaiting\s+listing', first_col, re.IGNORECASE):
                current_sub_category = 'LISTED'
                continue
            elif re.search(r'unlisted', first_col, re.IGNORECASE):
                current_sub_category = 'UNLISTED'
                continue
            
            # Skip totals and notes
            if re.search(r'total|sub-total|grand\s+total|^\^', first_col, re.IGNORECASE):
                if 'grand total' in first_col.lower():
                    break  # Stop at grand total
                continue
            
            # Parse holding
            holding = self._parse_holding_row(row, col_map, current_asset_class, current_sub_category)
            
            if holding and holding.get('security_name'):
                holdings.append(holding)
        
        return holdings
    
    def _map_columns(self, columns: pd.Index) -> Dict:
        """Map DataFrame columns to standard fields."""
        col_map = {
            'security_name': None,
            'isin': None,
            'industry': None,
            'quantity': None,
            'market_value': None,
            'percentage': None
        }
        
        for col in columns:
            col_lower = str(col).lower()
            
            if re.search(r'name.*instrument|instrument.*name', col_lower):
                col_map['security_name'] = col
            elif 'isin' in col_lower:
                col_map['isin'] = col
            elif re.search(r'industry|rating|sector', col_lower):
                col_map['industry'] = col
            elif 'quantity' in col_lower or 'no.' in col_lower:
                col_map['quantity'] = col
            elif re.search(r'market.*value|fair.*value', col_lower):
                col_map['market_value'] = col
            elif '%' in col_lower or 'percent' in col_lower or 'net assets' in col_lower:
                col_map['percentage'] = col
        
        return col_map
    
    def _detect_asset_class(self, text: str) -> Optional[str]:
        """Detect asset class from section header."""
        text_lower = text.lower()
        
        for asset_class, keywords in self.section_keywords.items():
            if any(kw in text_lower for kw in keywords):
                return asset_class
        
        return None
    
    def _parse_holding_row(self, row: pd.Series, col_map: Dict, 
                          asset_class: str, sub_category: str) -> Dict:
        """Parse single holding row."""
        holding = {
            'asset_class': asset_class,
            'listing_status': sub_category
        }
        
        # Security name
        if col_map['security_name']:
            name = str(row[col_map['security_name']]).strip()
            if name and name != 'nan':
                holding['security_name'] = name
        
        # ISIN validation
        if col_map['isin']:
            isin = str(row[col_map['isin']]).strip().upper()
            if isin and isin != 'NAN' and len(isin) == 12 and isin.startswith('IN'):
                holding['isin_code'] = isin
        
        # Industry/Rating
        if col_map['industry']:
            industry = str(row[col_map['industry']]).strip()
            if industry and industry != 'nan':
                holding['industry_sector'] = industry
                # Detect if it's a rating
                if re.match(r'^[A-D]{1,3}[+-]?$|^AAA|^AA|^A$', industry):
                    holding['rating'] = industry
        
        # Quantity (remove commas)
        if col_map['quantity']:
            qty_str = str(row[col_map['quantity']]).replace(',', '').strip()
            try:
                if qty_str and qty_str != 'nan':
                    holding['quantity'] = float(qty_str)
            except ValueError:
                pass
        
        # Market Value in Lakhs
        if col_map['market_value']:
            mv_str = str(row[col_map['market_value']]).replace(',', '').strip()
            try:
                if mv_str and mv_str != 'nan':
                    holding['market_value'] = float(mv_str)
            except ValueError:
                pass
        
        # Percentage (remove %)
        if col_map['percentage']:
            pct_str = str(row[col_map['percentage']]).replace('%', '').strip()
            try:
                if pct_str and pct_str != 'nan':
                    holding['portfolio_percentage'] = float(pct_str)
            except ValueError:
                pass
        
        return holding
    
    def validate_portfolio_data(self, holdings: List[Dict]) -> Dict:
        """Validate portfolio data quality."""
        validation = {
            'is_valid': True,
            'errors': [],
            'warnings': []
        }
        
        if not holdings:
            validation['is_valid'] = False
            validation['errors'].append('No holdings found')
            return validation
        
        # Check ISIN coverage for equity
        equity_holdings = [h for h in holdings if h.get('asset_class') == 'EQUITY']
        if equity_holdings:
            isin_coverage = len([h for h in equity_holdings if h.get('isin_code')]) / len(equity_holdings)
            if isin_coverage < 0.9:
                validation['warnings'].append(f'Low ISIN coverage: {isin_coverage:.1%}')
        
        # Check percentage sum
        total_pct = sum(h.get('portfolio_percentage', 0) for h in holdings)
        if abs(total_pct - 100) > 1.0:
            validation['warnings'].append(f'Portfolio % sums to {total_pct:.2f}%, not 100%')
        
        # Check market values
        holdings_with_value = [h for h in holdings if h.get('market_value')]
        if len(holdings_with_value) < len(holdings) * 0.95:
            validation['warnings'].append('Some holdings missing market value')
        
        return validation
    
    def save_to_database(self, portfolio_data: Dict, scheme_id: str):
        """Save parsed portfolio to database."""
        if not portfolio_data or not portfolio_data.get('holdings'):
            logger.warning("No data to save")
            return
        
        scheme_info = portfolio_data['scheme_info']
        holdings = portfolio_data['holdings']
        as_of_date = scheme_info.get('as_of_date')
        
        if not as_of_date:
            logger.error("Missing as_of_date - cannot save")
            return
        
        saved_count = 0
        
        with self.engine.begin() as conn:
            for holding in holdings:
                try:
                    conn.execute(
                        text("""
                            INSERT INTO portfolio_holdings_raw (
                                scheme_id, as_of_date,
                                security_name, isin_code, 
                                security_type, asset_class, listing_status,
                                industry_sector, rating,
                                quantity, market_value, portfolio_percentage,
                                extraction_confidence, source_file, created_at
                            ) VALUES (
                                :scheme_id, :as_of_date,
                                :security_name, :isin_code,
                                :security_type, :asset_class, :listing_status,
                                :industry_sector, :rating,
                                :quantity, :market_value, :portfolio_percentage,
                                0.95, :source_file, now()
                            )
                        """),
                        {
                            'scheme_id': scheme_id,
                            'as_of_date': as_of_date,
                            'security_name': holding.get('security_name'),
                            'isin_code': holding.get('isin_code'),
                            'security_type': holding.get('asset_class'),
                            'asset_class': holding.get('asset_class'),
                            'listing_status': holding.get('listing_status'),
                            'industry_sector': holding.get('industry_sector'),
                            'rating': holding.get('rating'),
                            'quantity': holding.get('quantity'),
                            'market_value': holding.get('market_value'),
                            'portfolio_percentage': holding.get('portfolio_percentage'),
                            'source_file': str(portfolio_data.get('source_file', ''))
                        }
                    )
                    saved_count += 1
                except Exception as e:
                    logger.error(f"Failed to save holding: {e}")
        
        logger.info(f"💾 Saved {saved_count}/{len(holdings)} holdings to database")


class AaumReportParser:
    """
    Parses Monthly AAUM reports in SEBI grid format.
    
    Format: Multi-level grid with T30/B30, Direct/Distributor, Investor categories.
    """
    
    def __init__(self, db_url: str):
        self.engine = create_engine(db_url, pool_pre_ping=True)
    
    def parse_aaum_csv(self, file_path: Path) -> Dict:
        """Parse AAUM report."""
        logger.info(f"📊 Parsing AAUM from {file_path}")
        
        # Read with auto-encoding
        df = self._read_file(file_path)
        if df is None:
            return None
        
        # Extract metadata
        header_info = self.extract_aaum_header(df)
        
        # Parse grid
        schemes_data = self.parse_aaum_grid(df)
        
        logger.info(f"✅ Extracted {len(schemes_data)} schemes")
        
        return {
            'header_info': header_info,
            'schemes': schemes_data
        }
    
    def _read_file(self, file_path: Path) -> Optional[pd.DataFrame]:
        """Read file with encoding fallback."""
        encodings = ['utf-8', 'cp1252', 'latin1']
        
        for encoding in encodings:
            try:
                if file_path.suffix.lower() == '.csv':
                    return pd.read_csv(file_path, encoding=encoding, dtype=str, header=None)
                else:
                    return pd.read_excel(file_path, dtype=str, header=None)
            except UnicodeDecodeError:
                continue
            except Exception as e:
                logger.error(f"Failed to read: {e}")
                return None
        
        return None
    
    def extract_aaum_header(self, df: pd.DataFrame) -> Dict:
        """Extract AMC name and date."""
        header = {}
        
        first_row = ' '.join(df.iloc[0].fillna('').astype(str).values)
        
        # AMC name
        amc_patterns = [
            r'([\w\s]+Mutual Fund)',
            r'(ADITYA BIRLA[\w\s]+)',
            r'(HDFC[\w\s]+)',
            r'(ICICI[\w\s]+)'
        ]
        for pattern in amc_patterns:
            match = re.search(pattern, first_row, re.IGNORECASE)
            if match:
                header['amc_name'] = match.group(1).strip()
                break
        
        # Date
        date_match = re.search(r'(\w+\s+\d{4}|\d{1,2}[-/]\w+[-/]\d{4})', first_row)
        if date_match:
            try:
                header['as_of_date'] = pd.to_datetime(date_match.group(1)).date()
            except:
                pass
        
        return header
    
    def parse_aaum_grid(self, df: pd.DataFrame) -> List[Dict]:
        """Parse SEBI grid structure."""
        schemes = []
        
        # Find data start
        data_start_idx = None
        for idx, row in df.iterrows():
            if 'Scheme Category' in str(row.iloc[0]):
                data_start_idx = idx
                break
        
        if data_start_idx is None:
            logger.warning("Could not find 'Scheme Category' row")
            return schemes
        
        # Parse rows
        current_category = None
        
        for idx in range(data_start_idx + 2, len(df)):
            row = df.iloc[idx]
            first_col = str(row.iloc[0]).strip()
            
            if not first_col or first_col == 'nan':
                continue
            
            # Category markers
            if re.match(r'^[A-Z]$|^\([ivx]+\)$', first_col):
                current_category = first_col
                continue
            
            # Skip totals
            if 'total' in first_col.lower():
                continue
            
            # Parse scheme row
            scheme_data = self._parse_aaum_scheme_row(row, current_category)
            if scheme_data:
                schemes.append(scheme_data)
        
        return schemes
    
    def _parse_aaum_scheme_row(self, row: pd.Series, category: str) -> Optional[Dict]:
        """Parse single AAUM scheme row."""
        scheme_name = str(row.iloc[0]).strip()
        
        if not scheme_name or scheme_name == 'nan':
            return None
        
        # Grand total (last column)
        try:
            grand_total = float(str(row.iloc[-1]).replace(',', ''))
        except:
            grand_total = 0
        
        # Simplified breakup (full implementation would parse all 60 columns)
        scheme_data = {
            'scheme_name': scheme_name,
            'category': category,
            'total_aum_crores': grand_total,
            'direct_plan_aum': self._sum_columns(row, 1, 20),
            'associate_distributor_aum': self._sum_columns(row, 21, 40),
            'non_associate_distributor_aum': self._sum_columns(row, 41, 60),
            't30_cities_aum': self._sum_t30(row),
            'b30_cities_aum': self._sum_b30(row)
        }
        
        return scheme_data
    
    def _sum_columns(self, row: pd.Series, start: int, end: int) -> float:
        """Sum column range."""
        total = 0
        for i in range(start, min(end + 1, len(row))):
            try:
                val = str(row.iloc[i]).replace(',', '').strip()
                if val and val != 'nan':
                    total += float(val)
            except:
                pass
        return total
    
    def _sum_t30(self, row: pd.Series) -> float:
        """Sum T30 columns."""
        return (self._sum_columns(row, 1, 10) + 
                self._sum_columns(row, 21, 30) + 
                self._sum_columns(row, 41, 50))
    
    def _sum_b30(self, row: pd.Series) -> float:
        """Sum B30 columns."""
        return (self._sum_columns(row, 11, 20) + 
                self._sum_columns(row, 31, 40) + 
                self._sum_columns(row, 51, 60))


class ExpenseRatioParser:
    """Parse TER disclosure files."""
    
    def __init__(self, db_url: str):
        self.engine = create_engine(db_url, pool_pre_ping=True)
    
    def parse_ter_file(self, file_path: Path) -> Dict:
        """Parse TER file."""
        logger.info(f"📊 Parsing TER from {file_path}")
        
        # Read file
        if file_path.suffix.lower() == '.csv':
            df = pd.read_csv(file_path, encoding='utf-8', dtype=str)
        else:
            df = pd.read_excel(file_path, dtype=str)
        
        metadata = self._extract_ter_metadata(df)
        ter_data = self._extract_ter_values(df)
        
        logger.info(f"✅ Extracted TER for {len(ter_data)} plans")
        
        return {
            'metadata': metadata,
            'ter_data': ter_data
        }
    
    def _extract_ter_metadata(self, df: pd.DataFrame) -> Dict:
        """Extract scheme and date."""
        metadata = {}
        
        header_text = ' '.join(df.iloc[0:5].fillna('').astype(str).values.flatten())
        
        # Scheme name
        scheme_match = re.search(r'([\w\s]+FUND)', header_text, re.IGNORECASE)
        if scheme_match:
            metadata['scheme_name'] = scheme_match.group(1)
        
        # Date
        date_match = re.search(r'(\d{1,2}[-/]\w+[-/]\d{4})', header_text)
        if date_match:
            try:
                metadata['as_of_date'] = pd.to_datetime(date_match.group(1)).date()
            except:
                pass
        
        return metadata
    
    def _extract_ter_values(self, df: pd.DataFrame) -> List[Dict]:
        """Extract TER for Regular and Direct plans."""
        ter_data = []
        
        for idx, row in df.iterrows():
            row_text = ' '.join(row.fillna('').astype(str).values)
            
            if 'Regular Plan' in row_text or 'Direct Plan' in row_text:
                plan_type = 'REGULAR' if 'Regular' in row_text else 'DIRECT'
                ter_value = self._extract_percentage(row)
                
                if ter_value:
                    ter_data.append({
                        'plan_type': plan_type,
                        'total_ter': ter_value
                    })
        
        return ter_data
    
    def _extract_percentage(self, row: pd.Series) -> Optional[float]:
        """Extract percentage from row."""
        for val in row:
            val_str = str(val).strip()
            if '%' in val_str:
                try:
                    return float(val_str.replace('%', ''))
                except:
                    pass
            try:
                num = float(val_str)
                if 0 < num < 5:
                    return num
            except:
                pass
        return None


# Main execution
def process_all_files(portfolio_file: Path, aaum_file: Path, ter_file: Path, 
                     scheme_id: str, db_url: str) -> Dict:
    """Process all three file types."""
    logger.info("🚀 Processing real-world AMC files")
    
    results = {}
    
    # Portfolio
    try:
        portfolio_parser = PortfolioStatementParser(db_url)
        portfolio_data = portfolio_parser.parse_portfolio_csv(portfolio_file, scheme_id)
        if portfolio_data:
            portfolio_parser.save_to_database(portfolio_data, scheme_id)
            results['portfolio'] = portfolio_data['summary']
    except Exception as e:
        logger.error(f"Portfolio parsing failed: {e}")
    
    # AAUM
    try:
        aaum_parser = AaumReportParser(db_url)
        aaum_data = aaum_parser.parse_aaum_csv(aaum_file)
        results['aaum'] = {'schemes_count': len(aaum_data.get('schemes', []))}
    except Exception as e:
        logger.error(f"AAUM parsing failed: {e}")
    
    # TER
    try:
        ter_parser = ExpenseRatioParser(db_url)
        ter_data = ter_parser.parse_ter_file(ter_file)
        results['ter'] = {'plans_count': len(ter_data.get('ter_data', []))}
    except Exception as e:
        logger.error(f"TER parsing failed: {e}")
    
    logger.info("🎉 All files processed")
    return results
