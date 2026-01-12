import sqlite3
db = sqlite3.connect("weldadmin.db")
cur = db.cursor()
cur.execute("SELECT id, customer_id, job_number, client_name, amount, quote_number, description, order_number, invoice_number, invoiced, status FROM customer_projects")
for r in cur.fetchall():
    print(r)
db.close()
