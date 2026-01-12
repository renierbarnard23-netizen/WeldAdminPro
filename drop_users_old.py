import sqlite3
conn = sqlite3.connect("weldadmin.db")
cur = conn.cursor()
cur.execute("DROP TABLE IF EXISTS users_old;")
conn.commit()
conn.close()
print("Dropped users_old (if it existed).")
