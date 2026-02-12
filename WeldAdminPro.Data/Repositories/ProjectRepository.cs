using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Data.Repositories
{
	public class ProjectRepository : IProjectRepository
	{
		private readonly string _connectionString;

		public ProjectRepository()
		{
			_connectionString = $"Data Source={DatabasePath.Get()}";
			EnsureTables();
		}

		private void EnsureTables()
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS Projects (
	Id TEXT PRIMARY KEY,
	JobNumber INTEGER NOT NULL UNIQUE,
	ProjectName TEXT NOT NULL,
	Client TEXT NOT NULL,
	ClientRepresentative TEXT,
	Amount REAL NOT NULL DEFAULT 0, -- legacy (kept for safety)
	QuoteNumber TEXT,
	OrderNumber TEXT,
	Material TEXT,
	AssignedTo TEXT,
	IsInvoiced INTEGER NOT NULL,
	InvoiceNumber TEXT,
	StartDate TEXT,
	EndDate TEXT,
	Status INTEGER NOT NULL,
	CreatedOn TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS ProjectSettings (
	Key TEXT PRIMARY KEY,
	Value TEXT NOT NULL
);";
			cmd.ExecuteNonQuery();

			// ---- SAFE COLUMN MIGRATIONS ----
			AddColumnIfNotExists("Projects", "Budget REAL DEFAULT 0");
			AddColumnIfNotExists("Projects", "ActualCost REAL DEFAULT 0");
			AddColumnIfNotExists("Projects", "CommittedCost REAL DEFAULT 0");
			AddColumnIfNotExists("Projects", "CompletedOn TEXT");
			AddColumnIfNotExists("Projects", "LastModifiedOn TEXT");
			AddColumnIfNotExists("Projects", "IsArchived INTEGER DEFAULT 0");
		}

		private void AddColumnIfNotExists(string table, string columnDef)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = $"PRAGMA table_info({table});";

			using var reader = cmd.ExecuteReader();
			var columnName = columnDef.Split(' ')[0];
			var exists = false;

			while (reader.Read())
			{
				if (reader["name"].ToString() == columnName)
				{
					exists = true;
					break;
				}
			}

			if (!exists)
			{
				using var alterCmd = connection.CreateCommand();
				alterCmd.CommandText = $"ALTER TABLE {table} ADD COLUMN {columnDef};";
				alterCmd.ExecuteNonQuery();
			}
		}

		// ================= READ =================

		public IEnumerable<Project> GetAll()
		{
			var list = new List<Project>();

			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = "SELECT * FROM Projects WHERE IFNULL(IsArchived,0)=0 ORDER BY JobNumber ASC";

			using var reader = cmd.ExecuteReader();
			while (reader.Read())
				list.Add(Map(reader));

			return list;
		}

		public Project? GetById(Guid id)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = "SELECT * FROM Projects WHERE Id = @Id";
			cmd.Parameters.AddWithValue("@Id", id.ToString());

			using var reader = cmd.ExecuteReader();
			return reader.Read() ? Map(reader) : null;
		}

		// ================= CREATE =================

		public void Add(Project project)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var tx = connection.BeginTransaction();

			project.JobNumber = GetNextJobNumberInternal(connection);

			using var cmd = connection.CreateCommand();
			cmd.CommandText = @"
