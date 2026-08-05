using Dapper;
using Microsoft.Data.Sqlite;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Data.Repositories;

public class NcrWorkflowHistoryRepository
{
    private readonly string _connectionString;

    public NcrWorkflowHistoryRepository(
        string connectionString)
    {
        _connectionString =
            connectionString;
    }

    public void Add(
        NcrWorkflowHistoryEntry entry)
    {
        using var connection =
            new SqliteConnection(
                _connectionString);

        connection.Execute(
            @"
INSERT INTO NcrWorkflowHistory
(
    Id,
    NcrId,
    FromStatus,
    ToStatus,
    Action,
    PerformedBy,
    PerformedDate,
    Details
)
VALUES
(
    @Id,
    @NcrId,
    @FromStatus,
    @ToStatus,
    @Action,
    @PerformedBy,
    @PerformedDate,
    @Details
)",
            new
            {
                Id =
                    entry.Id.ToString(),

                NcrId =
                    entry.NcrId.ToString(),

                FromStatus =
                    entry.FromStatus.HasValue
                        ? (int)entry.FromStatus.Value
                        : (int?)null,

                ToStatus =
                    (int)entry.ToStatus,

                entry.Action,
                entry.PerformedBy,
                entry.PerformedDate,
                entry.Details
            });
    }

    public List<NcrWorkflowHistoryEntry> GetByNcr(
        Guid ncrId)
    {
        using var connection =
            new SqliteConnection(
                _connectionString);

        var rows =
            connection.Query(
                @"
SELECT
    Id,
    NcrId,
    FromStatus,
    ToStatus,
    Action,
    PerformedBy,
    PerformedDate,
    Details
FROM NcrWorkflowHistory
WHERE NcrId = @NcrId
ORDER BY PerformedDate,
         rowid",
                new
                {
                    NcrId =
                        ncrId.ToString()
                });

        return rows
            .Select(Map)
            .ToList();
    }

    private static NcrWorkflowHistoryEntry Map(
        dynamic row)
    {
        return new NcrWorkflowHistoryEntry
        {
            Id =
                Guid.Parse(
                    row.Id.ToString()),

            NcrId =
                Guid.Parse(
                    row.NcrId.ToString()),

            FromStatus =
                row.FromStatus == null
                    ? null
                    : (NcrStatus?)
                        Convert.ToInt32(
                            row.FromStatus),

            ToStatus =
                (NcrStatus)
                    Convert.ToInt32(
                        row.ToStatus),

            Action =
                row.Action?.ToString()
                ?? string.Empty,

            PerformedBy =
                row.PerformedBy?.ToString()
                ?? string.Empty,

            PerformedDate =
                Convert.ToDateTime(
                    row.PerformedDate),

            Details =
                row.Details?.ToString()
                ?? string.Empty
        };
    }
}
