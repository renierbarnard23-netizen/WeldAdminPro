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
			{
				list.Add(ReadCategory(reader));
			}

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
			{
				list.Add(ReadCategory(reader));
			}

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
			Add(new Category
			{
				Id = Guid.NewGuid(),
				Name = name,
				IsActive = true
			});
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

		// UI compatibility
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
		// SOFT DELETE / DISABLE
		// =========================
		public void Deactivate(Guid id)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			var cmd = connection.CreateCommand();
			cmd.CommandText =
				"UPDATE Categories SET IsActive = 0 WHERE Id = $id;";

			cmd.Parameters.AddWithValue("$id", id.ToString());
			cmd.ExecuteNonQuery();
		}

		// UI compatibility (bool)
		public void Disable(Guid id, bool isActive)
		{
			if (!isActive)
				Deactivate(id);
		}

		// UI compatibility (string — legacy signature)
		public void Disable(Guid id, string _)
		{
			Deactivate(id);
		}

		// Legacy / simple call
		public void Disable(Guid id)
		{
			Deactivate(id);
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
