using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Data.Repositories
{
    public class WpsRepository
    {
        private readonly string _connectionString;

        public WpsRepository()
        {
            _connectionString = $"Data Source={DatabasePath.Get()}";
        }

        public void Add(Wps wps)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
            INSERT INTO Wps
            (
                Id,
                ProjectId,
                WpsNumber,
                Revision,
                BaseMaterial,
                FillerMaterial,
                Process,
                Position,
                Preheat,
                PostHeat,
                Author,
                ApprovedBy,
                ApprovedAt
            )
            VALUES
            (
                $id,
                $projectId,
                $num,
                $rev,
                $base,
                $fill,
                $proc,
                $pos,
                $pre,
                $post,
                $auth,
                $appr,
                $date
            );";

            cmd.Parameters.AddWithValue("$id", wps.Id.ToString());
            cmd.Parameters.AddWithValue("$projectId", wps.ProjectId.ToString());
            cmd.Parameters.AddWithValue("$num", wps.WpsNumber);
            cmd.Parameters.AddWithValue("$rev", wps.Revision ?? "");
            cmd.Parameters.AddWithValue("$base", wps.BaseMaterial ?? "");
            cmd.Parameters.AddWithValue("$fill", wps.FillerMaterial);
            cmd.Parameters.AddWithValue("$proc", wps.Process);
            cmd.Parameters.AddWithValue("$pos", wps.Position);
            cmd.Parameters.AddWithValue("$pre", wps.Preheat ?? "");
            cmd.Parameters.AddWithValue("$post", wps.PostHeat ?? "");
            cmd.Parameters.AddWithValue("$auth", wps.Author ?? "");
            cmd.Parameters.AddWithValue("$appr", wps.ApprovedBy ?? "");
            cmd.Parameters.AddWithValue("$date", wps.ApprovedAt.HasValue ? wps.ApprovedAt.Value.ToString("o") : DBNull.Value
);

            cmd.ExecuteNonQuery();
        }

        public List<Wps> GetAll()
        {
            var list = new List<Wps>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM Wps;";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Guid.TryParse(reader[0]?.ToString(), out var id);

                list.Add(new Wps
                {
                    Id = id,
                    ProjectId = reader.IsDBNull(1) ? Guid.Empty : Guid.Parse(reader.GetString(1)),
                    WpsNumber = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    Revision = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    BaseMaterial = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    FillerMaterial = reader.IsDBNull(5) ? "" : reader.GetString(5),
                    Process = reader.IsDBNull(6) ? "" : reader.GetString(6),
                    Position = reader.IsDBNull(7) ? "" : reader.GetString(7),
                    Preheat = reader.IsDBNull(8) ? "" : reader.GetString(8),
                    PostHeat = reader.IsDBNull(9) ? "" : reader.GetString(9),
                    Author = reader.IsDBNull(10) ? "" : reader.GetString(10),
                    ApprovedBy = reader.IsDBNull(11) ? "" : reader.GetString(11),
                    ApprovedAt = reader.IsDBNull(12) ? null : DateTime.Parse(reader.GetString(12))
                });
            }

            return list;
        }
    }
}