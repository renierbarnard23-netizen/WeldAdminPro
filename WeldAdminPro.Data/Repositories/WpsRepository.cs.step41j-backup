using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Sqlite;
using WeldAdminPro.Core.Interfaces;
using WeldAdminPro.Core.Quality;

namespace WeldAdminPro.Data.Repositories
{
    public class WpsRepository : IWpsRepository
    {
        private string _connectionString =>
            $"Data Source={DatabasePath.Get()}";

        public WpsRepository()
        {
            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            EnsureTables(connection);
        }

        public void Add(Wps wps)
        {
            if (string.IsNullOrWhiteSpace(
                    wps.WpsNumber))
            {
                throw new InvalidOperationException(
                    "WPS Number is required.");
            }

            if (string.IsNullOrWhiteSpace(
                    wps.Process))
            {
                throw new InvalidOperationException(
                    "Process is required.");
            }

            wps.WpsNumber =
                wps.WpsNumber
                    .Trim()
                    .ToUpperInvariant();

            if (GetByWpsNumber(
                    wps.WpsNumber) != null)
            {
                throw new InvalidOperationException(
                    $"WPS '{wps.WpsNumber}' already exists.");
            }

            using var connection =
                new SqliteConnection(
                    _connectionString);

            connection.Open();
            EnsureTables(connection);

            if (wps.Id == Guid.Empty)
            {
                wps.Id = Guid.NewGuid();
            }

            var cmd = connection.CreateCommand();

            cmd.CommandText = @"
INSERT INTO Wps
(
    Id,
    WpsNumber,
    Process,
    MaterialGroup,
    ThicknessMin,
    ThicknessMax,
    PqrId,
    PNumber,
    FNumber,
    Position,
    JointType,
    Diameter,
    IsApproved,
    IsLocked,
    ApprovedOn,
    ApprovedBy,
    Revision,
    IsActive
)
VALUES
(
    $id,
    $wpsNumber,
    $process,
    $materialGroup,
    $tMin,
    $tMax,
    $pqrId,
    $pNumber,
    $fNumber,
    $position,
    $jointType,
    $diameter,
    $approved,
    $locked,
    $approvedOn,
    $approvedBy,
    $revision,
    $isActive
);";

            cmd.Parameters.AddWithValue(
                "$id",
                wps.Id.ToString());

            cmd.Parameters.AddWithValue(
                "$wpsNumber",
                wps.WpsNumber);

            cmd.Parameters.AddWithValue(
                "$process",
                wps.Process);

            cmd.Parameters.AddWithValue(
                "$materialGroup",
                wps.MaterialGroup);

            cmd.Parameters.AddWithValue(
                "$tMin",
                wps.ThicknessMin);

            cmd.Parameters.AddWithValue(
                "$tMax",
                wps.ThicknessMax);

            cmd.Parameters.AddWithValue(
                "$pqrId",
                (object?)wps.PqrId?.ToString()
                ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "$pNumber",
                (object?)wps.PNumber
                ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "$fNumber",
                (object?)wps.FNumber
                ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "$position",
                (object?)wps.Position
                ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "$jointType",
                (object?)wps.JointType
                ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "$diameter",
                wps.Diameter);

            cmd.Parameters.AddWithValue(
                "$approved",
                wps.IsApproved ? 1 : 0);

            cmd.Parameters.AddWithValue(
                "$locked",
                wps.IsLocked ? 1 : 0);

            cmd.Parameters.AddWithValue(
                "$approvedOn",
                (object?)wps.ApprovedOn?.ToString("s")
                ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "$approvedBy",
                (object?)wps.ApprovedBy
                ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "$revision",
                wps.Revision);

            cmd.Parameters.AddWithValue(
                "$isActive",
                wps.IsActive ? 1 : 0);

            cmd.ExecuteNonQuery();
        }

