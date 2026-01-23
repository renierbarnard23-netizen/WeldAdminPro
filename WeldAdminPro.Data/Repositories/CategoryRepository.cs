using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Data.Repositories
{
	public class CategoryRepository
	{
		private readonly string _cs;

		public CategoryRepository()
		{
			_cs = $"Data Source={DatabasePath.Get()}";
			EnsureSchema();
		}

		private void EnsureSchema()
		{
			using var c = new SqliteConnection(_cs);
			c.Open();

			using var cmd = c.CreateCommand();
			cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS Categories (
                Id TEXT PRIMARY KEY,
                Name TEXT NOT NULL UNIQUE,
                IsActive INTEGER NOT NULL DEFAULT 1
            );
            """;
			cmd.ExecuteNonQuery();

			// Ensure Uncategorised exists (stable ID)
			cmd.CommandText = """
            INSERT OR IGNORE INTO Categories (Id, Name, IsActive)
            VALUES ('00000000-0000-0000-0000-000000000000', 'Uncategorised', 1);
            """;
			cmd.ExecuteNonQuery();
		}

		// =========================
		// GET
		// =========================

		public List<Category> GetAll()
		{
			var list = new List<Category>();

			using var c = new SqliteConnection(_cs);
			c.Open();

			using var cmd = c.CreateCommand();
			cmd.CommandText =
				"SELECT Id, Name, IsActive FROM Categories ORDER BY Name;";

			using var r = cmd.ExecuteReader();
			while (r.Read())
			{
				list.Add(new Category
				{
					Id = Guid.Parse(r.GetString(0)),
					Name = r.GetString(1),
					IsActive = r.GetInt32(2) == 1
				});
			}

			return list;
		}

		public List<Category> GetAllActive()
		{
			var list = new List<Category>();

			using var c = new SqliteConnection(_cs);
			c.Open();

			using var cmd = c.CreateCommand();
			cmd.CommandText =
				"SELECT Id, Name, IsActive FROM Categories WHERE IsActive = 1 ORDER BY Name;";

			using var r = cmd.ExecuteReader();
			while (r.Read())
			{
				list.Add(new Category
				{
					Id = Guid.Parse(r.GetString(0)),
					Name = r.GetString(1),
					IsActive = r.GetInt32(2) == 1
				});
			}

			return list;
		}

		// =========================
		// WRITE
		// =========================

		public void Add(string name)
		{
			using var c = new SqliteConnection(_cs);
			c.Open();

			using var cmd = c.CreateCommand();
			cmd.CommandText =
				"INSERT INTO Categories (Id, Name, IsActive) VALUES ($id,$name,1);";

			cmd.Parameters.AddWithValue("$id", Guid.NewGuid().ToString());
			cmd.Parameters.AddWithValue("$name", name.Trim());
			cmd.ExecuteNonQuery();
		}

		public void SetActive(Guid id, bool active)
		{
			using var c = new SqliteConnection(_cs);
			c.Open();

			using var cmd = c.CreateCommand();
			cmd.CommandText =
				"UPDATE Categories SET IsActive=$a WHERE Id=$id;";
			cmd.Parameters.AddWithValue("$a", active ? 1 : 0);
			cmd.Parameters.AddWithValue("$id", id.ToString());
			cmd.ExecuteNonQuery();
		}

		public void Delete(Guid id)
		{
			using var c = new SqliteConnection(_cs);
			c.Open();

			using var cmd = c.CreateCommand();
			cmd.CommandText =
				"DELETE FROM Categories WHERE Id=$id;";
			cmd.Parameters.AddWithValue("$id", id.ToString());
			cmd.ExecuteNonQuery();
		}
	}
}
