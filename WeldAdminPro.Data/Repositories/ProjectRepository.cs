using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Data.Repositories
{
    public class ProjectRepository
    {
        private readonly string _connectionString =
            "Data Source=weldadmin.db";

        public List<Project> GetAll()
        {
            var projects = new List<Project>();

            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText =
                @"SELECT Id, ProjectName, CreatedAt
                  FROM Projects";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                projects.Add(new Project
                {
                    Id = Guid.Parse(reader.GetString(0)),
                    ProjectName = reader.GetString(1),
                    CreatedAt = reader.GetString(2)
                });
            }

            return projects;
        }

        public void Add(Project project)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText =
                @"INSERT INTO Projects (Id, ProjectName, CreatedAt)
                  VALUES ($id, $name, $created)";

            cmd.Parameters.AddWithValue("$id", project.Id.ToString());
            cmd.Parameters.AddWithValue("$name", project.ProjectName);
            cmd.Parameters.AddWithValue("$created", project.CreatedAt);

            cmd.ExecuteNonQuery();
        }

        public void Update(Project project)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText =
                @"UPDATE Projects
                  SET ProjectName = $name
                  WHERE Id = $id";

            cmd.Parameters.AddWithValue("$name", project.ProjectName);
            cmd.Parameters.AddWithValue("$id", project.Id.ToString());

            cmd.ExecuteNonQuery();
        }

        public void Delete(Guid id)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText =
                "DELETE FROM Projects WHERE Id = $id";

            cmd.Parameters.AddWithValue("$id", id.ToString());
            cmd.ExecuteNonQuery();
        }
    }
}
