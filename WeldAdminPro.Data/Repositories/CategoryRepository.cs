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

            // Ensure Uncategorised exists
            cmd.CommandText =
                "INSERT OR IGNORE INTO Categories (Id, Name, IsActive) " +
                "VALUES ('00000000-0000-0000-0000-000000000000', 'Uncategorised', 1);";

            cmd.ExecuteNonQuery();
        }

        // =========================
        // READ
        // =========================

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

        public List<Category> GetAllActive()
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
                list.Add(new Category
                {
                    Id = Guid.Parse(reader.GetString(0)),
                    Name = reader.GetString(1),
                    IsActive = true
                });
            }

            return list;
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
                "INSERT INTO Categories (Id, Name, IsActive) " +
                "VALUES ($id, $name, 1);";

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

        public void SetActive(Guid id, bool isActive)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText =
                "UPDATE Categories SET IsActive = $active WHERE Id = $id;";

            cmd.Parameters.AddWithValue("$id", id.ToString());
            cmd.Parameters.AddWithValue("$active", isActive ? 1 : 0);

            cmd.ExecuteNonQuery();
        }
    }
}
