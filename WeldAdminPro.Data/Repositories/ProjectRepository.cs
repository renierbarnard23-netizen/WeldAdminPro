using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;
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
			EnsureColumns();
		}

		public ProjectRepository(string connectionString)
		{
			_connectionString = connectionString;
			EnsureTables();
			EnsureColumns();
		}

		// =========================
		// SCHEMA
		// =========================

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
	Amount REAL NOT NULL,
	QuoteNumber TEXT,
	OrderNumber TEXT,
	Material TEXT,
	AssignedTo TEXT,
	IsInvoiced INTEGER NOT NULL,
	InvoiceNumber TEXT,
	CreatedOn TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS ProjectSettings (
	Key TEXT PRIMARY KEY,
	Value TEXT NOT NULL
);
";
			cmd.ExecuteNonQuery();
		}

		/// <summary>
		/// Adds missing columns safely for existing databases
		/// </summary>
		private void EnsureColumns()
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			void AddColumnIfMissing(string columnName, string sqlType)
			{
				using var check = connection.CreateCommand();
				check.CommandText = $"PRAGMA table_info(Projects);";

				using var reader = check.ExecuteReader();
				while (reader.Read())
				{
					if (reader["name"].ToString() == columnName)
						return;
				}

				using var alter = connection.CreateCommand();
				alter.CommandText = $"ALTER TABLE Projects ADD COLUMN {columnName} {sqlType};";
				alter.ExecuteNonQuery();
			}

			AddColumnIfMissing("StartDate", "TEXT");
			AddColumnIfMissing("EndDate", "TEXT");
		}

		// =========================
		// READ
		// =========================

		public IEnumerable<Project> GetAll()
		{
			var list = new List<Project>();

			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = "SELECT * FROM Projects ORDER BY JobNumber DESC";

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

		// =========================
		// CREATE
		// =========================

		public void Add(Project project)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var tx = connection.BeginTransaction();

			int nextJobNumber = GetNextJobNumberInternal(connection);
			project.JobNumber = nextJobNumber;

			using var cmd = connection.CreateCommand();
			cmd.CommandText = @"
INSERT INTO Projects (
	Id, JobNumber, ProjectName, Client, ClientRepresentative,
	Amount, QuoteNumber, OrderNumber, Material, AssignedTo,
	IsInvoiced, InvoiceNumber, StartDate, EndDate, CreatedOn
) VALUES (
	@Id, @JobNumber, @ProjectName, @Client, @ClientRepresentative,
	@Amount, @QuoteNumber, @OrderNumber, @Material, @AssignedTo,
	@IsInvoiced, @InvoiceNumber, @StartDate, @EndDate, @CreatedOn
);";

			cmd.Parameters.AddWithValue("@Id", project.Id.ToString());
			cmd.Parameters.AddWithValue("@JobNumber", project.JobNumber);
			cmd.Parameters.AddWithValue("@ProjectName", project.ProjectName);
			cmd.Parameters.AddWithValue("@Client", project.Client);
			cmd.Parameters.AddWithValue("@ClientRepresentative", project.ClientRepresentative);
			cmd.Parameters.AddWithValue("@Amount", project.Amount);
			cmd.Parameters.AddWithValue("@QuoteNumber", project.QuoteNumber);
			cmd.Parameters.AddWithValue("@OrderNumber", project.OrderNumber);
			cmd.Parameters.AddWithValue("@Material", project.Material);
			cmd.Parameters.AddWithValue("@AssignedTo", project.AssignedTo);
			cmd.Parameters.AddWithValue("@IsInvoiced", project.IsInvoiced ? 1 : 0);
			cmd.Parameters.AddWithValue("@InvoiceNumber", project.InvoiceNumber ?? string.Empty);
			cmd.Parameters.AddWithValue("@StartDate", project.StartDate?.ToString("O"));
			cmd.Parameters.AddWithValue("@EndDate", project.EndDate?.ToString("O"));
			cmd.Parameters.AddWithValue("@CreatedOn", project.CreatedOn.ToString("O"));

			cmd.ExecuteNonQuery();

			UpdateNextJobNumber(connection, nextJobNumber + 1);
			tx.Commit();
		}

		// =========================
		// UPDATE
		// =========================

		public void Update(Project project)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = @"
