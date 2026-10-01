using Dapper;
using Microsoft.Data.Sqlite;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Data.Repositories;

public class ProjectQualityControlPlanItemApprovalHistoryRepository
{
    private readonly string _connectionString;

    public ProjectQualityControlPlanItemApprovalHistoryRepository(
        string connectionString)
    {
        _connectionString =
            connectionString;
    }

    public void Add(
        ProjectQualityControlPlanItemApprovalHistory entry)
    {
        using var connection =
            new SqliteConnection(
                _connectionString);

        connection.Execute(
            @"
INSERT INTO ProjectQualityControlPlanItemApprovalHistory
(
    Id,
    ProjectQualityControlPlanItemId,
    Status,
    Action,
    ApprovedBy,
    ApprovedOn,
    Remarks
)
VALUES
(
    @Id,
    @ProjectQualityControlPlanItemId,
    @Status,
    @Action,
    @ApprovedBy,
    @ApprovedOn,
    @Remarks
)",
            new
            {
                Id =
                    entry.Id.ToString(),

                ProjectQualityControlPlanItemId =
                    entry.ProjectQualityControlPlanItemId.ToString(),

                Status =
                    (int)entry.Status,

                entry.Action,
                entry.ApprovedBy,
                entry.ApprovedOn,
                entry.Remarks
            });
    }

    public List<ProjectQualityControlPlanItemApprovalHistory> GetByItem(
        Guid projectQualityControlPlanItemId)
    {
        using var connection =
            new SqliteConnection(
                _connectionString);

        var rows =
            connection.Query(
                @"
SELECT
    Id,
    ProjectQualityControlPlanItemId,
    Status,
    Action,
    ApprovedBy,
    ApprovedOn,
    Remarks
FROM ProjectQualityControlPlanItemApprovalHistory
WHERE ProjectQualityControlPlanItemId =
      @ProjectQualityControlPlanItemId
ORDER BY ApprovedOn,
         rowid",
                new
                {
                    ProjectQualityControlPlanItemId =
                        projectQualityControlPlanItemId.ToString()
                });

        return rows
            .Select(Map)
            .ToList();
    }

    private static ProjectQualityControlPlanItemApprovalHistory Map(
        dynamic row)
    {
        return new ProjectQualityControlPlanItemApprovalHistory
        {
            Id =
                Guid.Parse(
                    row.Id.ToString()),

            ProjectQualityControlPlanItemId =
                Guid.Parse(
                    row.ProjectQualityControlPlanItemId.ToString()),

            Status =
                (QcpApprovalStatus)
                    Convert.ToInt32(
                        row.Status),

            Action =
                row.Action?.ToString()
                ?? string.Empty,

            ApprovedBy =
                row.ApprovedBy?.ToString()
                ?? string.Empty,

            ApprovedOn =
                Convert.ToDateTime(
                    row.ApprovedOn),

            Remarks =
                row.Remarks?.ToString()
                ?? string.Empty
        };
    }
}
