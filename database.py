"""
database.py
SQLite helpers, migrations and safe column migrations for WeldAdmin Pro.
"""
import sqlite3
from pathlib import Path
from typing import Optional, Dict, Tuple

DB_FILE = "weldadmin.db"

# Base CREATE TABLE statements (idempotent)
REQUIRED_TABLES_SQL = {
    'customers': '''
        CREATE TABLE IF NOT EXISTS customers (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            name TEXT,
            address TEXT,
            contact TEXT,
            phone TEXT,
            reg TEXT,
            vat TEXT,
            created_at TEXT
        )
    ''',
    'representatives': '''
        CREATE TABLE IF NOT EXISTS representatives (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            customer_id INTEGER,
            name TEXT,
            phone TEXT,
            email TEXT
        )
    ''',
    'customer_projects': '''
        CREATE TABLE IF NOT EXISTS customer_projects (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            customer_id INTEGER,
            job_number TEXT,
            client_name TEXT DEFAULT '',
            amount TEXT DEFAULT '',
            quote_number TEXT DEFAULT '',
            description TEXT DEFAULT '',
            order_number TEXT DEFAULT '',
            invoice_number TEXT DEFAULT '',
            invoiced INTEGER DEFAULT 0,
            status TEXT DEFAULT 'Open',
            req_wps INTEGER DEFAULT 0,
            req_pqr INTEGER DEFAULT 0,
            req_wpqr INTEGER DEFAULT 0,
            created_at TEXT
        )
    ''',
    'welding_documents': '''
        CREATE TABLE IF NOT EXISTS welding_documents (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            doc_type TEXT,
            name TEXT,
            file_path TEXT DEFAULT ''
        )
    ''',
    'job_welding_links': '''
        CREATE TABLE IF NOT EXISTS job_welding_links (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            job_id INTEGER,
            welding_doc_id INTEGER
        )
    ''',
    'job_history': '''
        CREATE TABLE IF NOT EXISTS job_history (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            job_id INTEGER,
            event_time TEXT,
            event_type TEXT,
            details TEXT
        )
    ''',
    'iso_documents': '''
        CREATE TABLE IF NOT EXISTS iso_documents (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            iso_type TEXT,
            name TEXT,
            revision TEXT,
            doc_date TEXT,
            status TEXT
        )
    ''',
    'stock_items': '''
        CREATE TABLE IF NOT EXISTS stock_items (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            name TEXT,
            qty INTEGER DEFAULT 0
        )
    ''',
    'machines': '''
        CREATE TABLE IF NOT EXISTS machines (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            name TEXT
        )
    ''',
}

# Per-table required columns (name -> (type, default_clause))
REQUIRED_COLUMNS: Dict[str, Dict[str, Tuple[str, str]]] = {
    'customers': {
        # already defined in CREATE, kept for safety if older table existed without created_at
        'created_at': ('TEXT', "DEFAULT ''"),
    },
    'customer_projects': {
        'created_at': ('TEXT', "DEFAULT ''"),
        'client_name': ('TEXT', "DEFAULT ''"),
        'amount': ('TEXT', "DEFAULT ''"),
        'quote_number': ('TEXT', "DEFAULT ''"),
        'description': ('TEXT', "DEFAULT ''"),
        'order_number': ('TEXT', "DEFAULT ''"),
        'invoice_number': ('TEXT', "DEFAULT ''"),
        'invoiced': ('INTEGER', "DEFAULT 0"),
        'status': ('TEXT', "DEFAULT 'Open'"),
        'req_wps': ('INTEGER', "DEFAULT 0"),
        'req_pqr': ('INTEGER', "DEFAULT 0"),
        'req_wpqr': ('INTEGER', "DEFAULT 0"),
    },
    # other tables are created fully in CREATE TABLE; include defensive columns if desired
    'welding_documents': {},
    'job_history': {},
    'iso_documents': {},
    'representatives': {},
    'stock_items': {},
    'machines': {},
    'job_welding_links': {},
}


def get_conn(path: Optional[str] = None) -> sqlite3.Connection:
    path = path or DB_FILE
    return sqlite3.connect(path)


def _table_columns(conn: sqlite3.Connection, table: str):
    """Return set of column names for a given table."""
    cur = conn.cursor()
    cur.execute(f"PRAGMA table_info({table})")
    rows = cur.fetchall()
    # PRAGMA table_info returns rows where name is column 1 (index 1)
    return {r[1] for r in rows}


def _add_column(conn: sqlite3.Connection, table: str, col: str, col_type: str, default_clause: str = ""):
    """Safely add a column to a table."""
    sql = f"ALTER TABLE {table} ADD COLUMN {col} {col_type} {default_clause}"
    cur = conn.cursor()
    cur.execute(sql)


def ensure_migrations(path: Optional[str] = None) -> None:
    """
    Ensure all required tables exist and required columns are present.
    This is safe to call on every startup.
    """
    path = path or DB_FILE
    conn = get_conn(path)
    cur = conn.cursor()

    # 1) Create any missing tables (idempotent)
    for name, create_sql in REQUIRED_TABLES_SQL.items():
        cur.execute(create_sql)

    conn.commit()

    # 2) For each table, check required columns and add any missing ones
    for table, cols in REQUIRED_COLUMNS.items():
        if not cols:
            continue
        existing = _table_columns(conn, table)
        for col_name, (col_type, default_clause) in cols.items():
            if col_name not in existing:
                try:
                    _add_column(conn, table, col_name, col_type, default_clause)
                    print(f"[migrate] Added column {col_name} to {table}")
                except sqlite3.OperationalError as e:
                    # If column exists or cannot be added, print and continue
                    print(f"[migrate] Could not add column {col_name} to {table}: {e}")

    conn.commit()
    conn.close()


def seed_customers(path: Optional[str] = None) -> None:
    """
    Insert default test customers if the customers table is empty.
    """
    path = path or DB_FILE
    conn = get_conn(path)
    cur = conn.cursor()
    try:
        cur.execute('SELECT COUNT(*) FROM customers')
        count = cur.fetchone()[0]
    except Exception:
        count = 0
    if count == 0:
        defaults = [
            ('NCP Chlorchem', '', '', '', '', ''),
            ('AECI Mining', '', '', '', '', ''),
            ('Mixtec', '', '', '', '', ''),
            ('Lodex', '', '', '', '', ''),
            ('ASK Chemicals', '', '', '', '', ''),
        ]
        cur.executemany(
            'INSERT INTO customers (name,address,contact,phone,reg,vat,created_at) VALUES (?, ?, ?, ?, ?, ?, datetime("now"))',
            [(c[0], c[1], c[2], c[3], c[4], c[5]) for c in defaults]
        )
        conn.commit()
        print("[seed] Inserted default customers")
    conn.close()
