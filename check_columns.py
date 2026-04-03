import sqlite3

db = sqlite3.connect("weldadmin.db")
cur = db.cursor()

cur.execute("PRAGMA table_info(customer_projects)")
cols = cur.fetchall()

print("Columns in customer_projects:\n")
for c in cols:
    print(f"{c[1]}  ({c[2]})")

db.close()
