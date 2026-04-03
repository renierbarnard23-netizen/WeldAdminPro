import sqlite3, shutil, sys
DB = "weldadmin.db"
BACKUP = DB + ".before_merge_createdat.bak"

# backup file
shutil.copyfile(DB, BACKUP)
print("Backup created:", BACKUP)

conn = sqlite3.connect(DB)
cur = conn.cursor()

# find users table name (case-insensitive)
cur.execute("SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;")
tables = [r[0] for r in cur.fetchall()]
users_table = None
for t in tables:
    if t.lower() == "users":
        users_table = t
        break

if not users_table:
    print("ERROR: users table not found. Tables:", tables)
    sys.exit(1)

print("Operating on table:", users_table)
print("Tables:", ", ".join(tables))

# create users_new without DEFAULT expressions to avoid SQL dialect issues
cur.execute(f"""
CREATE TABLE IF NOT EXISTS users_new (
  id INTEGER PRIMARY KEY,
  username TEXT NOT NULL,
  display_name TEXT,
  email TEXT,
  password_hash TEXT,
  role TEXT,
  created_at TEXT,
  updated_at TEXT
);
""")

# copy rows: prefer existing created_at; else use CreatedAt if non-empty; else datetime('now')
# Use NULLIF to treat empty-string as NULL, then COALESCE to pick first non-null.
copy_sql = f"""
INSERT INTO users_new (id, username, display_name, email, password_hash, role, created_at, updated_at)
SELECT id, username, display_name, email, password_hash, role,
       COALESCE(NULLIF(created_at, ''), NULLIF(CreatedAt, ''), datetime('now')) as created_at,
       updated_at
FROM {users_table};
"""
cur.execute(copy_sql)
conn.commit()
print("Rows copied into users_new:", cur.rowcount)

# rename original for safety, and rename new to users (preserves original name)
cur.execute(f"ALTER TABLE {users_table} RENAME TO {users_table}_old;")
cur.execute(f"ALTER TABLE users_new RENAME TO {users_table};")
conn.commit()
print(f"Renamed {users_table} -> {users_table}_old and users_new -> {users_table}")

# print resulting schema and samples
print("\\nFinal schema (PRAGMA table_info):")
for r in conn.execute(f"PRAGMA table_info({users_table});"):
    print(r)
print("\\nSample rows (rowid, username, created_at):")
for r in conn.execute(f"SELECT rowid, username, created_at FROM {users_table} LIMIT 20;"):
    print(r)

conn.close()
print("\\nDone. If all looks good you can remove the backup table named", users_table + "_old")
