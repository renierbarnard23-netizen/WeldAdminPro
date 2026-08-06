using Microsoft.Data.Sqlite;
using System.IO;
using WeldAdminPro.Core.Interfaces;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Services;

namespace WeldAdminPro.Data.Repositories
{
    public class WeldRepository : IWeldRepository
    {
        private readonly string _connectionString;

        public WeldRepository()
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

            UpgradeDatabase();
            CreateIndexes();
        }

        public WeldRepository(string connectionString)
        {
            _connectionString = connectionString;

            UpgradeDatabase();
            CreateIndexes();
        }
        // =====================================================
        // ADD WELD
        // =====================================================

        public async Task AddAsync(Weld weld)
        {
            if (weld.Id == Guid.Empty)
            {
                weld.Id = Guid.NewGuid();
            }

            if (weld.ProjectId == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "Weld must belong to a project.");
            }

            if (string.IsNullOrWhiteSpace(
                    weld.WeldNumber))
            {
                weld.WeldNumber =
                    await GetNextWeldNumberAsync(
                        weld.ProjectId);
            }

            if (string.IsNullOrWhiteSpace(
                    weld.WpsNumber))
            {
                throw new InvalidOperationException(
                    "WPS Number is required.");
            }

            if (string.IsNullOrWhiteSpace(
                    weld.WelderNumber))
            {
                throw new InvalidOperationException(
                    "Welder Number is required.");
            }

            if (weld.Thickness < 0)
            {
                throw new InvalidOperationException(
                    "Thickness cannot be negative.");
            }

            if (await ExistsAsync(
                weld.ProjectId,
                weld.WeldNumber))
            {
                throw new InvalidOperationException(
                    $"Weld '{weld.WeldNumber}' already exists.");
            }

            using var connection =
                new SqliteConnection(
                    _connectionString);

            if (weld.CreatedDate == default)
            {
                weld.CreatedDate = DateTime.UtcNow;
            }

            var readiness =
                _readinessEngine
                    .Evaluate(weld);

            weld.ReadinessScore =
                readiness.ReadinessScore;

            weld.IsReady =
                readiness.IsReady;

            weld.ReadinessSummary =
                string.Join(
                    Environment.NewLine,
                    readiness.BlockingReasons);

            await connection.OpenAsync();

            var cmd =
                connection.CreateCommand();

            cmd.CommandText = @"
INSERT INTO Welds
(
    Id,
    ProjectId,
    WeldNumber,
    JointNumber,
    DrawingNumber,
    MaterialSpecification,
    Diameter,
    JointType,
    WeldType,
    WpsNumber,
    WelderNumber,

    Process,
    MaterialGroup,
    Position,
    Thickness,

    MaterialHeat1,
    MaterialHeat2,

    Status,

    WorkflowStatus,

    NdtStatus,

    RepairCount,
    RepairCycle,
    RequiresRepair,

    LastNdtDate,
    LastNdtResult,
    NdtPendingDate,

    IsValid,
    ValidationMessage,

    CreatedDate,
    RequiredNdt,
    ReadinessScore,
    IsReady
)
VALUES
(
    $Id,
    $ProjectId,
    $WeldNumber,
    $JointNumber,
    $DrawingNumber,
    $MaterialSpecification,
    $Diameter,
    $JointType,
    $WeldType,
    $WpsNumber,
    $WelderNumber,

    $Process,
    $MaterialGroup,
    $Position,
    $Thickness,

    $MaterialHeat1,
    $MaterialHeat2,

    $Status,
    $WorkflowStatus,
    $NdtStatus,

    $RepairCount,
    $RepairCycle,
    $RequiresRepair,

    $LastNdtDate,
    $LastNdtResult,
    $NdtPendingDate,

    $IsValid,
    $ValidationMessage,

    $CreatedDate,
    $RequiredNdt,
    $ReadinessScore,
    $IsReady
);";

            cmd.Parameters.AddWithValue(
                "$Id",
                weld.Id.ToString());

            cmd.Parameters.AddWithValue(
                "$ProjectId",
                weld.ProjectId.ToString());

