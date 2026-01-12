import sqlite3
conn=sqlite3.connect('weldadmin.db')
conn.execute('VACUUM;')
conn.close()
print('VACUUM complete.')
