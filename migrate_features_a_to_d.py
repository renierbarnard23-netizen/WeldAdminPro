import sqlite3

db = sqlite3.connect("weldadmin.db")
cur = db.cursor()

def ensure_table(sql):
    cur.execute(sql)

# job_history
ensure_table("""
CREATE TABLE IF NOT EXISTS job_history (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    job_id INTEGER,
    event_time TEXT,
    event_type TEXT,
    details TEXT
)
""")

# welding_documents (ensure file_path column exists)
ensure_table("""
CREATE TABLE IF NOT EXISTS welding_documents (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    doc_type TEXT,
    name TEXT,
    file_path TEXT DEFAULT ''
)
""")

# job_welding_links (already created earlier but ensure)
ensure_table("""
CREATE TABLE IF NOT EXISTS job_welding_links (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    job_id INTEGER,
    welding_doc_id INTEGER
)
""")

# Add job-specific columns if missing (some may already exist)
cur.execute("PRAGMA table_info(customer_projects)")
existing = {r[1] for r in cur.fetchall()}

cols_to_add = {
    "req_wps": "INTEGER DEFAULT 0",
    "req_pqr": "INTEGER DEFAULT 0",
    "req_wpqr": "INTEGER DEFAULT 0"
}
for name, definition in cols_to_add.items():
    if name not in existing:
        try:
            cur.execute(f"ALTER TABLE customer_projects ADD COLUMN {name} {definition}")
            print("Added:", name)
        except Exception as e:
            print("Skip", name, e)

db.commit()
db.close()
print("Migration complete.")
