using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Data.Repositories
{
	public class CategoryRepository
	{
		private readonly string _connectionString = "Data Source=weldadmin.db";

		public CategoryRepository()
		{
			EnsureDatabase();
		}

		private void EnsureDatabase()
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			var cmd = connection.CreateCommand();

			cmd.CommandText =
				"CREATE TABLE IF NOT EXISTS Categories (" +
				"Id TEXT PRIMARY KEY, " +
				"Name TEXT NOT NULL UNIQUE, " +
				"IsActive INTEGER NOT NULL);";

			cmd.ExecuteNonQuery();
		}

		// =========================
		// READ
		// =========================

		public List<Category> GetAllActive()
		{
			var list = new List<Category>();

			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			var cmd = connection.CreateCommand();
			cmd.CommandText =
				"SELECT Id, Name, IsActive FROM Categories WHERE IsActive = 1 ORDER BY Name;";

			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
				list.Add(new Category
				{
					Id = Guid.Parse(reader.GetString(0)),
					Name = reader.GetString(1),
					IsActive = reader.GetInt32(2) == 1
				});
			}

			return list;
		}

		// =========================
		// SAFETY CHECK
		// =========================

		public bool IsCategoryInUse(string categoryName)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			var cmd = connection.CreateCommand();
			cmd.CommandText =
				"SELECT COUNT(1) FROM StockItems WHERE Category = $cat;";
			cmd.Parameters.AddWithValue("$cat", categoryName);

			return (long)cmd.ExecuteScalar() > 0;
		}

		// =========================
		// WRITE
		// =========================

		public void Add(string name)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			var cmd = connection.CreateCommand();
			cmd.CommandText =
				"INSERT INTO Categories (Id, Name, IsActive) VALUES ($id, $name, 1);";

			cmd.Parameters.AddWithValue("$id", Guid.NewGuid().ToString());
			cmd.Parameters.AddWithValue("$name", name);

			cmd.ExecuteNonQuery();
		}

		public void Rename(Guid id, string newName)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			var cmd = connection.CreateCommand();
			cmd.CommandText =
				"UPDATE Categories SET Name = $name WHERE Id = $id;";

			cmd.Parameters.AddWithValue("$id", id.ToString());
			cmd.Parameters.AddWithValue("$name", newName);

			cmd.ExecuteNonQuery();
		}

		public void Disable(Guid id, string name)
		{
			if (IsCategoryInUse(name))
				throw new InvalidOperationException(
					"This category is in use by stock items and cannot be disabled.");

			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			var cmd = connection.CreateCommand();
			cmd.CommandText =
				"UPDATE Categories SET IsActive = 0 WHERE Id = $id;";

			cmd.Parameters.AddWithValue("$id", id.ToString());
			cmd.ExecuteNonQuery();
		}
	}
}
