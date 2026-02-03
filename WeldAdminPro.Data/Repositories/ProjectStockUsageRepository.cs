using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Data.Repositories
{
	public class ProjectStockUsageRepository
	{
		private readonly string _connectionString;

		public ProjectStockUsageRepository()
		{
			_connectionString = $"Data Source={DatabasePath.Get()}";
			EnsureSchema();
		}

		// =========================
		// SCHEMA
		// =========================
		private void EnsureSchema()
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();

			cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS ProjectStockUsages (
                    Id TEXT PRIMARY KEY,
                    ProjectId TEXT NOT NULL,
                    StockItemId TEXT NOT NULL,
                    Quantity REAL NOT NULL,
                    IssuedOn TEXT NOT NULL,
                    IssuedBy TEXT,
                    Notes TEXT
                );
            ";
			cmd.ExecuteNonQuery();
		}

		// =========================
		// WRITE
		// =========================
		public void Add(ProjectStockUsage usage)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = @"
                INSERT INTO ProjectStockUsages
                (Id, ProjectId, StockItemId, Quantity, IssuedOn, IssuedBy, Notes)
                VALUES
                ($id, $projectId, $stockItemId, $qty, $issuedOn, $issuedBy, $notes);
            ";

			cmd.Parameters.AddWithValue("$id", usage.Id.ToString());
			cmd.Parameters.AddWithValue("$projectId", usage.ProjectId.ToString());
			cmd.Parameters.AddWithValue("$stockItemId", usage.StockItemId.ToString());
			cmd.Parameters.AddWithValue("$qty", usage.Quantity);
			cmd.Parameters.AddWithValue("$issuedOn", usage.IssuedOn.ToString("o"));
			cmd.Parameters.AddWithValue("$issuedBy", usage.IssuedBy ?? string.Empty);
			cmd.Parameters.AddWithValue("$notes", usage.Notes ?? string.Empty);

			cmd.ExecuteNonQuery();
		}

		// =========================
		// READ (PHASE 9.3)
		// =========================
		public List<ProjectStockUsage> GetByProjectId(Guid projectId)
		{
			var list = new List<ProjectStockUsage>();

			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = @"
                SELECT
                    Id,
                    ProjectId,
                    StockItemId,
                    Quantity,
                    IssuedOn,
                    IssuedBy,
                    Notes
                FROM ProjectStockUsages
                WHERE ProjectId = $projectId
                ORDER BY IssuedOn DESC;
            ";

			cmd.Parameters.AddWithValue("$projectId", projectId.ToString());

			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
				list.Add(new ProjectStockUsage
				{
					Id = Guid.Parse(reader.GetString(0)),
					ProjectId = Guid.Parse(reader.GetString(1)),
					StockItemId = Guid.Parse(reader.GetString(2)),
					Quantity = reader.GetDecimal(3),
					IssuedOn = DateTime.Parse(reader.GetString(4)),
					IssuedBy = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
					Notes = reader.IsDBNull(6) ? string.Empty : reader.GetString(6)
				});
			}

			return list;
		}
	}
}