            cmd.Parameters.AddWithValue(
                "$WeldNumber",
                weld.WeldNumber);

            cmd.Parameters.AddWithValue(
                "$JointNumber",
                weld.JointNumber);

            cmd.Parameters.AddWithValue(
                "$MaterialSpecification",
                weld.MaterialSpecification);

            cmd.Parameters.AddWithValue(
                "$Diameter",
                weld.Diameter);

            cmd.Parameters.AddWithValue(
                "$DrawingNumber",
                weld.DrawingNumber);

            cmd.Parameters.AddWithValue(
                "$JointType",
                weld.JointType);

            cmd.Parameters.AddWithValue(
                "$WeldType",
                weld.WeldType);

            cmd.Parameters.AddWithValue(
                "$WpsNumber",
                weld.WpsNumber);

            cmd.Parameters.AddWithValue(
                "$WelderNumber",
                weld.WelderNumber);

            cmd.Parameters.AddWithValue(
                "$Process",
                weld.Process);

            cmd.Parameters.AddWithValue(
                "$MaterialGroup",
                weld.MaterialGroup);

            cmd.Parameters.AddWithValue(
                "$Position",
                weld.Position);

            cmd.Parameters.AddWithValue(
                "$Thickness",
                weld.Thickness);

            cmd.Parameters.AddWithValue(
                "$MaterialHeat1",
                weld.MaterialHeat1);

            cmd.Parameters.AddWithValue(
                "$MaterialHeat2",
                weld.MaterialHeat2);

            cmd.Parameters.AddWithValue(
                "$Status",
                weld.Status.ToString());

            cmd.Parameters.AddWithValue(
                "$WorkflowStatus",
                weld.WorkflowStatus.ToString());

            cmd.Parameters.AddWithValue(
                "$NdtStatus",
                weld.NdtStatus);

            cmd.Parameters.AddWithValue(
                "$RepairCount",
                weld.RepairCount);

            cmd.Parameters.AddWithValue(
                "$RepairCycle",
                weld.RepairCycle);

            cmd.Parameters.AddWithValue(
                "$RequiresRepair",
                weld.RequiresRepair ? 1 : 0);

            cmd.Parameters.AddWithValue(
                "$LastNdtDate",
                weld.LastNdtDate != null
                    ? weld.LastNdtDate.Value.ToString("O")
                    : DBNull.Value);

            cmd.Parameters.AddWithValue(
                "$LastNdtResult",
                string.IsNullOrWhiteSpace(
                    weld.LastNdtResult)
                    ? DBNull.Value
                    : weld.LastNdtResult);

            cmd.Parameters.AddWithValue(
                "$NdtPendingDate",
                weld.NdtPendingDate.HasValue
                    ? weld.NdtPendingDate.Value.ToString("O")
                    : DBNull.Value);

            cmd.Parameters.AddWithValue(
                "$IsValid",
                weld.IsValid ? 1 : 0);

            cmd.Parameters.AddWithValue(
                "$ValidationMessage",
                weld.ValidationMessage);

            cmd.Parameters.AddWithValue(
                "$CreatedDate",
                weld.CreatedDate.ToString("O"));

            cmd.Parameters.AddWithValue(
                "$RequiredNdt",
                weld.RequiredNdt);

            cmd.Parameters.AddWithValue(
                "$ReadinessScore",
                weld.ReadinessScore);

            cmd.Parameters.AddWithValue(
                "$IsReady",
                weld.IsReady ? 1 : 0);

            await cmd.ExecuteNonQueryAsync();
        }

        private readonly IWeldReadinessEngine
    _readinessEngine =
        new WeldReadinessEngine();

        private async Task<bool> ExistsAsync(
    Guid projectId,
    string weldNumber)
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            await connection.OpenAsync();

            using var cmd =
                connection.CreateCommand();

            cmd.CommandText = @"
