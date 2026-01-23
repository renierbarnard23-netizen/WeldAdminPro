using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data;

namespace WeldAdminPro.Data.Repositories
{
	public class CategoryRepository
	{
		private readonly string _connectionString =
			$"Data Source={DatabasePath.Get()}";

		public CategoryRepository()
		{
			EnsureSchema();
		}

		// =========================
		// DB SETUP
		// =========================
		private void EnsureSchema()
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Categories (
                    Id TEXT PRIMARY KEY,
                    Name TEXT NOT NULL UNIQUE,
                    IsActive INTEGER NOT NULL DEFAULT 1
                );
            ";
			cmd.ExecuteNonQuery();

			EnsureDefaultCategory(connection);
		}

		private static void EnsureDefaultCategory(SqliteConnection connection)
		{
			using var cmd = connection.CreateCommand();
			cmd.CommandText = @"
                INSERT INTO Categories (Id, Name, IsActive)
                SELECT $id, 'Uncategorised', 1
                WHERE NOT EXISTS (
                    SELECT 1 FROM Categories WHERE Name = 'Uncategorised'
                );
            ";

			cmd.Parameters.AddWithValue("$id", Guid.NewGuid().ToString());
			cmd.ExecuteNonQuery();
		}

		// =========================
		// GET
		// =========================
		public List<Category> GetAll()
		{
			var list = new List<Category>();

			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText =
				"SELECT Id, Name, IsActive FROM Categories ORDER BY Name;";

			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
				list.Add(Read(reader));
			}

			return list;
		}

		public List<Category> GetAllActive()
		{
			var list = new List<Category>();

			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText =
				"SELECT Id, Name, IsActive FROM Categories WHERE IsActive = 1 ORDER BY Name;";

			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
				list.Add(Read(reader));
			}

			return list;
		}

		// =========================
		// ADD / UPDATE
		// =========================
		public void Add(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
				return;

			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = @"
                INSERT OR IGNORE INTO Categories (Id, Name, IsActive)
                VALUES ($id, $name, 1);
            ";

			cmd.Parameters.AddWithValue("$id", Guid.NewGuid().ToString());
			cmd.Parameters.AddWithValue("$name", name.Trim());

			cmd.ExecuteNonQuery();
		}

		public void Update(Category category)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = @"
                UPDATE Categories
                SET Name = $name,
                    IsActive = $active
                WHERE Id = $id;
            ";

			cmd.Parameters.AddWithValue("$id", category.Id.ToString());
			cmd.Parameters.AddWithValue("$name", category.Name.Trim());
			cmd.Parameters.AddWithValue("$active", category.IsActive ? 1 : 0);

			cmd.ExecuteNonQuery();
		}

		// =========================
		// DELETE / DISABLE (SAFE)
		// =========================
		public void Disable(Category category)
		{
			if (IsCategoryInUse(category.Name))
				throw new InvalidOperationException(
					"This category is used by one or more stock items and cannot be disabled.");

			category.IsActive = false;
			Update(category);
		}

		public void Delete(Category category)
		{
			if (IsCategoryInUse(category.Name))
				throw new InvalidOperationException(
					"This category is used by one or more stock items and cannot be deleted.");

			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText =
				"DELETE FROM Categories WHERE Id = $id;";

			cmd.Parameters.AddWithValue("$id", category.Id.ToString());
			cmd.ExecuteNonQuery();
		}

		// =========================
		// USAGE CHECK (KEY LOGIC)
		// =========================
		public bool IsCategoryInUse(string categoryName)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = @"
                SELECT COUNT(*)
                FROM StockItems
                WHERE Category = $name;
            ";

			cmd.Parameters.AddWithValue("$name", categoryName);

			return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
		}

		// =========================
		// MAP
		// =========================
		private static Category Read(SqliteDataReader r)
		{
			return new Category
			{
				Id = Guid.Parse(r.GetString(0)),
				Name = r.GetString(1),
				IsActive = r.GetInt32(2) == 1
			};
		}
	}
}
