using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Data.Repositories
{
    public class ProjectQualityControlPlanRepository
    {
        private readonly string _connectionString;

        public ProjectQualityControlPlanRepository(
            string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Add(ProjectQualityControlPlan item)
        {
            using var connection =
                new SqliteConnection(_connectionString);

            connection.Execute(
                @"INSERT INTO ProjectQualityControlPlans
                (
                    Id,
                    ProjectId,
                    QcpNumber,
                    Revision,
                    Status,
                    Title,
                    Description,
                    PreparedBy,
                    PreparedOn,
                    ApprovedBy,
                    ApprovedOn,
                    EffectiveDate,
                    SupersededOn,
                    PreviousRevisionId,
                    IsActive
                )
                VALUES
                (
                    @Id,
                    @ProjectId,
                    @QcpNumber,
                    @Revision,
                    @Status,
                    @Title,
                    @Description,
                    @PreparedBy,
                    @PreparedOn,
                    @ApprovedBy,
                    @ApprovedOn,
                    @EffectiveDate,
                    @SupersededOn,
                    @PreviousRevisionId,
                    @IsActive
                )",
                ToParameters(item));
        }

        public ProjectQualityControlPlan? GetById(Guid id)
        {
            using var connection =
                new SqliteConnection(_connectionString);

            var row = connection.QuerySingleOrDefault(
                @"SELECT *
                  FROM ProjectQualityControlPlans
                  WHERE Id = @Id",
                new
                {
                    Id = id.ToString()
                });

            return row == null
                ? null
                : Map(row);
        }

        public List<ProjectQualityControlPlan> GetByProject(
            Guid projectId)
        {
            using var connection =
                new SqliteConnection(_connectionString);

            var rows = connection.Query(
                @"SELECT *
                  FROM ProjectQualityControlPlans
                  WHERE ProjectId = @ProjectId
                  ORDER BY QcpNumber, Revision",
                new
                {
                    ProjectId = projectId.ToString()
                });

            var result =
                new List<ProjectQualityControlPlan>();

            foreach (var row in rows)
                result.Add(Map(row));

            return result;
        }

        public List<ProjectQualityControlPlan> GetAll()
        {
            using var connection =
                new SqliteConnection(_connectionString);

            var rows = connection.Query(
                @"SELECT *
                  FROM ProjectQualityControlPlans
                  ORDER BY ProjectId, QcpNumber, Revision");

            var result =
                new List<ProjectQualityControlPlan>();

            foreach (var row in rows)
                result.Add(Map(row));

            return result;
        }

        public void Update(ProjectQualityControlPlan item)
        {
            using var connection =
                new SqliteConnection(_connectionString);

            connection.Execute(
                @"UPDATE ProjectQualityControlPlans
                  SET
                    ProjectId = @ProjectId,
                    QcpNumber = @QcpNumber,
                    Revision = @Revision,
                    Status = @Status,
                    Title = @Title,
                    Description = @Description,
                    PreparedBy = @PreparedBy,
                    PreparedOn = @PreparedOn,
                    ApprovedBy = @ApprovedBy,
                    ApprovedOn = @ApprovedOn,
                    EffectiveDate = @EffectiveDate,
                    SupersededOn = @SupersededOn,
                    PreviousRevisionId = @PreviousRevisionId,
                    IsActive = @IsActive
                  WHERE Id = @Id",
                ToParameters(item));
        }

        public void SaveRevision(
            ProjectQualityControlPlan previous,
            ProjectQualityControlPlan revision)
        {
            SaveRevision(previous, revision, null);
        }

        public void SaveRevision(
            ProjectQualityControlPlan previous,
            ProjectQualityControlPlan revision,
            IEnumerable<ProjectQualityControlPlanItem>? revisionItems)
        {
            if (previous == null)
                throw new ArgumentNullException(nameof(previous));

            if (revision == null)
                throw new ArgumentNullException(nameof(revision));

            if (previous.Id == Guid.Empty)
                throw new ArgumentException(
                    "Previous project QCP ID cannot be empty.",
                    nameof(previous));

            if (revision.Id == Guid.Empty)
                throw new ArgumentException(
                    "New project QCP revision ID cannot be empty.",
                    nameof(revision));

            if (previous.ProjectId != revision.ProjectId)
                throw new InvalidOperationException(
                    "Previous and new project QCP revisions must belong to the same project.");

            if (!string.Equals(
                    previous.QcpNumber,
                    revision.QcpNumber,
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(
                    "Previous and new project QCP revisions must have the same QCP number.");

            if (revision.PreviousRevisionId != previous.Id)
                throw new InvalidOperationException(
                    "The new project QCP revision must reference the previous revision.");

            if (!previous.IsActive)
                throw new InvalidOperationException(
                    "The previous project QCP revision is not active.");

            if (string.IsNullOrWhiteSpace(revision.Revision))
                throw new ArgumentException(
                    "New project QCP revision is required.",
                    nameof(revision));

            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            using var transaction =
                connection.BeginTransaction();

            try
            {
                using (var deactivate =
                    connection.CreateCommand())
                {
                    deactivate.Transaction = transaction;

                    deactivate.CommandText = @"
UPDATE ProjectQualityControlPlans
SET
    IsActive = 0,
    Status = @Status,
    SupersededOn = @SupersededOn
WHERE Id = @Id
  AND IsActive = 1;";

                    deactivate.Parameters.AddWithValue(
                        "@Status",
                        (int)ProjectQualityControlPlanStatus.Superseded);

                    deactivate.Parameters.AddWithValue(
                        "@SupersededOn",
                        DateTime.Now.ToString("O"));

                    deactivate.Parameters.AddWithValue(
                        "@Id",
                        previous.Id.ToString());

                    var affected =
                        deactivate.ExecuteNonQuery();

                    if (affected != 1)
                        throw new InvalidOperationException(
                            "The previous project QCP revision could not be deactivated.");
                }

                using (var insert =
                    connection.CreateCommand())
                {
                    insert.Transaction = transaction;

                    insert.CommandText = @"
INSERT INTO ProjectQualityControlPlans
(
    Id,
    ProjectId,
    QcpNumber,
    Revision,
    Status,
    Title,
    Description,
    PreparedBy,
    PreparedOn,
    ApprovedBy,
    ApprovedOn,
    EffectiveDate,
    SupersededOn,
    PreviousRevisionId,
    IsActive
)
VALUES
(
    @Id,
    @ProjectId,
    @QcpNumber,
    @Revision,
    @Status,
    @Title,
    @Description,
    @PreparedBy,
    @PreparedOn,
    @ApprovedBy,
    @ApprovedOn,
    @EffectiveDate,
    @SupersededOn,
    @PreviousRevisionId,
    @IsActive
);";

                    insert.Parameters.AddWithValue(
                        "@Id",
                        revision.Id.ToString());

                    insert.Parameters.AddWithValue(
                        "@ProjectId",
                        revision.ProjectId.ToString());

                    insert.Parameters.AddWithValue(
                        "@QcpNumber",
                        revision.QcpNumber);

                    insert.Parameters.AddWithValue(
                        "@Revision",
                        revision.Revision);

                    insert.Parameters.AddWithValue(
                        "@Status",
                        (int)revision.Status);

                    insert.Parameters.AddWithValue(
                        "@Title",
                        revision.Title);

                    insert.Parameters.AddWithValue(
                        "@Description",
                        (object?)revision.Description ?? DBNull.Value);

                    insert.Parameters.AddWithValue(
                        "@PreparedBy",
                        (object?)revision.PreparedBy ?? DBNull.Value);

                    insert.Parameters.AddWithValue(
                        "@PreparedOn",
                        revision.PreparedOn.HasValue
                            ? revision.PreparedOn.Value.ToString("O")
                            : DBNull.Value);

                    insert.Parameters.AddWithValue(
                        "@ApprovedBy",
                        (object?)revision.ApprovedBy ?? DBNull.Value);

                    insert.Parameters.AddWithValue(
                        "@ApprovedOn",
                        revision.ApprovedOn.HasValue
                            ? revision.ApprovedOn.Value.ToString("O")
                            : DBNull.Value);

                    insert.Parameters.AddWithValue(
                        "@EffectiveDate",
                        revision.EffectiveDate.HasValue
                            ? revision.EffectiveDate.Value.ToString("O")
                            : DBNull.Value);

                    insert.Parameters.AddWithValue(
                        "@SupersededOn",
                        revision.SupersededOn.HasValue
                            ? revision.SupersededOn.Value.ToString("O")
                            : DBNull.Value);

                    insert.Parameters.AddWithValue(
                        "@PreviousRevisionId",
                        revision.PreviousRevisionId.HasValue
                            ? revision.PreviousRevisionId.Value.ToString()
                            : DBNull.Value);

                    insert.Parameters.AddWithValue(
                        "@IsActive",
                        revision.IsActive ? 1 : 0);

                    var inserted =
                        insert.ExecuteNonQuery();

                    if (inserted != 1)
                        throw new InvalidOperationException(
                            "The new project QCP revision could not be inserted.");
                }

                if (revisionItems != null)
                {
                    foreach (var item in revisionItems)
                    {
                        if (item == null)
                            throw new InvalidOperationException(
                                "A project QCP revision item cannot be null.");

                        if (item.ProjectQualityControlPlanId != revision.Id)
                            throw new InvalidOperationException(
                                "Revision snapshot items must belong to the new project QCP revision.");

                        using var itemInsert =
                            connection.CreateCommand();

                        itemInsert.Transaction = transaction;

                        itemInsert.CommandText = @"
INSERT INTO ProjectQualityControlPlanItems
(
    Id,
    ProjectQualityControlPlanId,
    SequenceNumber,
    Activity,
    CustomerQualityRequirementId,
    AcceptanceCriteria,
    VerificationMethod,
    ResponsibleParty,
    InspectionStage,
    HoldPointCategory,
    HoldPointType,
    RequiredNdtMethod,
    Mandatory,
    RequiredEvidence,
    Notes
)
VALUES
(
    @Id,
    @ProjectQualityControlPlanId,
    @SequenceNumber,
    @Activity,
    @CustomerQualityRequirementId,
    @AcceptanceCriteria,
    @VerificationMethod,
    @ResponsibleParty,
    @InspectionStage,
    @HoldPointCategory,
    @HoldPointType,
    @RequiredNdtMethod,
    @Mandatory,
    @RequiredEvidence,
    @Notes
);";

                        itemInsert.Parameters.AddWithValue(
                            "@Id",
                            Guid.NewGuid().ToString());

                        itemInsert.Parameters.AddWithValue(
                            "@ProjectQualityControlPlanId",
                            revision.Id.ToString());

                        itemInsert.Parameters.AddWithValue(
                            "@SequenceNumber",
                            item.SequenceNumber);

                        itemInsert.Parameters.AddWithValue(
                            "@Activity",
                            item.Activity);

                        itemInsert.Parameters.AddWithValue(
                            "@CustomerQualityRequirementId",
                            item.CustomerQualityRequirementId.HasValue
                                ? item.CustomerQualityRequirementId.Value.ToString()
                                : DBNull.Value);

                        itemInsert.Parameters.AddWithValue(
                            "@AcceptanceCriteria",
                            item.AcceptanceCriteria);

                        itemInsert.Parameters.AddWithValue(
                            "@VerificationMethod",
                            item.VerificationMethod);

                        itemInsert.Parameters.AddWithValue(
                            "@ResponsibleParty",
                            item.ResponsibleParty);

                        itemInsert.Parameters.AddWithValue(
                            "@InspectionStage",
                            item.InspectionStage);

                        itemInsert.Parameters.AddWithValue(
                            "@HoldPointCategory",
                            item.HoldPointCategory.HasValue
                                ? (int)item.HoldPointCategory.Value
                                : DBNull.Value);

                        itemInsert.Parameters.AddWithValue(
                            "@HoldPointType",
                            item.HoldPointType.HasValue
                                ? (int)item.HoldPointType.Value
                                : DBNull.Value);

                        itemInsert.Parameters.AddWithValue(
                            "@RequiredNdtMethod",
                            item.RequiredNdtMethod.HasValue
                                ? (int)item.RequiredNdtMethod.Value
                                : DBNull.Value);

                        itemInsert.Parameters.AddWithValue(
                            "@Mandatory",
                            item.Mandatory ? 1 : 0);

                        itemInsert.Parameters.AddWithValue(
                            "@RequiredEvidence",
                            item.RequiredEvidence);

                        itemInsert.Parameters.AddWithValue(
                            "@Notes",
                            item.Notes);

                        if (itemInsert.ExecuteNonQuery() != 1)
                            throw new InvalidOperationException(
                                "A project QCP revision item could not be inserted.");
                    }
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void Delete(Guid id)
        {
            using var connection =
                new SqliteConnection(_connectionString);

            connection.Execute(
                @"DELETE FROM ProjectQualityControlPlans
                  WHERE Id = @Id",
                new
                {
                    Id = id.ToString()
                });
        }

        private static object ToParameters(
            ProjectQualityControlPlan item)
        {
            return new
            {
                Id = item.Id.ToString(),
                ProjectId = item.ProjectId.ToString(),
                item.QcpNumber,
                item.Revision,
                Status = (int)item.Status,
                item.Title,
                item.Description,
                item.PreparedBy,
                PreparedOn =
                    item.PreparedOn.HasValue
                        ? item.PreparedOn.Value.ToString("O")
                        : null,
                item.ApprovedBy,
                ApprovedOn =
                    item.ApprovedOn.HasValue
                        ? item.ApprovedOn.Value.ToString("O")
                        : null,
                EffectiveDate =
                    item.EffectiveDate.HasValue
                        ? item.EffectiveDate.Value.ToString("O")
                        : null,
                SupersededOn =
                    item.SupersededOn.HasValue
                        ? item.SupersededOn.Value.ToString("O")
                        : null,
                PreviousRevisionId =
                    item.PreviousRevisionId.HasValue
                        ? item.PreviousRevisionId.Value.ToString()
                        : null,
                IsActive = item.IsActive ? 1 : 0
            };
        }

        private static ProjectQualityControlPlan Map(
            dynamic row)
        {
            return new ProjectQualityControlPlan
            {
                Id =
                    Guid.Parse(
                        (string)row.Id),

                ProjectId =
                    Guid.Parse(
                        (string)row.ProjectId),

                QcpNumber =
                    row.QcpNumber?.ToString()
                    ?? string.Empty,

                Revision =
                    row.Revision?.ToString()
                    ?? string.Empty,

                Status =
                    (ProjectQualityControlPlanStatus)
                    Convert.ToInt32(row.Status),

                Title =
                    row.Title?.ToString()
                    ?? string.Empty,

                Description =
                    row.Description?.ToString(),

                PreparedBy =
                    row.PreparedBy?.ToString(),

                PreparedOn =
                    row.PreparedOn == null
                        ? null
                        : DateTime.Parse(
                            row.PreparedOn.ToString()),

                ApprovedBy =
                    row.ApprovedBy?.ToString(),

                ApprovedOn =
                    row.ApprovedOn == null
                        ? null
                        : DateTime.Parse(
                            row.ApprovedOn.ToString()),

                EffectiveDate =
                    row.EffectiveDate == null
                        ? null
                        : DateTime.Parse(
                            row.EffectiveDate.ToString()),

                SupersededOn =
                    row.SupersededOn == null
                        ? null
                        : DateTime.Parse(
                            row.SupersededOn.ToString()),

                PreviousRevisionId =
                    row.PreviousRevisionId == null
                        ? null
                        : Guid.Parse(
                            row.PreviousRevisionId.ToString()),

                IsActive =
                    Convert.ToBoolean(row.IsActive)
            };
        }
    }
}
