"""
weldadmin_import_to_db.py
-------------------------
SQLite integration for WeldAdmin Pro.

Uses weldadmin_auto_map.parse_pdf_to_model(...) to:
  - detect document type (WPS / PQR / WPQ)
  - build a clean record dict aligned with GUI tabs
  - insert or replace into SQLite tables: wps, pqr, wpq
"""

import os
import sqlite3
from typing import Dict, Any

from weldadmin_auto_map import parse_pdf_to_model


DB_PATH = "weldadmin.db"  # change if you want a different location/name

# -------------------------------------------------
# Column order (excluding primary key "id")
# These must match your GUI / auto_map fields
# -------------------------------------------------

WPS_COL_ORDER = [
    "doc_type",
    "company_name",
    "designation",
    "wps_number",
    "wps_rev",
    "wps_date",
    "pqr_number",
    "pqr_rev",
    "pqr_date",
    "code_standard",
    "construction_code",
    "thickness_range_text",
    "thickness_min_mm",
    "thickness_max_mm",
    "outside_diameter_range_text",
    "joint_type",
    "joint_design",
    "surface_prep",
    "groove_angle",
    "root_face_mm",
    "root_gap_mm",
    "max_misalignment_mm",
    "back_gouging",
    "backing",
    "backing_type",
    "process",
    "process_type",
    "shielding_gas",
    "backing_gas",
    "preheat_min_c",
    "interpass_max_c",
    "amps_range",
    "volts_range",
    "travel_speed_range_mm_min",
    "max_heat_input_kj_mm",
    "base_material_1_spec",
    "base_material_2_spec",
]

PQR_COL_ORDER = [
    "doc_type",
    "pqr_number",
    "wps_number",
    "code_standard",
    "pqr_date",
    "wps_date",
    "process",
    "position",
    "joint_type",
    "base_material_spec",
    "base_material_thickness_mm",
    "welder_name",
    "welder_id",
    "stamp_number",
    "test_lab",
    "test_report_no",
]

WPQ_COL_ORDER = [
    "doc_type",
    "wpq_record_no",
    "certificate_no",
    "welder_name",
    "welder_id",
    "qualified_to",
    "stamp_number",
    "wps_number",
    "process",
    "position",
    "base_material_spec",
    "test_date",
    "date_issued",
    "job_knowledge",
]

# -------------------------------------------------
# Column types for schema creation / upgrade
# -------------------------------------------------

WPS_COL_TYPES = {
    "id": "INTEGER PRIMARY KEY AUTOINCREMENT",
    "doc_type": "TEXT",
    "company_name": "TEXT",
    "designation": "TEXT",
    "wps_number": "TEXT UNIQUE",
    "wps_rev": "TEXT",
    "wps_date": "TEXT",
    "pqr_number": "TEXT",
    "pqr_rev": "TEXT",
    "pqr_date": "TEXT",
    "code_standard": "TEXT",
    "construction_code": "TEXT",
    "thickness_range_text": "TEXT",
    "thickness_min_mm": "REAL",
    "thickness_max_mm": "REAL",
    "outside_diameter_range_text": "TEXT",
    "joint_type": "TEXT",
    "joint_design": "TEXT",
    "surface_prep": "TEXT",
    "groove_angle": "TEXT",
    "root_face_mm": "TEXT",
    "root_gap_mm": "TEXT",
    "max_misalignment_mm": "TEXT",
    "back_gouging": "TEXT",
    "backing": "TEXT",
    "backing_type": "TEXT",
    "process": "TEXT",
    "process_type": "TEXT",
    "shielding_gas": "TEXT",
    "backing_gas": "TEXT",
    "preheat_min_c": "TEXT",
    "interpass_max_c": "TEXT",
    "amps_range": "TEXT",
    "volts_range": "TEXT",
    "travel_speed_range_mm_min": "TEXT",
    "max_heat_input_kj_mm": "TEXT",
    "base_material_1_spec": "TEXT",
    "base_material_2_spec": "TEXT",
}

PQR_COL_TYPES = {
    "id": "INTEGER PRIMARY KEY AUTOINCREMENT",
    "doc_type": "TEXT",
    "pqr_number": "TEXT UNIQUE",
    "wps_number": "TEXT",
    "code_standard": "TEXT",
    "pqr_date": "TEXT",
    "wps_date": "TEXT",
    "process": "TEXT",
    "position": "TEXT",
    "joint_type": "TEXT",
    "base_material_spec": "TEXT",
    "base_material_thickness_mm": "REAL",
    "welder_name": "TEXT",
    "welder_id": "TEXT",
    "stamp_number": "TEXT",
    "test_lab": "TEXT",
    "test_report_no": "TEXT",
}

