import sqlite3, sys
from datetime import datetime

db_path = "weldadmin.db"
target_table_lower = "users"   # case-insensitive match target

def find_table_name(cur, target_lower):
    cur.execute("SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;")
    tables = [r[0] for r in cur.fetchall()]
    for t in tables:
        if t.lower() == target_lower:
            return t
    return None

def column_exists(cur, table, column):
    cur.execute(f"PRAGMA table_info({table});")
    cols = [r[1].lower() for r in cur.fetchall()]  # r[1] is name
    return column.lower() in cols

try:
    conn = sqlite3.connect(db_path)
    cur = conn.cursor()

    # find the actual users table name (case-insensitive)
    table_name = find_table_name(cur, target_table_lower)
    if not table_name:
        print("ERROR: table matching 'users' not found in the database.")
        sys.exit(1)

    print(f"Using table name: {table_name}")

    # show tables
    cur.execute("SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;")
    tables = [r[0] for r in cur.fetchall()]
    print("Tables:", ", ".join(tables) or "<none>")

    # show current Users schema
    print("\nSchema before change:")
    cur.execute(f"PRAGMA table_info({table_name});")
    for r in cur.fetchall():
        print(f"  {r[1]} | {r[2]} | notnull={r[3]} | default={r[4]} | pk={r[5]}")

    # add column only if missing
    if not column_exists(cur, table_name, "CreatedAt"):
        print("\nAdding CreatedAt column...")
        cur.execute(f"ALTER TABLE {table_name} ADD COLUMN CreatedAt TEXT;")
        conn.commit()
        print("Added CreatedAt.")
    else:
        print("\nCreatedAt column already exists — skipping ALTER TABLE.")

    # populate existing rows that are NULL or empty with current UTC ISO timestamp
    now_iso = datetime.utcnow().strftime("%Y-%m-%d %H:%M:%S")
    cur.execute(f"UPDATE {table_name} SET CreatedAt = ? WHERE CreatedAt IS NULL OR trim(CreatedAt) = '';", (now_iso,))
    updated = cur.rowcount
    conn.commit()
    print(f"Populated CreatedAt for {updated} rows (UTC {now_iso}).")

    # confirm column exists and show a sample of rows
    print("\nSchema after change:")
    cur.execute(f"PRAGMA table_info({table_name});")
    for r in cur.fetchall():
        print(f"  {r[1]} | {r[2]} | notnull={r[3]} | default={r[4]} | pk={r[5]}")

    print("\nSample rows (rowid, username if present, CreatedAt) :")
    cur.execute(f"PRAGMA table_info({table_name});")
    colnames = [r[1] for r in cur.fetchall()]
    if "username" in [c.lower() for c in colnames]:
        cur.execute(f'SELECT rowid, username, CreatedAt FROM {table_name} LIMIT 20;')
        rows = cur.fetchall()
        for row in rows:
            print(row)
    else:
        cur.execute(f'SELECT rowid, CreatedAt FROM {table_name} LIMIT 20;')
        rows = cur.fetchall()
        for row in rows:
            print(row)

except sqlite3.Error as e:
    print("SQLite error:", e)
    sys.exit(1)
finally:
    try:
        conn.close()
    except:
        pass
