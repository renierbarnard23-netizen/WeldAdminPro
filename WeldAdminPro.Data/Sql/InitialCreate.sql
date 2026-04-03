-- InitialCreate.sql
-- Creates Projects, Documents and Wpss tables
-- This script is mainly for SQLite. For SQL Server, adjust types: TEXT->NVARCHAR(MAX), GUID->UNIQUEIDENTIFIER

BEGIN TRANSACTION;

CREATE TABLE IF NOT EXISTS Projects (
  Id TEXT PRIMARY KEY,
  ProjectNumber TEXT NOT NULL UNIQUE,
  Title TEXT NOT NULL,
  Client TEXT NOT NULL,
  StartDate TEXT NULL,
  EndDate TEXT NULL,
  DatabookVersion TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Documents (
  Id TEXT PRIMARY KEY,
  ProjectId TEXT NOT NULL,
  DocType TEXT NOT NULL,
  Title TEXT NOT NULL,
  DocumentNumber TEXT NOT NULL,
  Revision TEXT NOT NULL,
  Status TEXT NOT NULL,
  UploadedAt TEXT NOT NULL,
  UploadedBy TEXT NOT NULL,
  FilePath TEXT NOT NULL,
  FOREIGN KEY(ProjectId) REFERENCES Projects(Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS Wpss (
  Id TEXT PRIMARY KEY,
  ProjectId TEXT NOT NULL,
  WpsNumber TEXT NOT NULL,
  Revision TEXT NOT NULL,
  BaseMaterial TEXT NOT NULL,
  FillerMaterial TEXT NOT NULL,
  Process TEXT NOT NULL,
  Position TEXT NOT NULL,
  Preheat TEXT NOT NULL,
  PostHeat TEXT NOT NULL,
  Author TEXT NOT NULL,
  ApprovedBy TEXT NOT NULL,
  ApprovedAt TEXT NULL,
  FOREIGN KEY(ProjectId) REFERENCES Projects(Id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS IX_Projects_ProjectNumber ON Projects(ProjectNumber);
CREATE INDEX IF NOT EXISTS IX_Documents_ProjectId_DocumentNumber ON Documents(ProjectId, DocumentNumber);
CREATE INDEX IF NOT EXISTS IX_Wpss_ProjectId ON Wpss(ProjectId);

COMMIT;
