import sqlite3

db = sqlite3.connect("weldadmin.db")
cur = db.cursor()

# Required columns for the updated Jobs/Projects system
required_columns = {
    "client_name": "TEXT DEFAULT ''",
    "amount": "TEXT DEFAULT ''",
    "quote_number": "TEXT DEFAULT ''",
    "order_number": "TEXT DEFAULT ''",
    "invoice_number": "TEXT DEFAULT ''",
    "invoiced": "INTEGER DEFAULT 0",
}

# Check existing columns
cur.execute("PRAGMA table_info(customer_projects)")
existing = {col[1] for col in cur.fetchall()}

print("Existing columns:", existing)
print()

# Add missing columns
for col, col_type in required_columns.items():
    if col not in existing:
        try:
            cur.execute(f"ALTER TABLE customer_projects ADD COLUMN {col} {col_type}")
            print(f"Added column: {col}")
        except Exception as e:
            print(f"Error adding {col}: {e}")
    else:
        print(f"Column already exists: {col}")

db.commit()
db.close()

print("\nDone!")