        private void EnsureTables(
            SqliteConnection connection)
        {
            using var cmd =
                connection.CreateCommand();

            cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS Wps
(
    Id TEXT PRIMARY KEY,
    WpsNumber TEXT NOT NULL,
    Process TEXT,
    MaterialGroup TEXT,
    ThicknessMin REAL,
    ThicknessMax REAL,
    PqrId TEXT,
    PNumber TEXT,
    FNumber TEXT,
    Position TEXT,
    JointType TEXT,
    Diameter REAL NOT NULL DEFAULT 0,
    IsPipe INTEGER NOT NULL DEFAULT 0,
    IsApproved INTEGER NOT NULL DEFAULT 0,
    IsLocked INTEGER NOT NULL DEFAULT 0,
    ApprovedOn TEXT,
    ApprovedBy TEXT,
    Revision INTEGER NOT NULL DEFAULT 0,
    IsActive INTEGER NOT NULL DEFAULT 1
);";

            cmd.ExecuteNonQuery();

            cmd.CommandText = @"
CREATE INDEX IF NOT EXISTS
IX_Wps_Number
ON Wps(WpsNumber);

CREATE INDEX IF NOT EXISTS
IX_Wps_PqrId
ON Wps(PqrId);

CREATE INDEX IF NOT EXISTS
IX_Wps_Approved
ON Wps(IsApproved);

CREATE INDEX IF NOT EXISTS
IX_Wps_Active
ON Wps(IsActive);

CREATE INDEX IF NOT EXISTS
IX_Wps_Process
ON Wps(Process);
";

            cmd.ExecuteNonQuery();
        }

        public List<Wps> GetAll()
        {
            var list =
                new List<Wps>();

            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            var cmd =
                connection.CreateCommand();

            cmd.CommandText = @"
                SELECT *
                FROM Wps
                ORDER BY
                    WpsNumber,
                    Revision DESC;";

            using var reader =
                cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(Map(reader));
            }

            return list;
        }

        public Wps? GetByWpsNumber(
            string wpsNumber)
        {
            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            var cmd =
                connection.CreateCommand();

            cmd.CommandText =
                "SELECT * FROM Wps WHERE WpsNumber = $wpsNumber LIMIT 1";

            cmd.Parameters.AddWithValue(
                "$wpsNumber",
                wpsNumber);

            using var reader =
                cmd.ExecuteReader();

            if (reader.Read())
            {
                return Map(reader);
            }

            return null;
        }

        private Wps Map(
            SqliteDataReader reader)
        {
            Guid id =
                Guid.Empty;

            var idValue =
                reader["Id"]?.ToString();

            if (!string.IsNullOrWhiteSpace(idValue))
            {
                Guid.TryParse(
                    idValue,
                    out id);
            }

            Guid? pqrId =
                null;

            var pqrValue =
                reader["PqrId"]?.ToString();

            if (!string.IsNullOrWhiteSpace(pqrValue)
                &&
                Guid.TryParse(
                    pqrValue,
                    out var parsedPqr))
            {
                pqrId =
                    parsedPqr;
            }

            return new Wps
            {
                Id = id,

                WpsNumber =
                    reader["WpsNumber"]?.ToString()
                    ?? "",

                Process =
                    reader["Process"]?.ToString()
                    ?? "",

                MaterialGroup =
                    !string.IsNullOrWhiteSpace(
                        reader["MaterialGroup"]?.ToString())
                    ? reader["MaterialGroup"]!.ToString()!
                    : reader["PNumber"]?.ToString() ?? "",

                ThicknessMin =
                    reader["ThicknessMin"] != DBNull.Value
                        ? Convert.ToDouble(
                            reader["ThicknessMin"])
                        : 0,

                ThicknessMax =
                    reader["ThicknessMax"] != DBNull.Value
                        ? Convert.ToDouble(
                            reader["ThicknessMax"])
                        : 0,

                PqrId =
                    pqrId,

                PNumber =
                    reader["PNumber"] == DBNull.Value
                        ? null
                        : reader["PNumber"].ToString(),

                FNumber =
                    reader["FNumber"] == DBNull.Value
                        ? null
                        : reader["FNumber"].ToString(),

                Position =
                    reader["Position"] == DBNull.Value
                        ? null
                        : reader["Position"].ToString(),

                JointType =
                    reader["JointType"] == DBNull.Value
                        ? null
                        : reader["JointType"].ToString(),

                Diameter =
                    reader["Diameter"] != DBNull.Value
                        ? Convert.ToDouble(
                            reader["Diameter"])
                        : 0,

                IsApproved =
                    Convert.ToInt32(
                        reader["IsApproved"]) == 1,

                IsLocked =
                    Convert.ToInt32(
                        reader["IsLocked"]) == 1,

                ApprovedOn =
                    reader["ApprovedOn"] == DBNull.Value
                        ? null
                        : DateTime.Parse(
                            reader["ApprovedOn"]
                                .ToString()!),

                ApprovedBy =
                    reader["ApprovedBy"]?.ToString(),

                Revision =
                    reader["Revision"] != DBNull.Value
                        ? Convert.ToInt32(
                            reader["Revision"])
                        : 0,

                IsActive =
                    reader["IsActive"] != DBNull.Value
                    &&
                    Convert.ToInt32(
                        reader["IsActive"]) == 1
            };
        }