UPDATE Projects SET
	ProjectName = @ProjectName,
	Client = @Client,
	ClientRepresentative = @ClientRepresentative,
	Amount = @Amount,
	QuoteNumber = @QuoteNumber,
	OrderNumber = @OrderNumber,
	Material = @Material,
	AssignedTo = @AssignedTo,
	IsInvoiced = @IsInvoiced,
	InvoiceNumber = @InvoiceNumber,
	StartDate = @StartDate,
	EndDate = @EndDate
WHERE Id = @Id;";

			cmd.Parameters.AddWithValue("@Id", project.Id.ToString());
			cmd.Parameters.AddWithValue("@ProjectName", project.ProjectName);
			cmd.Parameters.AddWithValue("@Client", project.Client);
			cmd.Parameters.AddWithValue("@ClientRepresentative", project.ClientRepresentative);
			cmd.Parameters.AddWithValue("@Amount", project.Amount);
			cmd.Parameters.AddWithValue("@QuoteNumber", project.QuoteNumber);
			cmd.Parameters.AddWithValue("@OrderNumber", project.OrderNumber);
			cmd.Parameters.AddWithValue("@Material", project.Material);
			cmd.Parameters.AddWithValue("@AssignedTo", project.AssignedTo);
			cmd.Parameters.AddWithValue("@IsInvoiced", project.IsInvoiced ? 1 : 0);
			cmd.Parameters.AddWithValue("@InvoiceNumber", project.InvoiceNumber ?? string.Empty);
			cmd.Parameters.AddWithValue("@StartDate", project.StartDate?.ToString("O"));
			cmd.Parameters.AddWithValue("@EndDate", project.EndDate?.ToString("O"));

			cmd.ExecuteNonQuery();
		}

		// =========================
		// DELETE
		// =========================

		public void Delete(Guid id)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = "DELETE FROM Projects WHERE Id = @Id";
			cmd.Parameters.AddWithValue("@Id", id.ToString());
			cmd.ExecuteNonQuery();
		}

		// =========================
		// JOB NUMBER
		// =========================

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
			cmd.CommandText = "SELECT Value FROM ProjectSettings WHERE Key = 'NextJobNumber';";

			var result = cmd.ExecuteScalar();

			if (result == null)
			{
				using var insert = connection.CreateCommand();
				insert.CommandText = @"
INSERT INTO ProjectSettings (Key, Value)
VALUES ('NextJobNumber', '1001');";
				insert.ExecuteNonQuery();

				return 1000;
			}

			return int.Parse(result.ToString()!);
		}

		private void UpdateNextJobNumber(SqliteConnection connection, int next)
		{
			using var cmd = connection.CreateCommand();
			cmd.CommandText = @"
UPDATE ProjectSettings
SET Value = @Value
WHERE Key = 'NextJobNumber';";
			cmd.Parameters.AddWithValue("@Value", next.ToString());
			cmd.ExecuteNonQuery();
		}

		// =========================
		// MAP (SAFE)
		// =========================

		private static Project Map(IDataRecord r)
		{
			DateTime? ReadDate(string name)
			{
				try
				{
					var val = r[name];
					return val == DBNull.Value ? null : DateTime.Parse(val.ToString()!);
				}
				catch (IndexOutOfRangeException)
				{
					return null;
				}
			}

			return new Project
			{
				Id = Guid.Parse(r["Id"].ToString()!),
				JobNumber = Convert.ToInt32(r["JobNumber"]),
				ProjectName = r["ProjectName"].ToString()!,
				Client = r["Client"].ToString()!,
				ClientRepresentative = r["ClientRepresentative"]?.ToString() ?? string.Empty,
				Amount = Convert.ToDecimal(r["Amount"]),
				QuoteNumber = r["QuoteNumber"]?.ToString() ?? string.Empty,
				OrderNumber = r["OrderNumber"]?.ToString() ?? string.Empty,
				Material = r["Material"]?.ToString() ?? string.Empty,
				AssignedTo = r["AssignedTo"]?.ToString() ?? string.Empty,
				IsInvoiced = Convert.ToInt32(r["IsInvoiced"]) == 1,
				InvoiceNumber = r["InvoiceNumber"]?.ToString(),
				StartDate = ReadDate("StartDate"),
				EndDate = ReadDate("EndDate"),
				CreatedOn = DateTime.Parse(r["CreatedOn"].ToString()!)
			};
		}
	}
}
