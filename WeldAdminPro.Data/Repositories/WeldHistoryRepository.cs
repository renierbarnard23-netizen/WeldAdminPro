using Microsoft.Data.Sqlite;
using System.IO;
using System;
using System.Collections.Generic;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Data.Repositories
{
    public class WeldHistoryRepository
    {
        private readonly string _connectionString;

        public WeldHistoryRepository()
        {
            var appData =
                Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData);

            var dbFolder =
                Path.Combine(
                    appData,
                    "WeldAdminPro");

            Directory.CreateDirectory(
                dbFolder);

            _connectionString =
                $"Data Source={Path.Combine(
                    dbFolder,
                    "weldadmin.db")}";
        }

        public WeldHistoryRepository(
            string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Add(
            WeldHistoryEntry entry)
        {
            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            var cmd = connection.CreateCommand();

            cmd.CommandText = @"
INSERT INTO WeldHistory
(
    Id,
    WeldId,
    EventDate,
    EventType,
    Description,
    UserName,
    StatusSnapshot
)
VALUES
(
    $id,
    $weldId,
    $eventDate,
    $eventType,
    $description,
    $userName,
    $status
);";

            cmd.Parameters.AddWithValue(
                "$id",
                entry.Id.ToString());

            cmd.Parameters.AddWithValue(
                "$weldId",
                entry.WeldId.ToString());

            cmd.Parameters.AddWithValue(
                "$eventDate",
                entry.EventDate.ToString("O"));

            cmd.Parameters.AddWithValue(
                "$eventType",
                entry.EventType);

            cmd.Parameters.AddWithValue(
                "$description",
                entry.Description);

            cmd.Parameters.AddWithValue(
                "$userName",
                entry.UserName);

            cmd.Parameters.AddWithValue(
                "$status",
                entry.StatusSnapshot);

            cmd.ExecuteNonQuery();
        }

        public List<WeldHistoryEntry> GetByWeld(
            Guid weldId)
        {
            var list =
                new List<WeldHistoryEntry>();

            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            var cmd = connection.CreateCommand();

            cmd.CommandText = @"
SELECT *
FROM WeldHistory
WHERE WeldId = $weldId
ORDER BY EventDate DESC;";

            cmd.Parameters.AddWithValue(
                "$weldId",
                weldId.ToString());

            using var reader =
                cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new WeldHistoryEntry
                {
                    Id = Guid.Parse(
                        reader["Id"].ToString()!),

                    WeldId = Guid.Parse(
                        reader["WeldId"].ToString()!),

                    EventDate = DateTime.Parse(
                        reader["EventDate"].ToString()!),

                    EventType =
                        reader["EventType"]
                            ?.ToString() ?? "",

                    Description =
                        reader["Description"]
                            ?.ToString() ?? "",

                    UserName =
                        reader["UserName"]
                            ?.ToString() ?? "",

                    StatusSnapshot =
                        reader["StatusSnapshot"]
                            ?.ToString() ?? ""
                });
            }

            return list;
        }
    }
}