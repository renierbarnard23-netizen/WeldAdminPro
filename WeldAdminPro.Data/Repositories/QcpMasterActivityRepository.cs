using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Microsoft.Data.Sqlite;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Data.Repositories;

public class QcpMasterActivityRepository
{
    private readonly string _connectionString;

    public QcpMasterActivityRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void Add(QcpMasterActivity activity)
    {
        using var connection = new SqliteConnection(_connectionString);

        const string sql = """
            INSERT INTO QcpMasterActivities
            (
                Id,
                Code,
                Name,
                Category,
                Description,
                AcceptanceCriteria,
                VerificationMethod,
                ResponsibleParty,
                InspectionStage,
                HoldPointCategory,
                HoldPointType,
                RequiredNdtMethod,
                Mandatory,
                RequiredEvidence,
                Applicability,
                ConditionType,
                SourceReference,
                Notes,
                IsActive,
                CreatedBy,
                CreatedOn,
                ModifiedBy,
                ModifiedOn
            )
            VALUES
            (
                @Id,
                @Code,
                @Name,
                @Category,
                @Description,
                @AcceptanceCriteria,
                @VerificationMethod,
                @ResponsibleParty,
                @InspectionStage,
                @HoldPointCategory,
                @HoldPointType,
                @RequiredNdtMethod,
                @Mandatory,
                @RequiredEvidence,
                @Applicability,
                @ConditionType,
                @SourceReference,
                @Notes,
                @IsActive,
                @CreatedBy,
                @CreatedOn,
                @ModifiedBy,
                @ModifiedOn
            );
            """;

        connection.Execute(sql, Map(activity));
    }

    public QcpMasterActivity? GetById(Guid id)
    {
        using var connection = new SqliteConnection(_connectionString);

        const string sql = """
            SELECT
                Id,
                Code,
                Name,
                Category,
                Description,
                AcceptanceCriteria,
                VerificationMethod,
                ResponsibleParty,
                InspectionStage,
                HoldPointCategory,
                HoldPointType,
                RequiredNdtMethod,
                Mandatory,
                RequiredEvidence,
                Applicability,
                ConditionType,
                SourceReference,
                Notes,
                IsActive,
                CreatedBy,
                CreatedOn,
                ModifiedBy,
                ModifiedOn
            FROM QcpMasterActivities
            WHERE Id = @Id;
            """;

        var row = connection.QuerySingleOrDefault<dynamic>(sql, new { Id = id.ToString() }); return row == null ? null : MapRow(row);
    }

    public List<QcpMasterActivity> GetAll()
    {
        using var connection = new SqliteConnection(_connectionString);

        const string sql = """
            SELECT
                Id,
                Code,
                Name,
                Category,
                Description,
                AcceptanceCriteria,
                VerificationMethod,
                ResponsibleParty,
                InspectionStage,
                HoldPointCategory,
                HoldPointType,
                RequiredNdtMethod,
                Mandatory,
                RequiredEvidence,
                Applicability,
                ConditionType,
                SourceReference,
                Notes,
                IsActive,
                CreatedBy,
                CreatedOn,
                ModifiedBy,
                ModifiedOn
            FROM QcpMasterActivities
            ORDER BY Category, Code, Name;
            """;

        return connection.Query<dynamic>(sql).Select(MapRow).ToList();
    }

    public List<QcpMasterActivity> GetActive()
    {
        using var connection = new SqliteConnection(_connectionString);

        const string sql = """
            SELECT
                Id,
                Code,
                Name,
                Category,
                Description,
                AcceptanceCriteria,
                VerificationMethod,
                ResponsibleParty,
                InspectionStage,
                HoldPointCategory,
                HoldPointType,
                RequiredNdtMethod,
                Mandatory,
                RequiredEvidence,
                Applicability,
                ConditionType,
                SourceReference,
                Notes,
                IsActive,
                CreatedBy,
                CreatedOn,
                ModifiedBy,
                ModifiedOn
            FROM QcpMasterActivities
            WHERE IsActive = 1
            ORDER BY Category, Code, Name;
            """;

        return connection.Query<dynamic>(sql).Select(MapRow).ToList();
    }

