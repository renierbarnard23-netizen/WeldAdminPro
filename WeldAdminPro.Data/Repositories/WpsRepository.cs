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
        private readonly string _connectionString;

        public WpsRepository()
            : this(DatabasePath.Get())
        {
        }

        public WpsRepository(string databasePath)
        {
            if (string.IsNullOrWhiteSpace(databasePath))
                throw new ArgumentException(
                    "Database path is required.",
                    nameof(databasePath));

            var fullDatabasePath = System.IO.Path.GetFullPath(databasePath);
            var directory = System.IO.Path.GetDirectoryName(fullDatabasePath);

            if (!string.IsNullOrWhiteSpace(directory))
                System.IO.Directory.CreateDirectory(directory);

            _connectionString = $"Data Source={fullDatabasePath}";

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            EnsureTables(connection);
        }
        public void Add(Wps wps)
        {
            if (wps == null)
                throw new ArgumentNullException(nameof(wps));

            if (string.IsNullOrWhiteSpace(wps.WpsNumber))
                throw new InvalidOperationException("WPS Number is required.");

            if (string.IsNullOrWhiteSpace(wps.Process))
                throw new InvalidOperationException("Process is required.");

            wps.WpsNumber = wps.WpsNumber.Trim().ToUpperInvariant();

            if (GetByWpsNumber(wps.WpsNumber) != null)
            {
                throw new InvalidOperationException(
                    $"WPS '{wps.WpsNumber}' already exists.");
            }

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            EnsureTables(connection);

            if (wps.Id == Guid.Empty)
                wps.Id = Guid.NewGuid();

            if (wps.CreatedOn == default)
                wps.CreatedOn = DateTime.UtcNow;

            InsertFullWps(connection, null, wps);
        }

        private static readonly string[] FullWpsColumns =
        {
            "Id", "CompanyId", "WpsNumber", "Process", "MaterialGroup",
            "ThicknessMin", "ThicknessMax", "Diameter",
            "FillerMaterial", "GasType", "AmpsMin", "AmpsMax",
            "VoltsMin", "VoltsMax", "HeatInputMin", "HeatInputMax",
            "PreheatMin", "PreheatMax", "WelderNumber", "InterpassMax",
            "PwhtRequired", "PqrId", "LinkedPqrNumber", "PNumber",
            "FNumber", "Position", "JointType", "JointDesign", "Backing",
            "BackingType", "IsApproved", "IsLocked", "ApprovedOn",
            "ApprovedBy", "ParseConfidence", "ParseNotes",
            "PreviousRevisionId", "CreatedOn", "CreatedBy", "ImportSource",
            "Revision", "IsActive", "IsCompliant", "ValidationStatus",
            "CurrentType", "Polarity", "Progression", "JointDiagramPath",
            "PassDiagramPath", "QualifiedThicknessRange"
        };

        private static void InsertFullWps(
            SqliteConnection connection,
            SqliteTransaction? transaction,
            Wps wps)
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;

            command.CommandText =
                $"INSERT INTO Wps ({string.Join(", ", FullWpsColumns)}) " +
                $"VALUES ({string.Join(", ", Array.ConvertAll(FullWpsColumns, c => "$" + c))});";

            foreach (var column in FullWpsColumns)
            {
                var property = typeof(Wps).GetProperty(column);

                if (property == null)
                {
                    throw new InvalidOperationException(
                        $"WPS persistence mapping is missing model property '{column}'.");
                }

                object? value = property.GetValue(wps);

                if (value is Guid guid)
                    value = guid.ToString();
                else if (value is DateTime date)
                    value = date.ToString("O");
                else if (value is bool flag)
                    value = flag ? 1 : 0;

                command.Parameters.AddWithValue(
                    "$" + column,
                    value ?? DBNull.Value);
            }

            var affected = command.ExecuteNonQuery();

            if (affected != 1)
            {
                throw new InvalidOperationException(
                    "The WPS record could not be inserted.");
            }
        }
        private void EnsureTables(SqliteConnection connection)
        {
            using (var create = connection.CreateCommand())
            {
                create.CommandText = @"
CREATE TABLE IF NOT EXISTS Wps
(
    Id TEXT PRIMARY KEY,
    CompanyId TEXT NULL,
    WpsNumber TEXT NOT NULL,
    Process TEXT NULL,
    MaterialGroup TEXT NULL,
    ThicknessMin REAL NOT NULL DEFAULT 0,
    ThicknessMax REAL NOT NULL DEFAULT 0,
    Diameter REAL NOT NULL DEFAULT 0,
    FillerMaterial TEXT NULL,
    GasType TEXT NULL,
    AmpsMin REAL NOT NULL DEFAULT 0,
    AmpsMax REAL NOT NULL DEFAULT 0,
    VoltsMin REAL NOT NULL DEFAULT 0,
    VoltsMax REAL NOT NULL DEFAULT 0,
    HeatInputMin REAL NOT NULL DEFAULT 0,
    HeatInputMax REAL NOT NULL DEFAULT 0,
    PreheatMin REAL NOT NULL DEFAULT 0,
    PreheatMax REAL NOT NULL DEFAULT 0,
    WelderNumber TEXT NULL,
    InterpassMax REAL NULL,
    PwhtRequired INTEGER NOT NULL DEFAULT 0,
    PqrId TEXT NULL,
    LinkedPqrNumber TEXT NULL,
    PNumber TEXT NULL,
    FNumber TEXT NULL,
    Position TEXT NULL,
    JointType TEXT NULL,
    JointDesign TEXT NULL,
    Backing TEXT NULL,
    BackingType TEXT NULL,
    IsPipe INTEGER NOT NULL DEFAULT 0,
    IsApproved INTEGER NOT NULL DEFAULT 0,
    IsLocked INTEGER NOT NULL DEFAULT 0,
    ApprovedOn TEXT NULL,
    ApprovedBy TEXT NULL,
    ParseConfidence REAL NOT NULL DEFAULT 0,
    ParseNotes TEXT NULL,
    PreviousRevisionId TEXT NULL,
    CreatedOn TEXT NULL,
    CreatedBy TEXT NULL,
    ImportSource TEXT NULL,
    Revision INTEGER NOT NULL DEFAULT 0,
    IsActive INTEGER NOT NULL DEFAULT 1,
    IsCompliant INTEGER NOT NULL DEFAULT 0,
    ValidationStatus TEXT NULL,
    CurrentType TEXT NULL,
    Polarity TEXT NULL,
    Progression TEXT NULL,
    JointDiagramPath TEXT NULL,
    PassDiagramPath TEXT NULL,
    QualifiedThicknessRange TEXT NULL
);";
                create.ExecuteNonQuery();
            }

            var columns = new (string Name, string Definition)[]
            {
                ("CompanyId", "TEXT NULL"),
                ("WpsNumber", "TEXT NOT NULL DEFAULT ''"),
                ("Process", "TEXT NULL"),
                ("MaterialGroup", "TEXT NULL"),
                ("ThicknessMin", "REAL NOT NULL DEFAULT 0"),
                ("ThicknessMax", "REAL NOT NULL DEFAULT 0"),
                ("Diameter", "REAL NOT NULL DEFAULT 0"),
                ("FillerMaterial", "TEXT NULL"),
                ("GasType", "TEXT NULL"),
                ("AmpsMin", "REAL NOT NULL DEFAULT 0"),
                ("AmpsMax", "REAL NOT NULL DEFAULT 0"),
                ("VoltsMin", "REAL NOT NULL DEFAULT 0"),
                ("VoltsMax", "REAL NOT NULL DEFAULT 0"),
                ("HeatInputMin", "REAL NOT NULL DEFAULT 0"),
                ("HeatInputMax", "REAL NOT NULL DEFAULT 0"),
                ("PreheatMin", "REAL NOT NULL DEFAULT 0"),
                ("PreheatMax", "REAL NOT NULL DEFAULT 0"),
                ("WelderNumber", "TEXT NULL"),
                ("InterpassMax", "REAL NULL"),
                ("PwhtRequired", "INTEGER NOT NULL DEFAULT 0"),
                ("PqrId", "TEXT NULL"),
                ("LinkedPqrNumber", "TEXT NULL"),
                ("PNumber", "TEXT NULL"),
                ("FNumber", "TEXT NULL"),
                ("Position", "TEXT NULL"),
                ("JointType", "TEXT NULL"),
                ("JointDesign", "TEXT NULL"),
                ("Backing", "TEXT NULL"),
                ("BackingType", "TEXT NULL"),
                ("IsPipe", "INTEGER NOT NULL DEFAULT 0"),
                ("IsApproved", "INTEGER NOT NULL DEFAULT 0"),
                ("IsLocked", "INTEGER NOT NULL DEFAULT 0"),
                ("ApprovedOn", "TEXT NULL"),
                ("ApprovedBy", "TEXT NULL"),
                ("ParseConfidence", "REAL NOT NULL DEFAULT 0"),
                ("ParseNotes", "TEXT NULL"),
                ("PreviousRevisionId", "TEXT NULL"),
                ("CreatedOn", "TEXT NULL"),
                ("CreatedBy", "TEXT NULL"),
                ("ImportSource", "TEXT NULL"),
                ("Revision", "INTEGER NOT NULL DEFAULT 0"),
                ("IsActive", "INTEGER NOT NULL DEFAULT 1"),
                ("IsCompliant", "INTEGER NOT NULL DEFAULT 0"),
                ("ValidationStatus", "TEXT NULL"),
                ("CurrentType", "TEXT NULL"),
                ("Polarity", "TEXT NULL"),
                ("Progression", "TEXT NULL"),
                ("JointDiagramPath", "TEXT NULL"),
                ("PassDiagramPath", "TEXT NULL"),
                ("QualifiedThicknessRange", "TEXT NULL")
            };

            var existingColumns = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

            using (var inspect = connection.CreateCommand())
            {
                inspect.CommandText = "PRAGMA table_info(Wps);";
                using var reader = inspect.ExecuteReader();

                while (reader.Read())
                    existingColumns.Add(reader["name"].ToString() ?? "");
            }

            foreach (var column in columns)
            {
                if (existingColumns.Contains(column.Name))
                    continue;

                using var alter = connection.CreateCommand();
                alter.CommandText =
                    $"ALTER TABLE Wps ADD COLUMN [{column.Name}] {column.Definition};";
                alter.ExecuteNonQuery();
            }

            using var indexes = connection.CreateCommand();
            indexes.CommandText = @"
CREATE INDEX IF NOT EXISTS IX_Wps_Number ON Wps(WpsNumber);
CREATE INDEX IF NOT EXISTS IX_Wps_PqrId ON Wps(PqrId);
CREATE INDEX IF NOT EXISTS IX_Wps_Approved ON Wps(IsApproved);
CREATE INDEX IF NOT EXISTS IX_Wps_Active ON Wps(IsActive);
CREATE INDEX IF NOT EXISTS IX_Wps_Process ON Wps(Process);
CREATE INDEX IF NOT EXISTS IX_Wps_CompanyId ON Wps(CompanyId);";
            indexes.ExecuteNonQuery();
        }
        public List<Wps> GetByCompany(Guid companyId)
        {
            if (companyId == Guid.Empty)
                throw new ArgumentException("Company Id is required.", nameof(companyId));

            return GetAll()
                .Where(x => x.CompanyId == companyId)
                .ToList();
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

        public Wps? GetById(Guid id)
        {
            if (id == Guid.Empty)
                return null;

            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();
            EnsureTables(connection);

            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"SELECT * FROM Wps WHERE Id = $id LIMIT 1";
            cmd.Parameters.AddWithValue("$id", id.ToString());

            using var reader = cmd.ExecuteReader();
            return reader.Read() ? Map(reader) : null;
        }

        public Wps? GetById(
            Guid id,
            SqliteConnection connection,
            SqliteTransaction transaction)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));
            if (id == Guid.Empty)
                return null;

            using var cmd = connection.CreateCommand();
            cmd.Transaction = transaction;
            cmd.CommandText = @"SELECT * FROM Wps WHERE Id = $id LIMIT 1";
            cmd.Parameters.AddWithValue("$id", id.ToString());

            using var reader = cmd.ExecuteReader();
            return reader.Read() ? Map(reader) : null;
        }

        public Wps? GetByWpsNumber(
    string wpsNumber)
{
    if (string.IsNullOrWhiteSpace(wpsNumber))
        return null;

    using var connection =
        new SqliteConnection(_connectionString);

    connection.Open();
    EnsureTables(connection);

    using var cmd =
        connection.CreateCommand();

    cmd.CommandText = @"
SELECT *
FROM Wps
WHERE WpsNumber = $wpsNumber
ORDER BY
    IsActive DESC,
    Revision DESC
LIMIT 1";

    cmd.Parameters.AddWithValue(
        "$wpsNumber",
        wpsNumber.Trim());

    using var reader =
        cmd.ExecuteReader();

    if (reader.Read())
    {
        return Map(reader);
    }

    return null;
}

