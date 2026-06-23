import sqlite3, shutil, sys
DB = "weldadmin.db"
BACKUP = DB + ".cleanup.bak"
shutil.copyfile(DB, BACKUP)
print("File backup created:", BACKUP)

conn = sqlite3.connect(DB)
cur = conn.cursor()

def exists(table):
    cur.execute("SELECT name FROM sqlite_master WHERE type='table' AND name=?;", (table,))
    return cur.fetchone() is not None

tables = ['users', 'users_new', 'users_old']
present = [t for t in tables if exists(t)]
print("Present user-related tables:", present)

def info(table):
    cur.execute(f"SELECT COUNT(*) FROM {table};")
    cnt = cur.fetchone()[0]
    print(f"Table {table} count:", cnt)
    cur.execute(f"PRAGMA table_info({table});")
    print(" Schema:", [r for r in cur.fetchall()])
    print(" First 10 rows:")
    cur.execute(f"SELECT rowid, * FROM {table} LIMIT 10;")
    for r in cur.fetchall():
        print("   ", r)
    return cnt

counts = {}
for t in present:
    counts[t] = info(t)
    print("")

# if users_new exists and has same count as users, check id sets
if 'users_new' in present and 'users' in present:
    cur.execute("SELECT id FROM users;")
    ids_users = set(r[0] for r in cur.fetchall())
    cur.execute("SELECT id FROM users_new;")
    ids_new = set(r[0] for r in cur.fetchall())
    print("users ids count:", len(ids_users), "users_new ids count:", len(ids_new))
    if ids_users == ids_new:
        print("ID sets identical. Dropping users_new safely...")
        cur.execute("DROP TABLE IF EXISTS users_new;")
        conn.commit()
        print("Dropped users_new.")
    else:
        extra_in_new = sorted(list(ids_new - ids_users))[:10]
        extra_in_users = sorted(list(ids_users - ids_new))[:10]
        print("ID sets differ. Examples (up to 10):")
        if extra_in_new:
            print(" IDs in users_new but not in users:", extra_in_new)
        if extra_in_users:
            print(" IDs in users but not in users_new:", extra_in_users)
        print("Will NOT drop users_new automatically. Inspect above and decide.")

# Final note
print("\\nCleanup script finished. If you want to remove users_old now, run:")
print("  python - <<'PY'\nimport sqlite3\nconn=sqlite3.connect(\"weldadmin.db\")\ncur=conn.cursor()\ncur.execute(\"DROP TABLE IF EXISTS users_old;\")\nconn.commit()\nconn.close()\nprint('Dropped users_old')\nPY")

conn.close()
