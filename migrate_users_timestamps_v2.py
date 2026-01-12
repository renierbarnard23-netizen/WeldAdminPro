import sqlite3, shutil, sys
DB = "weldadmin.db"
BACKUP = DB + ".migrate_timestamps.v2.bak"

# backup DB file
shutil.copyfile(DB, BACKUP)
print("File backup created:", BACKUP)

conn = sqlite3.connect(DB)
cur = conn.cursor()

# find users table name (case-insensitive)
cur.execute("SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;")
tables = [r[0] for r in cur.fetchall()]
users_table = next((t for t in tables if t.lower() == "users"), None)
if not users_table:
    print("ERROR: users table not found. Tables:", tables)
    sys.exit(1)
print("Operating on table:", users_table)

# detect whether CreatedAt exists
cur.execute(f"PRAGMA table_info({users_table});")
cols = [r[1] for r in cur.fetchall()]
has_CreatedAt = "CreatedAt" in cols
print("Columns detected:", cols)
print("Has CreatedAt column:", has_CreatedAt)

# create users_new with DEFAULT on created_at
cur.executescript(f"""
CREATE TABLE IF NOT EXISTS users_new (
  id INTEGER PRIMARY KEY,
  username TEXT NOT NULL,
  display_name TEXT,
  email TEXT,
  password_hash TEXT,
  role TEXT,
  created_at TEXT DEFAULT (datetime('now')),
  updated_at TEXT
);
""")

# build copy SQL depending on whether CreatedAt exists
if has_CreatedAt:
    copy_sql = f"""
    INSERT INTO users_new (id, username, display_name, email, password_hash, role, created_at, updated_at)
    SELECT id, username, display_name, email, password_hash, role,
           COALESCE(NULLIF(created_at, ''), NULLIF(CreatedAt, ''), datetime('now')) as created_at,
           updated_at
    FROM {users_table};
    """
else:
    copy_sql = f"""
    INSERT INTO users_new (id, username, display_name, email, password_hash, role, created_at, updated_at)
    SELECT id, username, display_name, email, password_hash, role,
           COALESCE(NULLIF(created_at, ''), datetime('now')) as created_at,
           updated_at
    FROM {users_table};
    """

cur.execute(copy_sql)
conn.commit()
print("Rows copied into users_new:", cur.rowcount)

# swap tables
cur.execute(f"ALTER TABLE {users_table} RENAME TO {users_table}_old;")
cur.execute("ALTER TABLE users_new RENAME TO " + users_table + ";")
conn.commit()
print(f"Renamed {users_table} -> {users_table}_old and users_new -> {users_table}")

# create trigger to set updated_at automatically (replace any previous trigger)
cur.executescript(f"""
DROP TRIGGER IF EXISTS users_set_updated_at;
CREATE TRIGGER users_set_updated_at
AFTER UPDATE ON {users_table}
FOR EACH ROW
WHEN (NEW.updated_at IS NULL OR NEW.updated_at = OLD.updated_at)
BEGIN
  UPDATE {users_table} SET updated_at = datetime('now') WHERE id = NEW.id;
END;
""")
conn.commit()
print("Trigger users_set_updated_at created.")

# show final schema and sample rows
print("\\nFinal schema (PRAGMA table_info):")
for r in conn.execute(f"PRAGMA table_info({users_table});"):
    print(r)
print("\\nSample rows (rowid, username, created_at, updated_at):")
for r in conn.execute(f"SELECT rowid, username, created_at, updated_at FROM {users_table} LIMIT 20;"):
    print(r)

conn.close()
print("\\nMigration v2 complete. Original table retained as {0}_old and file backup at {1}.".format(users_table, BACKUP))
