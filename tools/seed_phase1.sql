BEGIN;
-- insert admin user (username admin, display_name Admin User)
INSERT OR IGNORE INTO users (username, display_name, email, password_hash, role, created_at)
VALUES ('admin', 'Admin User', 'admin@example.com', '', 'Admin', datetime('now'));

-- sample project
INSERT OR IGNORE INTO projects (code, name, client, created_at)
VALUES ('PRJ-001', 'Sample Project 1', 'Acme Corp', datetime('now'));

-- sample job linked to project (assign to admin)
INSERT OR IGNORE INTO jobs (job_code, title, description, project_id, status, priority, assigned_user_id, created_at)
VALUES (
  'JOB-001', 
  'Weld pipe spool A-1', 
  'Fit-up and weld spool A-1 per WPS', 
  (SELECT id FROM projects WHERE code='PRJ-001' LIMIT 1),
  'Open',
  1,
  (SELECT id FROM users WHERE username='admin' LIMIT 1),
  datetime('now')
);

COMMIT;
