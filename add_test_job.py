import sqlite3

db = sqlite3.connect("weldadmin.db")
cur = db.cursor()

cur.execute("""
INSERT INTO customer_projects
(customer_id, job_number, client_name, amount, quote_number, description, order_number, invoice_number, invoiced, status)
VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
""", (
    1,
    "TEST-001",
    "Site Rep",
    "1000",
    "Q-123",
    "Test job",
    "PO-456",
    "INV-789",
    0,
    "Open"
))

db.commit()
db.close()

print("Inserted test job for customer_id=1")