INSERT INTO Projects (
	Id, JobNumber, ProjectName, Client, ClientRepresentative,
	Amount, Budget, QuoteNumber, OrderNumber, Material, AssignedTo,
	IsInvoiced, InvoiceNumber, StartDate, EndDate,
	Status, CreatedOn, ActualCost, CommittedCost, CompletedOn,
	LastModifiedOn, IsArchived
) VALUES (
	@Id, @JobNumber, @ProjectName, @Client, @ClientRepresentative,
	@Amount, @Budget, @QuoteNumber, @OrderNumber, @Material, @AssignedTo,
	@IsInvoiced, @InvoiceNumber, @StartDate, @EndDate,
	@Status, @CreatedOn, @ActualCost, @CommittedCost, @CompletedOn,
	@LastModifiedOn, @IsArchived
);";

			project.LastModifiedOn = DateTime.Now;

			BindParameters(cmd, project);
			cmd.ExecuteNonQuery();

			UpdateNextJobNumber(connection, project.JobNumber + 1);
			tx.Commit();
		}

		// ================= UPDATE =================

		public void Update(Project project)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			project.LastModifiedOn = DateTime.Now;

			using var cmd = connection.CreateCommand();
			cmd.CommandText = @"
UPDATE Projects SET
	ProjectName = @ProjectName,
	Client = @Client,
	ClientRepresentative = @ClientRepresentative,
	Amount = @Amount,
	Budget = @Budget,
	QuoteNumber = @QuoteNumber,
	OrderNumber = @OrderNumber,
	Material = @Material,
	AssignedTo = @AssignedTo,
	IsInvoiced = @IsInvoiced,
	InvoiceNumber = @InvoiceNumber,
	StartDate = @StartDate,
	EndDate = @EndDate,
	Status = @Status,
	ActualCost = @ActualCost,
	CommittedCost = @CommittedCost,
	CompletedOn = @CompletedOn,
	LastModifiedOn = @LastModifiedOn,
	IsArchived = @IsArchived
WHERE Id = @Id;";

			BindParameters(cmd, project);
			cmd.ExecuteNonQuery();
		}

		// ================= ARCHIVE (REPLACES DELETE) =================

		public void Delete(Guid id)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = "UPDATE Projects SET IsArchived = 1 WHERE Id = @Id";
			cmd.Parameters.AddWithValue("@Id", id.ToString());
			cmd.ExecuteNonQuery();
		}

		// ================= JOB NUMBER =================

		public int GetNextJobNumber()
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();
			return GetNextJobNumberInternal(connection);
		}

		public void InitializeJobNumber(int startingNumber)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = @"
