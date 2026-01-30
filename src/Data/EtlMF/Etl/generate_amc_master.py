# generate_amc_master.py
import pandas as pd
from pathlib import Path
from cleaner import normalize_amc_name
from utils import logger

def make_code(name: str) -> str:
    """
    Create 3-letter prefix + counter code.
    """
    base = "".join([c for c in name.upper() if c.isalpha()])[:3]
    if len(base) < 3:
        base = (base + "XXX")[:3]
    return base

def generate(csv_in="amc_master_candidates.csv"):
    df = pd.read_csv(csv_in, dtype=str).fillna("")
    df["amc_norm"] = df["amc_name"].apply(normalize_amc_name)

    unique_amcs = sorted(df["amc_norm"].unique())

    result = []
    prefix_counter = {}

    for name in unique_amcs:
        prefix = make_code(name)
        prefix_counter[prefix] = prefix_counter.get(prefix, 0) + 1
        code = prefix + str(prefix_counter[prefix]).zfill(3)

        result.append({
            "amc_code": code,
            "amc_short_name": name,
            "amc_full_name": name
        })

    output_csv = "amc_master_final.csv"
    pd.DataFrame(result).to_csv(output_csv, index=False)

    # SQL seed creator
    output_sql = "V2__seed_amc_master.sql"
    with open(output_sql, "w") as f:
        f.write("-- Auto-generated AMC master seed\n")
        f.write("CREATE TABLE IF NOT EXISTS amc_master (\n")
        f.write("  amc_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),\n")
        f.write("  amc_code TEXT UNIQUE NOT NULL,\n")
        f.write("  amc_short_name TEXT,\n")
        f.write("  amc_full_name TEXT,\n")
        f.write("  cams_amc_code TEXT,\n")
        f.write("  kfin_amc_code TEXT,\n")
        f.write("  bse_amc_code TEXT,\n")
        f.write("  created_at TIMESTAMP DEFAULT now(),\n")
        f.write("  updated_at TIMESTAMP\n")
        f.write(");\n\n")

        for row in result:
            f.write(
                f"INSERT INTO amc_master (amc_code, amc_short_name, amc_full_name)\n"
                f"VALUES ('{row['amc_code']}', '{row['amc_short_name']}', '{row['amc_full_name']}')\n"
                f"ON CONFLICT (amc_code) DO NOTHING;\n\n"
            )

    logger.info(f"📄 AMC Master generated: {output_csv}, SQL: {output_sql}")


if __name__ == "__main__":
    generate()