SELECT COUNT(*)
FROM Welds
WHERE ProjectId = $ProjectId
AND WeldNumber = $WeldNumber;";

            cmd.Parameters.AddWithValue(
                "$ProjectId",
                projectId.ToString());

            cmd.Parameters.AddWithValue(
                "$WeldNumber",
                weldNumber);

            return Convert.ToInt32(
                await cmd.ExecuteScalarAsync())
                > 0;
        }

        // =====================================================
        // GET WELDS
        // =====================================================

        public async Task<List<Weld>> GetByProjectAsync(
            Guid projectId)
        {
            var welds = new List<Weld>();

            using var connection =
                new SqliteConnection(_connectionString);

            await connection.OpenAsync();

            var cmd = connection.CreateCommand();

            cmd.CommandText = @"
SELECT *
FROM Welds
WHERE ProjectId = $ProjectId
ORDER BY WeldNumber;";

            cmd.Parameters.AddWithValue(
                "$ProjectId",
                projectId.ToString());

            using var reader =
                await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                welds.Add(new Weld
                {
                    Id = Guid.Parse(
                        reader["Id"].ToString()!),

                    ProjectId = Guid.Parse(
                        reader["ProjectId"].ToString()!),

                    WeldNumber =
                        reader["WeldNumber"]?.ToString() ?? "",

                    JointNumber =
                        reader["JointNumber"]?.ToString() ?? "",

                    DrawingNumber =
                        reader["DrawingNumber"]?.ToString() ?? "",

                    MaterialSpecification =
                        reader["MaterialSpecification"]?.ToString() ?? "",

                    Diameter =
                        reader["Diameter"] == DBNull.Value
                        ? 0
                            : Convert.ToDouble(
                        reader["Diameter"]),

                    JointType =
                        reader["JointType"]?.ToString() ?? "",

                    WeldType =
                        reader["WeldType"]?.ToString() ?? "",

                    WpsNumber =
                        reader["WpsNumber"]?.ToString() ?? "",

                    WelderNumber =
                        reader["WelderNumber"]?.ToString() ?? "",

                    MaterialHeat1 =
                        reader["MaterialHeat1"]?.ToString() ?? "",

                    MaterialHeat2 =
                        reader["MaterialHeat2"]?.ToString() ?? "",

                    Status =
                        Enum.TryParse<WeldStatusType>(
                            reader["Status"]?.ToString(),
                            out var status)
                                ? status
                                : WeldStatusType.Pending,

                    WorkflowStatus =
                        Enum.TryParse<WeldWorkflowStatus>(
                            reader["WorkflowStatus"]
                                ?.ToString(),
                            out var workflowStatus)
                                ? workflowStatus
                                : WeldWorkflowStatus.Draft,

                    NdtStatus =
                        reader["NdtStatus"]?.ToString() ?? "",

                    RepairCount =
                        reader["RepairCount"] == DBNull.Value
                            ? 0
                            : Convert.ToInt32(
                                reader["RepairCount"]),

                    RepairCycle =
                        reader["RepairCycle"] == DBNull.Value
                            ? 0
                            : Convert.ToInt32(
                                reader["RepairCycle"]),

                    RequiresRepair =
                        reader["RequiresRepair"] != DBNull.Value
                        && Convert.ToInt32(
                            reader["RequiresRepair"]) == 1,

                    LastNdtDate =
                        reader["LastNdtDate"] == DBNull.Value
                            ? null
                            : DateTime.Parse(
                                reader["LastNdtDate"].ToString()!),

                    LastNdtResult =
                        reader["LastNdtResult"]?.ToString(),

                    CreatedDate =
                        DateTime.Parse(
                            reader["CreatedDate"].ToString()!),

                    RequiredNdt =
                            reader["RequiredNdt"]?.ToString() ?? "",

                    ReadinessScore =
                            reader["ReadinessScore"] == DBNull.Value
                                ? 0
                                : Convert.ToInt32(
                            reader["ReadinessScore"]),

                    IsReady =
                            reader["IsReady"] != DBNull.Value
                                &&
                                Convert.ToInt32(
                            reader["IsReady"]) == 1,

                    ReadinessSummary =
                            reader["ReadinessSummary"]?.ToString() ?? "",

                    ReleaseReady =
                        reader["ReleaseReady"] != DBNull.Value
                        &&
                        Convert.ToInt32(
                            reader["ReleaseReady"]) == 1,

                    TurnoverReady =
                        reader["TurnoverReady"] != DBNull.Value
                        &&
                        Convert.ToInt32(
                            reader["TurnoverReady"]) == 1,

                    BlockingCount =
                        reader["BlockingCount"] == DBNull.Value
                            ? 0
                            : Convert.ToInt32(
                        reader["BlockingCount"]),

                    ReleasedBy =
                        reader["ReleasedBy"]?.ToString() ?? "",

                    ReleasedDate =
                        reader["ReleasedDate"] == DBNull.Value
                            ? null
                            : DateTime.Parse(
                                reader["ReleasedDate"].ToString()!),

                    IsReleased =
                        reader["IsReleased"] != DBNull.Value
                        &&
                        Convert.ToInt32(
                            reader["IsReleased"]) == 1,

                    RequiredReleaseRole =
                        reader["RequiredReleaseRole"] == DBNull.Value
                            ? WeldReleaseRole.QA
                            : (WeldReleaseRole)
                                Convert.ToInt32(
                                    reader["RequiredReleaseRole"]),

                    Process =
                        reader["Process"]?.ToString() ?? "",

                    MaterialGroup =
                        reader["MaterialGroup"]?.ToString() ?? "",

                    Position =
                        reader["Position"]?.ToString() ?? "",

                    Thickness =
                        reader["Thickness"] == DBNull.Value
                            ? 0
                            : Convert.ToDouble(
                                reader["Thickness"]),

                    IsValid =
                        reader["IsValid"] != DBNull.Value
                        && Convert.ToInt32(
                            reader["IsValid"]) == 1,

                    ValidationMessage =
                        reader["ValidationMessage"]?.ToString() ?? ""
                });
            }

            return welds;
        }

        // =====================================================
        // NEXT WELD NUMBER
        // =====================================================

        // =====================================================
        // GET WELD BY ID
        // =====================================================

        public async Task<Weld?> GetByIdAsync(
            Guid weldId)
        {
            using var connection =
                new SqliteConnection(_connectionString);

            await connection.OpenAsync();

            var cmd = connection.CreateCommand();

            cmd.CommandText = @"
SELECT *
FROM Welds
WHERE Id = $WeldId
LIMIT 1;";

            cmd.Parameters.AddWithValue(
                "$WeldId",
                weldId.ToString());

            using var reader =
                await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return null;
            }

            return new Weld
            {
                Id = Guid.Parse(
                    reader["Id"].ToString()!),

                ProjectId = Guid.Parse(
                    reader["ProjectId"].ToString()!),

                WeldNumber =
                    reader["WeldNumber"]?.ToString() ?? "",

                JointNumber =
                    reader["JointNumber"]?.ToString() ?? "",

                DrawingNumber =
                    reader["DrawingNumber"]?.ToString() ?? "",

                MaterialSpecification =
                    reader["MaterialSpecification"]?.ToString() ?? "",

                Diameter =
                    reader["Diameter"] == DBNull.Value
                        ? 0
                        : Convert.ToDouble(
                            reader["Diameter"]),

                JointType =
                    reader["JointType"]?.ToString() ?? "",

                WeldType =
                    reader["WeldType"]?.ToString() ?? "",

                WpsNumber =
                    reader["WpsNumber"]?.ToString() ?? "",

                WelderNumber =
                    reader["WelderNumber"]?.ToString() ?? "",

                MaterialHeat1 =
                    reader["MaterialHeat1"]?.ToString() ?? "",

                MaterialHeat2 =
                    reader["MaterialHeat2"]?.ToString() ?? "",

                Status =
                    Enum.TryParse<WeldStatusType>(
                        reader["Status"]?.ToString(),
                        out var status)
                            ? status
                            : WeldStatusType.Pending,

                WorkflowStatus =
                    Enum.TryParse<WeldWorkflowStatus>(
                        reader["WorkflowStatus"]?.ToString(),
                        out var workflowStatus)
                            ? workflowStatus
                            : WeldWorkflowStatus.Draft,

                NdtStatus =
                    reader["NdtStatus"]?.ToString() ?? "",

                RepairCount =
                    reader["RepairCount"] == DBNull.Value
                        ? 0
                        : Convert.ToInt32(
                            reader["RepairCount"]),

                RepairCycle =
                    reader["RepairCycle"] == DBNull.Value
                        ? 0
                        : Convert.ToInt32(
                            reader["RepairCycle"]),

                RequiresRepair =
                    reader["RequiresRepair"] != DBNull.Value
                    && Convert.ToInt32(
                        reader["RequiresRepair"]) == 1,

                LastNdtDate =
                    reader["LastNdtDate"] == DBNull.Value
                        ? null
                        : DateTime.Parse(
                            reader["LastNdtDate"].ToString()!),

                LastNdtResult =
                    reader["LastNdtResult"]?.ToString(),

                CreatedDate =
                    reader["CreatedDate"] == DBNull.Value
                        ? DateTime.UtcNow
                        : DateTime.Parse(
                            reader["CreatedDate"].ToString()!),

                RequiredNdt =
                    HasColumn(reader, "RequiredNdt")
                        ? reader["RequiredNdt"]?.ToString() ?? ""
                        : "",

                ReadinessScore =
                    HasColumn(reader, "ReadinessScore")
                    && reader["ReadinessScore"] != DBNull.Value
                        ? Convert.ToInt32(
                            reader["ReadinessScore"])
                        : 0,

                IsReady =
                    HasColumn(reader, "IsReady")
                    && reader["IsReady"] != DBNull.Value
                    && Convert.ToInt32(
                        reader["IsReady"]) == 1,

                ReadinessSummary =
                    reader["ReadinessSummary"]?.ToString() ?? "",

                ReleaseReady =
                    HasColumn(reader, "ReleaseReady")
                    && reader["ReleaseReady"] != DBNull.Value
                    && Convert.ToInt32(
                        reader["ReleaseReady"]) == 1,

                TurnoverReady =
                    reader["TurnoverReady"] != DBNull.Value
                    && Convert.ToInt32(
                        reader["TurnoverReady"]) == 1,

                BlockingCount =
                    reader["BlockingCount"] == DBNull.Value
                        ? 0
                        : Convert.ToInt32(
                            reader["BlockingCount"]),

                ReleasedBy =
                    reader["ReleasedBy"]?.ToString() ?? "",

                ReleasedDate =
                    reader["ReleasedDate"] == DBNull.Value
                        ? null
                        : DateTime.Parse(
                            reader["ReleasedDate"].ToString()!),

                IsReleased =
                    reader["IsReleased"] != DBNull.Value
                    && Convert.ToInt32(
                        reader["IsReleased"]) == 1,

                RequiredReleaseRole =
                    reader["RequiredReleaseRole"] == DBNull.Value
                        ? WeldReleaseRole.QA
                        : (WeldReleaseRole)
                            Convert.ToInt32(
                                reader["RequiredReleaseRole"]),

                Process =
                    reader["Process"]?.ToString() ?? "",

                MaterialGroup =
                    reader["MaterialGroup"]?.ToString() ?? "",

                Position =
                    reader["Position"]?.ToString() ?? "",

                Thickness =
                    reader["Thickness"] == DBNull.Value
                        ? 0
                        : Convert.ToDouble(
                            reader["Thickness"]),

                IsValid =
                    reader["IsValid"] != DBNull.Value
                    && Convert.ToInt32(
                        reader["IsValid"]) == 1,

                ValidationMessage =
                    reader["ValidationMessage"]?.ToString() ?? ""
            };
        }

        public async Task<string> GetNextWeldNumberAsync(
            Guid projectId)
        {
            using var connection =
                new SqliteConnection(_connectionString);

            await connection.OpenAsync();

            var cmd = connection.CreateCommand();

            cmd.CommandText = @"
SELECT WeldNumber
FROM Welds
WHERE ProjectId = $ProjectId;";

            cmd.Parameters.AddWithValue(
                "$ProjectId",
                projectId.ToString());

            using var reader =
                await cmd.ExecuteReaderAsync();

            var maxNumber = 0;

            while (await reader.ReadAsync())
            {
                var weldNumber =
                    reader["WeldNumber"]?.ToString();

                if (string.IsNullOrWhiteSpace(
                    weldNumber))
                    continue;

                if (weldNumber.StartsWith("W-"))
                {
                    var numberPart =
                        weldNumber.Replace("W-", "");

                    if (int.TryParse(
                        numberPart,
                        out int number))
                    {
                        if (number > maxNumber)
                        {
                            maxNumber = number;
                        }
                    }
                }
            }

            return $"W-{(maxNumber + 1):000}";
        }

        public List<Weld> GetAll()
        {
            var welds =
            new List<Weld>();

            using var connection =
                new SqliteConnection(
                    _connectionString);

            connection.Open();

            var command =
                connection.CreateCommand();

            command.CommandText = @"
                SELECT *
                FROM Welds
                ORDER BY CreatedDate DESC;";

            using var reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                welds.Add(
                    new Weld
                    {
                        Id =
                            Guid.Parse(
                                reader["Id"].ToString()!),

                        ProjectId =
                            Guid.Parse(
                                reader["ProjectId"].ToString()!),

                        WeldNumber =
                            reader["WeldNumber"]
                                ?.ToString() ?? "",

                        JointNumber =
                            reader["JointNumber"]?.ToString() ?? "",

                        DrawingNumber =
                            reader["DrawingNumber"]?.ToString() ?? "",

                        MaterialSpecification =
                            reader["MaterialSpecification"]?.ToString() ?? "",

                        Diameter =
                            reader["Diameter"] == DBNull.Value
                            ? 0
                                : Convert.ToDouble(
                            reader["Diameter"]),

                        WpsNumber =
                            reader["WpsNumber"]
                                ?.ToString() ?? "",

                        WelderNumber =
                            reader["WelderNumber"]
                                ?.ToString() ?? "",

                        MaterialGroup =
                            reader["MaterialGroup"]
                                ?.ToString() ?? "",

                        Process =
                            reader["Process"]
                                ?.ToString() ?? "",

                        Position =
                            reader["Position"]
                                ?.ToString() ?? "",

                        JointType =
                            reader["JointType"]
                                ?.ToString() ?? "",

                        WeldType =
                            reader["WeldType"]?.ToString() ?? "",

                        NdtStatus =
                            reader["NdtStatus"]
                                ?.ToString() ?? "",

                        LastNdtDate =
                            reader["LastNdtDate"] == DBNull.Value
                                ? null
                                : DateTime.Parse(
                            reader["LastNdtDate"].ToString()!),

                        NdtPendingDate =
                            HasColumn(reader, "NdtPendingDate")
                            && reader["NdtPendingDate"] != DBNull.Value
                                ? DateTime.Parse(
                                reader["NdtPendingDate"].ToString()!)
                                : null,

                        CreatedDate =
                            reader["CreatedDate"] == DBNull.Value
                                ? DateTime.UtcNow
                                : DateTime.Parse(
                            reader["CreatedDate"].ToString()!),

                        RequiredNdt =
                            HasColumn(reader, "RequiredNdt")
                                ? reader["RequiredNdt"]?.ToString() ?? ""
                                : "",

                        ReadinessScore =
                            HasColumn(reader, "ReadinessScore")
                            &&
                            reader["ReadinessScore"] != DBNull.Value
                            ? Convert.ToInt32(
                                reader["ReadinessScore"])
                            : 0,

                        IsReady =
                            HasColumn(reader, "IsReady")
                            &&
                            reader["IsReady"] != DBNull.Value
                            &&
                                Convert.ToInt32(
                            reader["IsReady"]) == 1,

                        ReleaseReady =
                            HasColumn(reader, "ReleaseReady")
                            &&
                                reader["ReleaseReady"] != DBNull.Value
                            &&
                            Convert.ToInt32(reader["ReleaseReady"]) == 1,

                        ReadinessSummary =
                            reader["ReadinessSummary"]?.ToString() ?? "",

                        RepairCount =
                            reader["RepairCount"] == DBNull.Value
                                ? 0
                                : Convert.ToInt32(
                                    reader["RepairCount"]),

                        Status =
                            Enum.TryParse<WeldStatusType>(
                                reader["Status"]
                                    ?.ToString(),
                                out var status)
                                    ? status
                                    : WeldStatusType.Pending,

                        WorkflowStatus =
                            Enum.TryParse<WeldWorkflowStatus>(
                                reader["WorkflowStatus"]
                                    ?.ToString(),
                            out var workflowStatus)
                                ? workflowStatus
                                : WeldWorkflowStatus.Draft,
                    });
            }

            return welds;
        }

        private bool HasColumn(
    SqliteDataReader reader,
    string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i)
                    .Equals(columnName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }



        // =====================================================
        // UPDATE WELD
        // =====================================================

        public async Task UpdateAsync(Weld weld)
        {

            var readiness =
                _readinessEngine
                    .Evaluate(weld);

            weld.ReadinessScore =
                readiness.ReadinessScore;

            weld.IsReady =
                readiness.IsReady;

            weld.ReadinessSummary =
                string.Join(
                    Environment.NewLine,
                    readiness.BlockingReasons);

            if (weld.Id == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "Weld Id cannot be empty.");
            }

            using var connection =
                new SqliteConnection(_connectionString);

            await connection.OpenAsync();

            var cmd = connection.CreateCommand();

            cmd.CommandText = @"
