def _init_job_history_table(self) -> None:
    cur = self.conn.cursor()
    cur.execute("""
        CREATE TABLE IF NOT EXISTS job_history (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            job_id INTEGER,
            event_time TEXT,
            event_type TEXT,
            details TEXT
        )
    """)
    self.conn.commit()

def _append_job_history(self, job_id: int, event_type: str, details: str = "") -> None:
    import datetime
    cur = self.conn.cursor()
    cur.execute("INSERT INTO job_history (job_id, event_time, event_type, details) VALUES (?, ?, ?, ?)",
                (job_id, datetime.datetime.utcnow().isoformat(), event_type, details))
    self.conn.commit()

def _get_job_history(self, job_id: int):
    cur = self.conn.cursor()
    cur.execute("SELECT event_time, event_type, details FROM job_history WHERE job_id = ? ORDER BY event_time DESC", (job_id,))
    return cur.fetchall()