INSERT OR IGNORE INTO ProjectSettings (Key, Value)
VALUES ('NextJobNumber', @Value);";
			cmd.Parameters.AddWithValue("@Value", startingNumber.ToString());
			cmd.ExecuteNonQuery();
		}

		private int GetNextJobNumberInternal(SqliteConnection connection)
		{
			using var cmd = connection.CreateCommand();
			cmd.CommandText = "SELECT Value FROM ProjectSettings WHERE Key='NextJobNumber';";

			var result = cmd.ExecuteScalar();
			if (result == null)
			{
				UpdateNextJobNumber(connection, 1001);
				return 1000;
			}

			return int.Parse(result.ToString()!);
		}

		private void UpdateNextJobNumber(SqliteConnection connection, int next)
		{
			using var cmd = connection.CreateCommand();
			cmd.CommandText = "UPDATE ProjectSettings SET Value=@Value WHERE Key='NextJobNumber';";
			cmd.Parameters.AddWithValue("@Value", next.ToString());
			cmd.ExecuteNonQuery();
		}

		// ================= SAFE PARAM BINDING =================

		private static void BindParameters(SqliteCommand cmd, Project p)
		{
			cmd.Parameters.AddWithValue("@Id", p.Id.ToString());
			cmd.Parameters.AddWithValue("@JobNumber", p.JobNumber);
			cmd.Parameters.AddWithValue("@ProjectName", p.ProjectName);
			cmd.Parameters.AddWithValue("@Client", p.Client);
			cmd.Parameters.AddWithValue("@ClientRepresentative", p.ClientRepresentative ?? string.Empty);

			// Legacy compatibility
			cmd.Parameters.AddWithValue("@Amount", p.Budget);

			cmd.Parameters.AddWithValue("@Budget", p.Budget);
			cmd.Parameters.AddWithValue("@QuoteNumber", p.QuoteNumber ?? string.Empty);
			cmd.Parameters.AddWithValue("@OrderNumber", p.OrderNumber ?? string.Empty);
			cmd.Parameters.AddWithValue("@Material", p.Material ?? string.Empty);
			cmd.Parameters.AddWithValue("@AssignedTo", p.AssignedTo ?? string.Empty);
			cmd.Parameters.AddWithValue("@IsInvoiced", p.IsInvoiced ? 1 : 0);
			cmd.Parameters.AddWithValue("@InvoiceNumber", p.InvoiceNumber ?? string.Empty);

			cmd.Parameters.AddWithValue("@StartDate",
				p.StartDate.HasValue ? p.StartDate.Value.ToString("O") : DBNull.Value);

			cmd.Parameters.AddWithValue("@EndDate",
				p.EndDate.HasValue ? p.EndDate.Value.ToString("O") : DBNull.Value);

			cmd.Parameters.AddWithValue("@Status", (int)p.Status);
			cmd.Parameters.AddWithValue("@CreatedOn", p.CreatedOn.ToString("O"));
			cmd.Parameters.AddWithValue("@ActualCost", p.ActualCost);
			cmd.Parameters.AddWithValue("@CommittedCost", p.CommittedCost);
			cmd.Parameters.AddWithValue("@CompletedOn",
				p.CompletedOn.HasValue ? p.CompletedOn.Value.ToString("O") : DBNull.Value);
			cmd.Parameters.AddWithValue("@LastModifiedOn",
				p.LastModifiedOn.HasValue ? p.LastModifiedOn.Value.ToString("O") : DBNull.Value);
			cmd.Parameters.AddWithValue("@IsArchived", p.IsArchived ? 1 : 0);
		}

		private static Project Map(IDataRecord r) => new()
		{
			Id = Guid.Parse(r["Id"].ToString()!),
			JobNumber = Convert.ToInt32(r["JobNumber"]),
			ProjectName = r["ProjectName"].ToString()!,
			Client = r["Client"].ToString()!,
			ClientRepresentative = r["ClientRepresentative"]?.ToString() ?? string.Empty,

			Budget = r["Budget"] != DBNull.Value
				? Convert.ToDecimal(r["Budget"])
				: Convert.ToDecimal(r["Amount"]), // fallback for legacy

			QuoteNumber = r["QuoteNumber"]?.ToString() ?? string.Empty,
			OrderNumber = r["OrderNumber"]?.ToString() ?? string.Empty,
			Material = r["Material"]?.ToString() ?? string.Empty,
			AssignedTo = r["AssignedTo"]?.ToString() ?? string.Empty,
			IsInvoiced = Convert.ToInt32(r["IsInvoiced"]) == 1,
			InvoiceNumber = r["InvoiceNumber"]?.ToString(),

			StartDate = r["StartDate"] == DBNull.Value ? null : DateTime.Parse(r["StartDate"].ToString()!),
			EndDate = r["EndDate"] == DBNull.Value ? null : DateTime.Parse(r["EndDate"].ToString()!),
			CompletedOn = r["CompletedOn"] == DBNull.Value ? null : DateTime.Parse(r["CompletedOn"].ToString()!),

			Status = (ProjectStatus)Convert.ToInt32(r["Status"]),
			CreatedOn = DateTime.Parse(r["CreatedOn"].ToString()!),
			LastModifiedOn = r["LastModifiedOn"] == DBNull.Value ? null : DateTime.Parse(r["LastModifiedOn"].ToString()!),

			ActualCost = r["ActualCost"] != DBNull.Value ? Convert.ToDecimal(r["ActualCost"]) : 0,
			CommittedCost = r["CommittedCost"] != DBNull.Value ? Convert.ToDecimal(r["CommittedCost"]) : 0,
			IsArchived = r["IsArchived"] != DBNull.Value && Convert.ToInt32(r["IsArchived"]) == 1
		};
	}
}