        public void Update(Wps wps)
        {
            if (string.IsNullOrWhiteSpace(
        wps.WpsNumber))
            {
                throw new InvalidOperationException(
                    "WPS Number is required.");
            }

            wps.WpsNumber =
                wps.WpsNumber
                    .Trim()
                    .ToUpperInvariant();

            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            var cmd =
                connection.CreateCommand();

            cmd.CommandText = @"
UPDATE Wps
SET
    WpsNumber = $wpsNumber,
    Process = $process,
    MaterialGroup = $materialGroup,
    ThicknessMin = $tMin,
    ThicknessMax = $tMax,
    PqrId = $pqrId,
    PNumber = $pNumber,
    FNumber = $fNumber,
    Position = $position,
    JointType = $jointType,
    Diameter = $diameter,
    
    IsApproved = $approved,
    IsLocked = $locked,
    ApprovedOn = $approvedOn,
    ApprovedBy = $approvedBy,
    Revision = $revision,
    IsActive = $isActive
WHERE Id = $id;";

            cmd.Parameters.AddWithValue(
                "$id",
                wps.Id.ToString());

            cmd.Parameters.AddWithValue(
                "$wpsNumber",
                wps.WpsNumber);

            cmd.Parameters.AddWithValue(
                "$process",
                wps.Process);

            cmd.Parameters.AddWithValue(
                "$materialGroup",
                wps.MaterialGroup);

            cmd.Parameters.AddWithValue(
                "$tMin",
                wps.ThicknessMin);

            cmd.Parameters.AddWithValue(
                "$tMax",
                wps.ThicknessMax);

            cmd.Parameters.AddWithValue(
                "$pqrId",
                (object?)wps.PqrId?.ToString()
                ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "$pNumber",
                (object?)wps.PNumber
                ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "$fNumber",
                (object?)wps.FNumber
                ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "$position",
                (object?)wps.Position
                ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "$jointType",
                (object?)wps.JointType
                ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "$diameter",
                wps.Diameter);

            cmd.Parameters.AddWithValue(
                "$approved",
                wps.IsApproved ? 1 : 0);

            cmd.Parameters.AddWithValue(
                "$locked",
                wps.IsLocked ? 1 : 0);

            cmd.Parameters.AddWithValue(
                "$approvedOn",
                (object?)wps.ApprovedOn?.ToString("s")
                ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "$approvedBy",
                (object?)wps.ApprovedBy
                ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "$revision",
                wps.Revision);

            cmd.Parameters.AddWithValue(
                "$isActive",
                wps.IsActive ? 1 : 0);

            cmd.ExecuteNonQuery();
        }

        public void Delete(Guid id)
        {
            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            var cmd =
                connection.CreateCommand();

            cmd.CommandText =
                "DELETE FROM Wps WHERE Id = $id;";

            cmd.Parameters.AddWithValue(
                "$id",
                id.ToString());

            cmd.ExecuteNonQuery();
        }

        public List<Wps> GetActive()
        {
            var list =
                new List<Wps>();

            using var connection =
                new SqliteConnection(
                    _connectionString);

            connection.Open();

            using var cmd =
                connection.CreateCommand();

            cmd.CommandText = @"
SELECT *
FROM Wps
WHERE IsActive = 1
ORDER BY
    WpsNumber,
    Revision DESC;";

            using var reader =
                cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(
                    Map(reader));
            }

            return list;
        }

        public int GetNextRevision(
            string wpsNumber)
        {
            var all =
                GetAll()
                    .Where(x =>
                        x.WpsNumber == wpsNumber)
                    .ToList();

            if (!all.Any())
                return 0;

            return all.Max(x => x.Revision) + 1;
        }

        public void DeactivatePrevious(
            string wpsNumber)
        {
            var items =
                GetAll()
                    .Where(x =>
                        x.WpsNumber == wpsNumber
                        &&
                        x.IsActive)
                    .ToList();

            foreach (var item in items)
            {
                item.IsActive = false;
                Update(item);
            }
        }

        public Wps? GetByNumber(
    string wpsNumber)
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            connection.Open();

            using var cmd =
                connection.CreateCommand();

            cmd.CommandText = @"
SELECT *
FROM Wps
WHERE WpsNumber = $number
LIMIT 1;";

            cmd.Parameters.AddWithValue(
                "$number",
                wpsNumber);

            using var reader =
                cmd.ExecuteReader();

            if (!reader.Read())
                return null;

            return Map(reader);
        }
    }
}