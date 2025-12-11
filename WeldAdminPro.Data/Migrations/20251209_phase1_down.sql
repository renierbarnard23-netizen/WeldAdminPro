PRAGMA foreign_keys = OFF;
SAVEPOINT sp_phase1_down;

-- drop triggers/tables in reverse order
DROP TRIGGER IF EXISTS trg_attachments_parent_type_check;
DROP TABLE IF EXISTS attachment_parent_types;

DROP INDEX IF EXISTS idx_job_history_actor;
DROP INDEX IF EXISTS idx_job_history_job;
DROP TABLE IF EXISTS job_history;

DROP INDEX IF EXISTS idx_attachments_uploader;
DROP INDEX IF EXISTS idx_attachments_parent;
DROP TABLE IF EXISTS attachments;

DROP TRIGGER IF EXISTS trg_jobs_updated_at;
DROP INDEX IF EXISTS idx_jobs_project;
DROP INDEX IF EXISTS idx_jobs_assigned;
DROP INDEX IF EXISTS idx_jobs_status;
DROP INDEX IF EXISTS ux_jobs_project_jobcode;
DROP TABLE IF EXISTS jobs;

DROP TRIGGER IF EXISTS trg_projects_updated_at;
DROP INDEX IF EXISTS idx_projects_owner;
DROP INDEX IF EXISTS idx_projects_code;
DROP TABLE IF EXISTS projects;

DROP TRIGGER IF EXISTS trg_users_updated_at;
DROP INDEX IF EXISTS idx_users_username;
DROP TABLE IF EXISTS users;

DROP TABLE IF EXISTS job_status_values;

RELEASE sp_phase1_down;
PRAGMA foreign_keys = ON;