    public void Update(QcpMasterActivity activity)
    {
        using var connection = new SqliteConnection(_connectionString);

        const string sql = """
            UPDATE QcpMasterActivities
            SET
                Code = @Code,
                Name = @Name,
                Category = @Category,
                Description = @Description,
                AcceptanceCriteria = @AcceptanceCriteria,
                VerificationMethod = @VerificationMethod,
                ResponsibleParty = @ResponsibleParty,
                InspectionStage = @InspectionStage,
                HoldPointCategory = @HoldPointCategory,
                HoldPointType = @HoldPointType,
                RequiredNdtMethod = @RequiredNdtMethod,
                Mandatory = @Mandatory,
                RequiredEvidence = @RequiredEvidence,
                Applicability = @Applicability,
                ConditionType = @ConditionType,
                SourceReference = @SourceReference,
                Notes = @Notes,
                IsActive = @IsActive,
                ModifiedBy = @ModifiedBy,
                ModifiedOn = @ModifiedOn
            WHERE Id = @Id;
            """;

        connection.Execute(sql, Map(activity));
    }

    public void Deactivate(Guid id, string modifiedBy, DateTime modifiedOn)
    {
        using var connection = new SqliteConnection(_connectionString);

        const string sql = """
            UPDATE QcpMasterActivities
            SET
                IsActive = 0,
                ModifiedBy = @ModifiedBy,
                ModifiedOn = @ModifiedOn
            WHERE Id = @Id;
            """;

        connection.Execute(sql, new
        {
            Id = id.ToString(),
            ModifiedBy = modifiedBy,
            ModifiedOn = modifiedOn
        });
    }

    private static QcpMasterActivity MapRow(dynamic row)
    {
        return new QcpMasterActivity
        {
            Id = Guid.Parse((string)row.Id),
            Code = (string)(row.Code ?? string.Empty),
            Name = (string)(row.Name ?? string.Empty),
            Category = (string)(row.Category ?? string.Empty),
            Description = (string)(row.Description ?? string.Empty),
            AcceptanceCriteria = (string)(row.AcceptanceCriteria ?? string.Empty),
            VerificationMethod = (string)(row.VerificationMethod ?? string.Empty),
            ResponsibleParty = (string)(row.ResponsibleParty ?? string.Empty),
            InspectionStage = (string)(row.InspectionStage ?? string.Empty),
            HoldPointCategory = row.HoldPointCategory == null ? null : (HoldPointCategory?)(long)row.HoldPointCategory,
            HoldPointType = row.HoldPointType == null ? null : (HoldPointType?)(long)row.HoldPointType,
            RequiredNdtMethod = row.RequiredNdtMethod == null ? null : (NdtMethodType?)(long)row.RequiredNdtMethod,
            Mandatory = Convert.ToInt32(row.Mandatory) != 0,
            RequiredEvidence = (string)(row.RequiredEvidence ?? string.Empty),
            Applicability = (string)(row.Applicability ?? string.Empty),
            ConditionType = (string)(row.ConditionType ?? string.Empty),
            SourceReference = (string)(row.SourceReference ?? string.Empty),
            Notes = (string)(row.Notes ?? string.Empty),
            IsActive = Convert.ToInt32(row.IsActive) != 0,
            CreatedBy = (string)(row.CreatedBy ?? string.Empty),
            CreatedOn = DateTime.Parse((string)row.CreatedOn),
            ModifiedBy = (string)(row.ModifiedBy ?? string.Empty),
            ModifiedOn = row.ModifiedOn == null ? null : DateTime.Parse((string)row.ModifiedOn)
        };
    }
    private static object Map(QcpMasterActivity activity)
    {
        return new
        {
            Id = activity.Id.ToString(),
            activity.Code,
            activity.Name,
            activity.Category,
            activity.Description,
            activity.AcceptanceCriteria,
            activity.VerificationMethod,
            activity.ResponsibleParty,
            activity.InspectionStage,
            HoldPointCategory = activity.HoldPointCategory.HasValue
                ? (int?)activity.HoldPointCategory.Value
                : null,
            HoldPointType = activity.HoldPointType.HasValue
                ? (int?)activity.HoldPointType.Value
                : null,
            RequiredNdtMethod = activity.RequiredNdtMethod.HasValue
                ? (int?)activity.RequiredNdtMethod.Value
                : null,
            Mandatory = activity.Mandatory ? 1 : 0,
            activity.RequiredEvidence,
            activity.Applicability,
            activity.ConditionType,
            activity.SourceReference,
            activity.Notes,
            IsActive = activity.IsActive ? 1 : 0,
            activity.CreatedBy,
            activity.CreatedOn,
            activity.ModifiedBy,
            activity.ModifiedOn
        };
    }
}



