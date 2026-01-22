using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Data.Repositories
{
	public class CategoryRepository
	{
		private readonly string _connectionString;

		public CategoryRepository()
		{
			var dbPath = DatabasePath.Get();
			_connectionString = $"Data Source={dbPath}";
		}


		// =========================
		// DB INIT
		// =========================
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

			AddIfMissing("Uncategorised");
		}

		// =========================
		// GET
		// =========================
		public IEnumerable<Category> GetAll()
		{
			var list = new List<Category>();

			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			var cmd = connection.CreateCommand();
			cmd.CommandText =
				"SELECT Id, Name, IsActive FROM Categories ORDER BY Name;";

			using var reader = cmd.ExecuteReader();
			while (reader.Read())
				list.Add(ReadCategory(reader));

			return list;
		}

		public IEnumerable<Category> GetAllActive()
		{
			var list = new List<Category>();

			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			var cmd = connection.CreateCommand();
			cmd.CommandText =
				"SELECT Id, Name, IsActive FROM Categories " +
				"WHERE IsActive = 1 ORDER BY Name;";

			using var reader = cmd.ExecuteReader();
			while (reader.Read())
				list.Add(ReadCategory(reader));

			return list;
		}

		// =========================
		// ADD
		// =========================
		public void Add(Category category)
		{
			if (string.IsNullOrWhiteSpace(category.Name))
				return;

			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			var cmd = connection.CreateCommand();
			cmd.CommandText =
				"INSERT OR IGNORE INTO Categories (Id, Name, IsActive) " +
				"VALUES ($id, $name, 1);";

			cmd.Parameters.AddWithValue(
				"$id",
				category.Id == Guid.Empty
					? Guid.NewGuid().ToString()
					: category.Id.ToString());

			cmd.Parameters.AddWithValue("$name", category.Name.Trim());
			cmd.ExecuteNonQuery();
		}

		public void Add(string name)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			var cmd = connection.CreateCommand();
			cmd.CommandText =
				"INSERT INTO Categories (Id, Name, IsActive) " +
				"VALUES ($id, $name, 1) " +
				"ON CONFLICT(Name) DO UPDATE SET IsActive = 1;";

			cmd.Parameters.AddWithValue("$id", Guid.NewGuid().ToString());
			cmd.Parameters.AddWithValue("$name", name.Trim());

			cmd.ExecuteNonQuery();
		}

		private void AddIfMissing(string name)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			var cmd = connection.CreateCommand();
			cmd.CommandText =
				"INSERT OR IGNORE INTO Categories (Id, Name, IsActive) " +
				"VALUES ($id, $name, 1);";

			cmd.Parameters.AddWithValue("$id", Guid.NewGuid().ToString());
			cmd.Parameters.AddWithValue("$name", name);
			cmd.ExecuteNonQuery();
		}

		// =========================
		// UPDATE / RENAME
		// =========================
		public void Update(Category category)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			var cmd = connection.CreateCommand();
			cmd.CommandText =
				"UPDATE Categories SET Name = $name, IsActive = $active " +
				"WHERE Id = $id;";

			cmd.Parameters.AddWithValue("$id", category.Id.ToString());
			cmd.Parameters.AddWithValue("$name", category.Name);
			cmd.Parameters.AddWithValue("$active", category.IsActive ? 1 : 0);

			cmd.ExecuteNonQuery();
		}

		public void Rename(Guid id, string newName)
		{
			Update(new Category
			{
				Id = id,
				Name = newName,
				IsActive = true
			});
		}

		// =========================
		// DISABLE (WITH USAGE CHECK)
		// =========================
		public void Disable(Guid id, string categoryName)
		{
			if (IsCategoryInUse(categoryName))
			{
				throw new InvalidOperationException(
					"This category is currently used by one or more stock items and cannot be disabled.");
			}

			Deactivate(id);
		}

		public void Disable(Guid id)
		{
			Deactivate(id);
		}

		private void Deactivate(Guid id)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			var cmd = connection.CreateCommand();
			cmd.CommandText =
				"UPDATE Categories SET IsActive = 0 WHERE Id = $id;";

			cmd.Parameters.AddWithValue("$id", id.ToString());
			cmd.ExecuteNonQuery();
		}

		// =========================
		// USAGE CHECK
		// =========================
		private bool IsCategoryInUse(string categoryName)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			var cmd = connection.CreateCommand();
			cmd.CommandText =
				"SELECT COUNT(*) FROM StockItems WHERE Category = $name;";
			cmd.Parameters.AddWithValue("$name", categoryName);

			var count = Convert.ToInt32(cmd.ExecuteScalar());
			return count > 0;
		}

		// =========================
		// MAP
		// =========================
		private static Category ReadCategory(SqliteDataReader reader)
		{
			return new Category
			{
				Id = Guid.Parse(reader.GetString(0)),
				Name = reader.GetString(1),
				IsActive = reader.GetInt32(2) == 1
			};
		}
	}
}
