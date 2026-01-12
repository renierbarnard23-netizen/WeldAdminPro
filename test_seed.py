"""Small test to insert a test job for customer id 1."""
import database

if __name__ == '__main__':
    database.ensure_migrations()
    database.seed_customers()
    conn = database.get_conn()
    cur = conn.cursor()
    cur.execute('INSERT INTO customer_projects (customer_id, job_number, client_name, amount, quote_number, description, order_number, invoice_number, invoiced, created_at) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, datetime("now"))', (1, 'TEST-001', 'Site Rep', '1000', 'Q-123', 'Test job', 'PO-456', 'INV-789', 0))
    conn.commit(); conn.close()
    print('Inserted test job for customer_id=1')
