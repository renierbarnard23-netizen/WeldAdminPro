-- Phase 1 migration (fixed): Projects, Jobs, Users, Attachments, Job_History (SQLite)
-- Uses SAVEPOINT to avoid "cannot start a transaction within a transaction" errors.

PRAGMA foreign_keys = ON;

-- Use a savepoint so the script can be run whether or not the caller already has a transaction open.
SAVEPOINT sp_phase1_init;

-- ===================================================================
-- users table (simple auth record)
-- ===================================================================
CREATE TABLE IF NOT EXISTS users (
    id          INTEGER PRIMARY KEY AUTOINCREMENT,
    username    TEXT NOT NULL UNIQUE,
    display_name TEXT,
    email       TEXT,
    password_hash TEXT, -- store salted hash; not plain text
    role        TEXT NOT NULL DEFAULT 'user', -- e.g. admin, manager, welder, inspector
    active      INTEGER NOT NULL DEFAULT 1,
    created_at  TEXT NOT NULL DEFAULT (datetime('now')),
    updated_at  TEXT NOT NULL DEFAULT (datetime('now'))
);

CREATE INDEX IF NOT EXISTS idx_users_username ON users(username);

-- trigger: update updated_at on users update
CREATE TRIGGER IF NOT EXISTS trg_users_updated_at
AFTER UPDATE ON users
FOR EACH ROW
WHEN NEW.updated_at = OLD.updated_at
BEGIN
  UPDATE users SET updated_at = datetime('now') WHERE id = NEW.id;
END;

-- ===================================================================
-- projects table
-- ===================================================================
CREATE TABLE IF NOT EXISTS projects (
    id          INTEGER PRIMARY KEY AUTOINCREMENT,
    code        TEXT UNIQUE, -- client/project code e.g. 'PRJ-2025-001'
    name        TEXT NOT NULL,
    client      TEXT,
    description TEXT,
    owner_user_id INTEGER REFERENCES users(id) ON DELETE SET NULL,
    created_at  TEXT NOT NULL DEFAULT (datetime('now')),
    updated_at  TEXT NOT NULL DEFAULT (datetime('now'))
);

CREATE INDEX IF NOT EXISTS idx_projects_code ON projects(code);
CREATE INDEX IF NOT EXISTS idx_projects_owner ON projects(owner_user_id);

CREATE TRIGGER IF NOT EXISTS trg_projects_updated_at
AFTER UPDATE ON projects
FOR EACH ROW
WHEN NEW.updated_at = OLD.updated_at
BEGIN
  UPDATE projects SET updated_at = datetime('now') WHERE id = NEW.id;
END;

-- ===================================================================
-- jobs table
-- ===================================================================
CREATE TABLE IF NOT EXISTS jobs (
    id              INTEGER PRIMARY KEY AUTOINCREMENT,
    project_id      INTEGER NOT NULL REFERENCES projects(id) ON DELETE CASCADE,
    job_code        TEXT NOT NULL, -- e.g. JOB-0001
    title           TEXT NOT NULL,
    description     TEXT,
    status          TEXT NOT NULL DEFAULT 'Draft', -- Draft, Planned, InProgress, Completed, Closed
    priority        INTEGER DEFAULT 0,
    assigned_user_id INTEGER REFERENCES users(id) ON DELETE SET NULL,
    estimated_cost  REAL DEFAULT 0.0,
    start_date      TEXT, -- ISO date strings: 'YYYY-MM-DD'
    due_date        TEXT,
    revision_number INTEGER DEFAULT 1,
    created_at      TEXT NOT NULL DEFAULT (datetime('now')),
    updated_at      TEXT NOT NULL DEFAULT (datetime('now'))
);

-- Composite unique: job_code within project (optional)
CREATE UNIQUE INDEX IF NOT EXISTS ux_jobs_project_jobcode ON jobs(project_id, job_code);

CREATE INDEX IF NOT EXISTS idx_jobs_status ON jobs(status);
CREATE INDEX IF NOT EXISTS idx_jobs_assigned ON jobs(assigned_user_id);
CREATE INDEX IF NOT EXISTS idx_jobs_project ON jobs(project_id);

CREATE TRIGGER IF NOT EXISTS trg_jobs_updated_at
AFTER UPDATE ON jobs
FOR EACH ROW
WHEN NEW.updated_at = OLD.updated_at
BEGIN
  UPDATE jobs SET updated_at = datetime('now') WHERE id = NEW.id;
END;

-- Constrain allowed status values (lookup table)
CREATE TABLE IF NOT EXISTS job_status_values (val TEXT PRIMARY KEY);
INSERT OR IGNORE INTO job_status_values (val) VALUES ('Draft'), ('Planned'), ('InProgress'), ('Completed'), ('Closed');

-- ===================================================================
-- attachments table
-- ===================================================================
CREATE TABLE IF NOT EXISTS attachments (
    id              INTEGER PRIMARY KEY AUTOINCREMENT,
    parent_type     TEXT NOT NULL, -- 'project' or 'job'
    parent_id       INTEGER NOT NULL, -- FK depends on parent_type (enforce in app)
    filename        TEXT NOT NULL,
    content_type    TEXT,
    storage_path    TEXT NOT NULL, -- relative path on disk or object key
    uploaded_by     INTEGER REFERENCES users(id) ON DELETE SET NULL,
    uploaded_at     TEXT NOT NULL DEFAULT (datetime('now')),
    size_bytes      INTEGER DEFAULT 0,
    notes           TEXT
);

CREATE INDEX IF NOT EXISTS idx_attachments_parent ON attachments(parent_type, parent_id);
CREATE INDEX IF NOT EXISTS idx_attachments_uploader ON attachments(uploaded_by);

CREATE TABLE IF NOT EXISTS attachment_parent_types (val TEXT PRIMARY KEY);
INSERT OR IGNORE INTO attachment_parent_types (val) VALUES ('project'), ('job');

CREATE TRIGGER IF NOT EXISTS trg_attachments_parent_type_check
BEFORE INSERT ON attachments
FOR EACH ROW
BEGIN
    SELECT
        CASE
            WHEN (SELECT COUNT(1) FROM attachment_parent_types WHERE val = NEW.parent_type) = 0
            THEN RAISE(ABORT, 'Invalid attachments.parent_type value')
        END;
END;

-- ===================================================================
-- job_history audit table
-- ===================================================================
CREATE TABLE IF NOT EXISTS job_history (
    id          INTEGER PRIMARY KEY AUTOINCREMENT,
    job_id      INTEGER NOT NULL REFERENCES jobs(id) ON DELETE CASCADE,
    action      TEXT NOT NULL, -- e.g. "create", "update", "status_change", "attachment_added"
    actor_user_id INTEGER REFERENCES users(id) ON DELETE SET NULL,
    details     TEXT, -- JSON string with custom details
    created_at  TEXT NOT NULL DEFAULT (datetime('now'))
);

CREATE INDEX IF NOT EXISTS idx_job_history_job ON job_history(job_id);
CREATE INDEX IF NOT EXISTS idx_job_history_actor ON job_history(actor_user_id);

-- ===================================================================
-- Example seed: admin user (password hashing to be implemented in app)
-- ===================================================================
INSERT OR IGNORE INTO users (username, display_name, email, password_hash, role, active)
VALUES ('admin', 'Administrator', 'admin@example.com', '<replace-with-hash>', 'admin', 1);

-- Release the savepoint (commit the work done in this script)
RELEASE sp_phase1_init;