UPDATE Welds
SET
    DrawingNumber = $DrawingNumber,
    JointNumber = $JointNumber,

    Status = $Status,
    JointType = $JointType,
    WeldType = $WeldType,
    WorkflowStatus = $WorkflowStatus,
    NdtStatus = $NdtStatus,
    RepairCount = $RepairCount,
    RepairCycle = $RepairCycle,
    RequiresRepair = $RequiresRepair,
    LastNdtDate = $LastNdtDate,
    LastNdtResult = $LastNdtResult,
    NdtPendingDate = $NdtPendingDate,

    ReadinessScore = $ReadinessScore,
    IsReady = $IsReady,
    ReadinessSummary = $ReadinessSummary
WHERE Id = $Id;";

            cmd.Parameters.AddWithValue(
                "$DrawingNumber",
                weld.DrawingNumber);

            cmd.Parameters.AddWithValue(
                "$JointNumber",
                weld.JointNumber);

            cmd.Parameters.AddWithValue(
                "$Status",
                weld.Status.ToString());

            cmd.Parameters.AddWithValue(
                "$JointType",
                weld.JointType);

            cmd.Parameters.AddWithValue(
                "$WeldType",
                weld.WeldType);

            cmd.Parameters.AddWithValue(
                "$WorkflowStatus",
                weld.WorkflowStatus.ToString());

            cmd.Parameters.AddWithValue(
                "$NdtStatus",
                weld.NdtStatus);

            cmd.Parameters.AddWithValue(
                "$RepairCount",
                weld.RepairCount);

            cmd.Parameters.AddWithValue(
                "$RepairCycle",
                weld.RepairCycle);

            cmd.Parameters.AddWithValue(
                "$RequiresRepair",
                weld.RequiresRepair ? 1 : 0);

            cmd.Parameters.AddWithValue(
                "$LastNdtDate",
                weld.LastNdtDate != null
                    ? weld.LastNdtDate.Value.ToString("O")
                    : DBNull.Value);

            cmd.Parameters.AddWithValue(
                "$LastNdtResult",
                string.IsNullOrWhiteSpace(
                    weld.LastNdtResult)
                    ? DBNull.Value
                    : weld.LastNdtResult);

            cmd.Parameters.AddWithValue(
                "$NdtPendingDate",
                    weld.NdtPendingDate.HasValue
                        ? weld.NdtPendingDate.Value.ToString("O")
                        : DBNull.Value);

            cmd.Parameters.AddWithValue(
                "$Id",
                weld.Id.ToString());

            cmd.Parameters.AddWithValue(
                "$ReadinessScore",
                weld.ReadinessScore);

            cmd.Parameters.AddWithValue(
                "$IsReady",
                weld.IsReady ? 1 : 0);

            cmd.Parameters.AddWithValue(
                "$ReadinessSummary",
                weld.ReadinessSummary);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteAsync(Guid weldId)
        {
            using var connection =
                new SqliteConnection(_connectionString);

            await connection.OpenAsync();

            var cmd = connection.CreateCommand();

            cmd.CommandText = @"
DELETE FROM Welds
WHERE Id = $Id;";

            cmd.Parameters.AddWithValue(
                "$Id",
                weldId.ToString());

            await cmd.ExecuteNonQueryAsync();
        }

        private void UpgradeDatabase()
        {
            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            TryAddColumn(
                connection,
                "ALTER TABLE Welds ADD COLUMN JointNumber TEXT");

            TryAddColumn(
                connection,
                "ALTER TABLE Welds ADD COLUMN MaterialSpecification TEXT");

            TryAddColumn(
                connection,
                "ALTER TABLE Welds ADD COLUMN Diameter REAL");

            TryAddColumn(
                connection,
                "ALTER TABLE Welds ADD COLUMN NdtPendingDate TEXT");

            TryAddColumn(
                connection,
                "ALTER TABLE Welds ADD COLUMN ReleaseReady INTEGER DEFAULT 0");

            TryAddColumn(
                connection,
                "ALTER TABLE Welds ADD COLUMN TurnoverReady INTEGER DEFAULT 0");

            TryAddColumn(
                connection,
                "ALTER TABLE Welds ADD COLUMN BlockingCount INTEGER DEFAULT 0");

            TryAddColumn(
                connection,
                "ALTER TABLE Welds ADD COLUMN ReadinessSummary TEXT");

            TryAddColumn(
                connection,
                "ALTER TABLE Welds ADD COLUMN ReleasedBy TEXT");

            TryAddColumn(
                connection,
                "ALTER TABLE Welds ADD COLUMN ReleasedDate TEXT");

            TryAddColumn(
                connection,
                "ALTER TABLE Welds ADD COLUMN IsReleased INTEGER DEFAULT 0");

            TryAddColumn(
                connection,
                "ALTER TABLE Welds ADD COLUMN RequiredReleaseRole INTEGER DEFAULT 0");

            TryAddColumn(
                connection,
                "ALTER TABLE Welds ADD COLUMN RequiredNdt TEXT");

            TryAddColumn(
                connection,
                "ALTER TABLE Welds ADD COLUMN ReadinessScore INTEGER DEFAULT 0");

            TryAddColumn(
                connection,
                "ALTER TABLE Welds ADD COLUMN IsReady INTEGER DEFAULT 0");

        }
        private void CreateIndexes()
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            connection.Open();

            using var cmd =
                connection.CreateCommand();

            cmd.CommandText = @"
CREATE INDEX IF NOT EXISTS
IX_Welds_ProjectId
ON Welds(ProjectId);

CREATE INDEX IF NOT EXISTS
IX_Welds_WeldNumber
ON Welds(WeldNumber);

CREATE INDEX IF NOT EXISTS
IX_Welds_Status
ON Welds(Status);

CREATE INDEX IF NOT EXISTS
IX_Welds_WorkflowStatus
ON Welds(WorkflowStatus);

CREATE INDEX IF NOT EXISTS
IX_Welds_WpsNumber
ON Welds(WpsNumber);

CREATE INDEX IF NOT EXISTS
IX_Welds_WelderNumber
ON Welds(WelderNumber);";

            cmd.ExecuteNonQuery();
        }


        private void TryAddColumn(
            SqliteConnection connection,
            string sql)
        {
            try
            {
                using var cmd =
                    connection.CreateCommand();

                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
            catch
            {
                // Column already exists
            }
        }
    } }
