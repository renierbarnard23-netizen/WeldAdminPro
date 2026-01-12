PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS users (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    username TEXT NOT NULL UNIQUE,
    display_name TEXT,
    email TEXT,
    password_hash TEXT,
    role TEXT,
    created_at TEXT DEFAULT (datetime('now')),
    updated_at TEXT
);

CREATE TABLE IF NOT EXISTS projects (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    code TEXT NOT NULL UNIQUE,
    name TEXT NOT NULL,
    client TEXT,
    created_at TEXT DEFAULT (datetime('now')),
    updated_at TEXT
);

CREATE TABLE IF NOT EXISTS jobs (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    job_code TEXT NOT NULL UNIQUE,
    title TEXT NOT NULL,
    description TEXT,
    project_id INTEGER,
    status TEXT NOT NULL DEFAULT 'Open',
    priority INTEGER DEFAULT 0,
    assigned_user_id INTEGER,
    due_date TEXT,
    created_at TEXT DEFAULT (datetime('now')),
    updated_at TEXT,
    FOREIGN KEY(project_id) REFERENCES projects(id) ON DELETE SET NULL,
    FOREIGN KEY(assigned_user_id) REFERENCES users(id) ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS job_history (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    job_id INTEGER NOT NULL,
    action TEXT NOT NULL,
    details TEXT,
    actor_user_id INTEGER,
    created_at TEXT DEFAULT (datetime('now')),
    FOREIGN KEY(job_id) REFERENCES jobs(id) ON DELETE CASCADE,
    FOREIGN KEY(actor_user_id) REFERENCES users(id) ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS attachments (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    job_id INTEGER,
    project_id INTEGER,
    filename TEXT NOT NULL,
    file_path TEXT NOT NULL,
    content_type TEXT,
    uploaded_by INTEGER,
    uploaded_at TEXT DEFAULT (datetime('now')),
    FOREIGN KEY(job_id) REFERENCES jobs(id) ON DELETE CASCADE,
    FOREIGN KEY(project_id) REFERENCES projects(id) ON DELETE CASCADE,
    FOREIGN KEY(uploaded_by) REFERENCES users(id) ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS idx_jobs_project_id ON jobs(project_id);
CREATE INDEX IF NOT EXISTS idx_jobs_assigned_user ON jobs(assigned_user_id);
CREATE INDEX IF NOT EXISTS idx_job_history_job_id ON job_history(job_id);
CREATE INDEX IF NOT EXISTS idx_attachments_job_id ON attachments(job_id);
CREATE INDEX IF NOT EXISTS idx_attachments_project_id ON attachments(project_id);
