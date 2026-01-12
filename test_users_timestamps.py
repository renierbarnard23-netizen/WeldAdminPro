import sqlite3
conn = sqlite3.connect('weldadmin.db')
cur = conn.cursor()

# insert test row
cur.execute("INSERT INTO users (username) VALUES ('ci_test_user');")
conn.commit()
cur.execute("SELECT rowid, username, created_at, updated_at FROM users WHERE username='ci_test_user';")
print('After insert:', cur.fetchone())

# update test row
cur.execute("UPDATE users SET email='ci@test.local' WHERE username='ci_test_user';")
conn.commit()
cur.execute("SELECT rowid, username, created_at, updated_at FROM users WHERE username='ci_test_user';")
print('After update:', cur.fetchone())

# cleanup test row
cur.execute("DELETE FROM users WHERE username='ci_test_user';")
conn.commit()

conn.close()
print('Test complete.')
