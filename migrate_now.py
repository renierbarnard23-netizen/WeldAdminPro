# ===== migrate_now.py =====
# Save this as migrate_now.py

import database

if __name__ == "__main__":
    print("Running migrations...")
    database.ensure_migrations()
    print("Seeding default customers (if empty)...")
    database.seed_customers()
    print("Migrations + seeding complete.")
