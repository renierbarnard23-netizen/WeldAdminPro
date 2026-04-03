import sqlite3

db = sqlite3.connect("weldadmin.db")
cur = db.cursor()
cur.execute("ALTER TABLE customer_projects ADD COLUMN created_at TEXT DEFAULT ''")
db.commit()
db.close()

print("Migration done: added created_at column.")