WPQ_COL_TYPES = {
    "id": "INTEGER PRIMARY KEY AUTOINCREMENT",
    "doc_type": "TEXT",
    "wpq_record_no": "TEXT UNIQUE",
    "certificate_no": "TEXT",
    "welder_name": "TEXT",
    "welder_id": "TEXT",
    "qualified_to": "TEXT",
    "stamp_number": "TEXT",
    "wps_number": "TEXT",
    "process": "TEXT",
    "position": "TEXT",
    "base_material_spec": "TEXT",
    "test_date": "TEXT",
    "date_issued": "TEXT",
    "job_knowledge": "TEXT",
}

# ---------- Helpers for schema management ----------


def _get_existing_columns(cur: sqlite3.Cursor, table: str) -> set:
    cur.execute(f"PRAGMA table_info({table})")
    return {row[1] for row in cur.fetchall()}


def _ensure_table_columns(cur: sqlite3.Cursor, table: str, columns: Dict[str, str]) -> None:
    """
    Ensure the given table has at least the listed columns.
    If the table doesn't exist, it will be created with all columns.
    If it exists but is missing columns, they will be added via ALTER TABLE.
    """
    cur.execute(
        "SELECT name FROM sqlite_master WHERE type='table' AND name=?",
        (table,),
    )
    row = cur.fetchone()

    if not row:
        # Table does not exist – create it with the full schema
        cols_def = ", ".join(f"{name} {ctype}" for name, ctype in columns.items())
        cur.execute(f"CREATE TABLE {table} ({cols_def})")
        return

    # Table exists – add any missing columns
    existing = _get_existing_columns(cur, table)
    for name, ctype in columns.items():
        if name not in existing:
            cur.execute(f"ALTER TABLE {table} ADD COLUMN {name} {ctype}")


# ---------- Table setup ----------


def ensure_tables() -> None:
    """Create or upgrade WPS / PQR / WPQ tables as needed."""
    conn = sqlite3.connect(DB_PATH)
    cur = conn.cursor()

    _ensure_table_columns(cur, "wps", WPS_COL_TYPES)
    _ensure_table_columns(cur, "pqr", PQR_COL_TYPES)
    _ensure_table_columns(cur, "wpq", WPQ_COL_TYPES)

    conn.commit()
    conn.close()


# ---------- Import logic ----------


def _insert_generic(
    cur: sqlite3.Cursor,
    table: str,
    col_order: list[str],
    rec: Dict[str, Any],
) -> None:
    """
    Generic helper: builds an INSERT OR REPLACE for the given table,
    using the given column order and record dict.
    """
    cols_sql = ", ".join(col_order)
    placeholders = ", ".join(["?"] * len(col_order))

    # For numeric thickness fields we allow None; for others default to ""
    defaults_numeric = {"thickness_min_mm", "thickness_max_mm", "base_material_thickness_mm"}
    values = []
    for c in col_order:
        if c in defaults_numeric:
            values.append(rec.get(c, None))
        else:
            values.append(rec.get(c, ""))

    sql = f"""
        INSERT OR REPLACE INTO {table} (
            {cols_sql}
        ) VALUES (
            {placeholders}
        )
    """
    cur.execute(sql, values)


def import_pdf_to_db(pdf_path: str) -> Dict[str, Any]:
    """
    Parse the given PDF and insert/update the corresponding record
    into the appropriate table (wps / pqr / wpq).

    Returns the model dict:
        { "table": str | None, "record": dict }
    """
    model = parse_pdf_to_model(pdf_path)
    table = model["table"]
    rec = model["record"]

    if table is None:
        print("⚠️ Unknown or unsupported document type. Not saving to DB.")
        return model

    conn = sqlite3.connect(DB_PATH)
    cur = conn.cursor()

    if table == "wps":
        _insert_generic(cur, "wps", WPS_COL_ORDER, rec)
    elif table == "pqr":
        _insert_generic(cur, "pqr", PQR_COL_ORDER, rec)
    elif table == "wpq":
        _insert_generic(cur, "wpq", WPQ_COL_ORDER, rec)

    conn.commit()
    conn.close()

    print(f"✅ Imported '{pdf_path}' as {table.upper()} into '{DB_PATH}'.")
    return model


# ---------- CLI entry point ----------


if __name__ == "__main__":
    ensure_tables()
    pdf_path = input("Enter PDF to import to DB: ").strip()
    if not pdf_path:
        print("No path entered. Exiting.")
    elif not os.path.isfile(pdf_path):
        print(f"File not found: {pdf_path}")
    else:
        model = import_pdf_to_db(pdf_path)
        print(f"Detected table: {model['table']}")
        print("----- RECORD SAVED -----")
        for k, v in model["record"].items():
            print(f"{k}: {v}")

    input("\nDone. Press Enter to close...")
