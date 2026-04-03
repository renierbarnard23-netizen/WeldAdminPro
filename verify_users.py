import sqlite3
conn = sqlite3.connect('weldadmin.db')
cur = conn.cursor()

print('Tables:')
for r in cur.execute("SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;"):
    print(' ', r[0])

print('\nusers schema:')
for r in cur.execute("PRAGMA table_info(users);"):
    print(' ', r)

print('\nTriggers:')
for r in cur.execute("SELECT name, sql FROM sqlite_master WHERE type='trigger';"):
    print(' ', r)

print('\nSample rows:')
for r in cur.execute("SELECT rowid, username, created_at, updated_at FROM users LIMIT 10;"):
    print(' ', r)

conn.close()