public Wps? GetByWpsNumber(
    string wpsNumber,
    SqliteConnection connection,
    SqliteTransaction transaction)
{
    if (connection == null)
        throw new ArgumentNullException(nameof(connection));

    if (transaction == null)
        throw new ArgumentNullException(nameof(transaction));

    if (string.IsNullOrWhiteSpace(wpsNumber))
        return null;

    using var cmd =
        connection.CreateCommand();

    cmd.Transaction = transaction;

    cmd.CommandText = @"
SELECT *
FROM Wps
WHERE WpsNumber = $wpsNumber
ORDER BY
    IsActive DESC,
    Revision DESC
LIMIT 1";

    cmd.Parameters.AddWithValue(
        "$wpsNumber",
        wpsNumber.Trim());

    using var reader =
        cmd.ExecuteReader();

    if (reader.Read())
    {
        return Map(reader);
    }

    return null;
}

        private Wps Map(SqliteDataReader reader)
        {
            string? Text(string name)
            {
                return reader[name] == DBNull.Value
                    ? null
                    : Convert.ToString(reader[name]);
            }

            double Number(string name)
            {
                return reader[name] == DBNull.Value
                    ? 0d
                    : Convert.ToDouble(reader[name]);
            }

            double? NullableNumber(string name)
            {
                return reader[name] == DBNull.Value
                    ? null
                    : Convert.ToDouble(reader[name]);
            }

            bool Flag(string name)
            {
                return reader[name] != DBNull.Value
                    && Convert.ToInt32(reader[name]) != 0;
            }

            Guid? NullableGuid(string name)
            {
                var value = Text(name);
                return Guid.TryParse(value, out var parsed)
                    ? parsed
                    : null;
            }

            DateTime? NullableDate(string name)
            {
                var value = Text(name);

                return DateTime.TryParse(
                    value,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.RoundtripKind,
                    out var parsed)
                        ? parsed
                        : null;
            }
            return new Wps
            {
                Id = NullableGuid("Id") ?? Guid.Empty,
                CompanyId = NullableGuid("CompanyId"),
                WpsNumber = Text("WpsNumber") ?? "",
                Process = Text("Process") ?? "",
                MaterialGroup = Text("MaterialGroup") ?? "",
                ThicknessMin = Number("ThicknessMin"),
                ThicknessMax = Number("ThicknessMax"),
                Diameter = Number("Diameter"),

                FillerMaterial = Text("FillerMaterial") ?? "",
                GasType = Text("GasType") ?? "",
                AmpsMin = Number("AmpsMin"),
                AmpsMax = Number("AmpsMax"),
                VoltsMin = Number("VoltsMin"),
                VoltsMax = Number("VoltsMax"),
                HeatInputMin = Number("HeatInputMin"),
                HeatInputMax = Number("HeatInputMax"),
                PreheatMin = Number("PreheatMin"),
                PreheatMax = Number("PreheatMax"),
                WelderNumber = Text("WelderNumber") ?? "",
                InterpassMax = NullableNumber("InterpassMax"),
                PwhtRequired = Flag("PwhtRequired"),

                PqrId = NullableGuid("PqrId"),
                LinkedPqrNumber = Text("LinkedPqrNumber"),
                PNumber = Text("PNumber"),
                FNumber = Text("FNumber"),
                Position = Text("Position"),
                JointType = Text("JointType"),
                JointDesign = Text("JointDesign") ?? "",
                Backing = Text("Backing") ?? "",
                BackingType = Text("BackingType") ?? "",

                IsApproved = Flag("IsApproved"),
                IsLocked = Flag("IsLocked"),
                ApprovedOn = NullableDate("ApprovedOn"),
                ApprovedBy = Text("ApprovedBy"),

                ParseConfidence = Number("ParseConfidence"),
                ParseNotes = Text("ParseNotes") ?? "",
                PreviousRevisionId = NullableGuid("PreviousRevisionId"),
                CreatedOn = NullableDate("CreatedOn") ?? DateTime.UtcNow,
                CreatedBy = Text("CreatedBy") ?? "",
                ImportSource = Text("ImportSource") ?? "",
                Revision = Convert.ToInt32(reader["Revision"]),
                IsActive = Flag("IsActive"),
                IsCompliant = Flag("IsCompliant"),
                ValidationStatus = Text("ValidationStatus") ?? "",

                CurrentType = Text("CurrentType") ?? "",
                Polarity = Text("Polarity") ?? "",
                Progression = Text("Progression") ?? "",
                JointDiagramPath = Text("JointDiagramPath") ?? "",
                PassDiagramPath = Text("PassDiagramPath") ?? "",
                QualifiedThicknessRange = Text("QualifiedThicknessRange") ?? ""
            };
        }
        public void Update(Wps wps)
        {
            if (wps == null)
                throw new ArgumentNullException(nameof(wps));

            if (string.IsNullOrWhiteSpace(wps.WpsNumber))
                throw new InvalidOperationException("WPS Number is required.");

            if (string.IsNullOrWhiteSpace(wps.Process))
                throw new InvalidOperationException("Process is required.");

            wps.WpsNumber = wps.WpsNumber.Trim().ToUpperInvariant();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            EnsureTables(connection);

            using var command = connection.CreateCommand();

            var updateColumns = FullWpsColumns
                .Where(column => !string.Equals(
                    column, "Id", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            command.CommandText =
                $"UPDATE Wps SET {string.Join(", ", Array.ConvertAll(updateColumns, c => c + " = $" + c))} " +
                "WHERE Id = $Id;";

            command.Parameters.AddWithValue("$Id", wps.Id.ToString());

            foreach (var column in updateColumns)
            {
                var property = typeof(Wps).GetProperty(column);

                if (property == null)
                {
                    throw new InvalidOperationException(
                        $"WPS persistence mapping is missing model property '{column}'.");
                }

                object? value = property.GetValue(wps);

                if (value is Guid guid)
                    value = guid.ToString();
                else if (value is DateTime date)
                    value = date.ToString("O");
                else if (value is bool flag)
                    value = flag ? 1 : 0;

                command.Parameters.AddWithValue(
                    "$" + column,
                    value ?? DBNull.Value);
            }

            var affected = command.ExecuteNonQuery();

            if (affected != 1)
            {
                throw new InvalidOperationException(
                    $"WPS '{wps.WpsNumber}' could not be updated. Verify that the record exists.");
            }
        }
        public void SaveRevision(
            Wps previous,
            Wps revision)
        {
            if (previous == null)
                throw new ArgumentNullException(nameof(previous));

            if (revision == null)
                throw new ArgumentNullException(nameof(revision));

            if (previous.Id == Guid.Empty)
                throw new InvalidOperationException("Previous WPS revision has no ID.");

            if (string.IsNullOrWhiteSpace(previous.WpsNumber))
                throw new InvalidOperationException("Previous WPS Number is required.");

            if (string.IsNullOrWhiteSpace(revision.WpsNumber))
                throw new InvalidOperationException("Revision WPS Number is required.");

            previous.WpsNumber = previous.WpsNumber.Trim().ToUpperInvariant();
            revision.WpsNumber = revision.WpsNumber.Trim().ToUpperInvariant();

            if (!string.Equals(
                    previous.WpsNumber,
                    revision.WpsNumber,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Previous and new WPS revisions must have the same WPS Number.");
            }

            if (revision.Id != Guid.Empty && revision.Id == previous.Id)
            {
                throw new InvalidOperationException(
                    "A new WPS revision must have a different ID from the previous revision.");
            }

            if (revision.Id == Guid.Empty)
                revision.Id = Guid.NewGuid();

            if (revision.PreviousRevisionId == null)
                revision.PreviousRevisionId = previous.Id;

            if (revision.CreatedOn == default)
                revision.CreatedOn = DateTime.UtcNow;

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            EnsureTables(connection);

            using var transaction = connection.BeginTransaction();

            try
            {
                using (var deactivate = connection.CreateCommand())
                {
                    deactivate.Transaction = transaction;
                    deactivate.CommandText = @"
UPDATE Wps
SET IsActive = 0
WHERE Id = $id;";

                    deactivate.Parameters.AddWithValue(
                        "$id",
                        previous.Id.ToString());

                    var affected = deactivate.ExecuteNonQuery();

                    if (affected != 1)
                    {
                        throw new InvalidOperationException(
                            "The previous WPS revision could not be deactivated.");
                    }
                }

                InsertFullWps(connection, transaction, revision);

                transaction.Commit();
            }
            catch
            {
                try
                {
                    transaction.Rollback();
                }
                catch
                {
                    // Preserve the original exception if rollback also fails.
                }

                throw;
            }
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





