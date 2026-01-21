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

			// Seed default categories (safe, idempotent)
			AddIfMissing("Electrodes");
			AddIfMissing("Gas");
			AddIfMissing("Abrasives");
			AddIfMissing("PPE");
			AddIfMissing("Uncategorised");
		}

		private void AddIfMissing(string name)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			var check = connection.CreateCommand();
			check.CommandText = "SELECT COUNT(1) FROM Categories WHERE Name = $name;";
			check.Parameters.AddWithValue("$name", name);

			long exists = (long)check.ExecuteScalar();
			if (exists > 0)
				return;

			var insert = connection.CreateCommand();
			insert.CommandText =
				"INSERT INTO Categories (Id, Name, IsActive) VALUES ($id, $name, 1);";
			insert.Parameters.AddWithValue("$id", Guid.NewGuid().ToString());
			insert.Parameters.AddWithValue("$name", name);

			insert.ExecuteNonQuery();
		}

		public List<Category> GetAll()
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
				list.Add(new Category
				{
					Id = Guid.Parse(reader.GetString(0)),
					Name = reader.GetString(1),
					IsActive = reader.GetInt32(2) == 1
				});
			}

			return list;
		}

		public List<Category> GetActive()
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
					IsActive = true
				});
			}

			return list;
		}

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

		public void Rename(string oldName, string newName)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var tx = connection.BeginTransaction();

			var updateCategory = connection.CreateCommand();
			updateCategory.CommandText =
				"UPDATE Categories SET Name = $new WHERE Name = $old;";
			updateCategory.Parameters.AddWithValue("$old", oldName);
			updateCategory.Parameters.AddWithValue("$new", newName);
			updateCategory.ExecuteNonQuery();

			var updateStock = connection.CreateCommand();
			updateStock.CommandText =
				"UPDATE StockItems SET Category = $new WHERE Category = $old;";
			updateStock.Parameters.AddWithValue("$old", oldName);
			updateStock.Parameters.AddWithValue("$new", newName);
			updateStock.ExecuteNonQuery();

			tx.Commit();
		}

		public void Disable(string name)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			var cmd = connection.CreateCommand();
			cmd.CommandText =
				"UPDATE Categories SET IsActive = 0 WHERE Name = $name;";
			cmd.Parameters.AddWithValue("$name", name);

			cmd.ExecuteNonQuery();
		}
	}
}
