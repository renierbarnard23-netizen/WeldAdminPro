import sqlite3, sys
from datetime import datetime

db_path = "weldadmin.db"

def column_exists(cur, table, column):
    cur.execute(f"PRAGMA table_info({table});")
    cols = [r[1].lower() for r in cur.fetchall()]  # r[1] is name
    return column.lower() in cols

try:
    conn = sqlite3.connect(db_path)
    cur = conn.cursor()

    # show tables
    cur.execute("SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;")
    tables = [r[0] for r in cur.fetchall()]
    print("Tables:", ", ".join(tables) or "<none>")

    if "Users" not in tables:
        print("ERROR: 'Users' table not found in the database.")
        sys.exit(1)

    # show current Users schema
    print("\nUsers schema before change:")
    cur.execute("PRAGMA table_info(Users);")
    for r in cur.fetchall():
        # r -> (cid, name, type, notnull, dflt_value, pk)
        print(f"  {r[1]} | {r[2]} | notnull={r[3]} | default={r[4]} | pk={r[5]}")

    # add column only if missing
    if not column_exists(cur, "Users", "CreatedAt"):
        print("\nAdding CreatedAt column...")
        cur.execute("ALTER TABLE Users ADD COLUMN CreatedAt TEXT;")
        conn.commit()
        print("Added CreatedAt.")
    else:
        print("\nCreatedAt column already exists — skipping ALTER TABLE.")

    # populate existing rows that are NULL or empty with current UTC ISO timestamp
    now_iso = datetime.utcnow().strftime("%Y-%m-%d %H:%M:%S")
    cur.execute("UPDATE Users SET CreatedAt = ? WHERE CreatedAt IS NULL OR trim(CreatedAt) = '';", (now_iso,))
    updated = cur.rowcount
    conn.commit()
    print(f"Populated CreatedAt for {updated} rows (UTC {now_iso}).")

    # confirm column exists and show a sample of rows
    print("\nUsers schema after change:")
    cur.execute("PRAGMA table_info(Users);")
    for r in cur.fetchall():
        print(f"  {r[1]} | {r[2]} | notnull={r[3]} | default={r[4]} | pk={r[5]}")

    print("\nSample rows (rowid, username if present, CreatedAt) :")
    cur.execute("PRAGMA table_info(Users);")
    colnames = [r[1] for r in cur.fetchall()]
    if "username" in [c.lower() for c in colnames]:
        cur.execute('SELECT rowid, username, CreatedAt FROM Users LIMIT 20;')
        rows = cur.fetchall()
        for row in rows:
            print(row)
    else:
        cur.execute('SELECT rowid, CreatedAt FROM Users LIMIT 20;')
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
