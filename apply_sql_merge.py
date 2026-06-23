import sqlite3
DB="weldadmin.db"
conn=sqlite3.connect(DB)
cur=conn.cursor()
cur.executescript("""
CREATE TABLE IF NOT EXISTS users_new (
  id INTEGER PRIMARY KEY,
  username TEXT NOT NULL,
  display_name TEXT,
  email TEXT,
  password_hash TEXT,
  role TEXT,
  created_at TEXT DEFAULT datetime('now'),
  updated_at TEXT
);
INSERT INTO users_new (id, username, display_name, email, password_hash, role, created_at, updated_at)
SELECT id, username, display_name, email, password_hash, role,
       CASE WHEN created_at IS NULL OR trim(created_at) = '' 
            THEN (CASE WHEN CreatedAt IS NOT NULL AND trim(CreatedAt) != '' THEN CreatedAt ELSE datetime('now') END)
            ELSE created_at END as created_at,
       updated_at
FROM users;
ALTER TABLE users RENAME TO users_old;
ALTER TABLE users_new RENAME TO users;
""")
conn.commit()
for r in conn.execute("PRAGMA table_info(users);"):
    print(r)
print("Sample rows:")
for r in conn.execute("SELECT rowid, username, created_at FROM users LIMIT 20;"):
    print(r)
conn.close()
