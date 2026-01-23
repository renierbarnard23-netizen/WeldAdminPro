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
		// ENABLE / DISABLE
		// =========================
		public void Disable(Guid id)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText =
				"UPDATE Categories SET IsActive = 0 WHERE Id = $id;";
			cmd.Parameters.AddWithValue("$id", id.ToString());

			cmd.ExecuteNonQuery();
		}

		public void Enable(Guid id)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			using var cmd = connection.CreateCommand();
			cmd.CommandText =
				"UPDATE Categories SET IsActive = 1 WHERE Id = $id;";
			cmd.Parameters.AddWithValue("$id", id.ToString());

			cmd.ExecuteNonQuery();
		}

		// =========================
		// MAP
		// =========================
		private static Category Read(SqliteDataReader reader)
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
