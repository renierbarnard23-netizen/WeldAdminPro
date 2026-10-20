using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Data.Repositories
{
    public class CustomerQualityRequirementRepository
    {
        private readonly string _connectionString;

        public CustomerQualityRequirementRepository(
            string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Add(CustomerQualityRequirement item)
        {
            using var connection =
                new SqliteConnection(_connectionString);

            connection.Execute(
                @"INSERT INTO CustomerQualityRequirements
                (
                    Id,
                    ProjectId,
                    RequirementNumber,
                    Category,
                    Description,
                    SourceDocument,
                    SourceClause,
                    Revision,
                    Mandatory,
                    AcceptanceCriteria,
                    VerificationMethod,
                    ResponsibleParty,
                    RequiredEvidence,
                    Status,
                    CreatedOn,
                    CreatedBy,
                    ApprovedOn,
                    ApprovedBy,
                    EffectiveDate,
                    SupersededOn,
                    PreviousRevisionId,
                    IsActive
                )
                VALUES
                (
                    @Id,
                    @ProjectId,
                    @RequirementNumber,
                    @Category,
                    @Description,
                    @SourceDocument,
                    @SourceClause,
                    @Revision,
                    @Mandatory,
                    @AcceptanceCriteria,
                    @VerificationMethod,
                    @ResponsibleParty,
                    @RequiredEvidence,
                    @Status,
                    @CreatedOn,
                    @CreatedBy,
                    @ApprovedOn,
                    @ApprovedBy,
                    @EffectiveDate,
                    @SupersededOn,
                    @PreviousRevisionId,
                    @IsActive
                )",
                ToParameters(item));
        }

        public CustomerQualityRequirement? GetById(Guid id)
        {
            using var connection =
                new SqliteConnection(_connectionString);

            var row = connection.QuerySingleOrDefault(
                @"SELECT *
                  FROM CustomerQualityRequirements
                  WHERE Id = @Id",
                new
                {
                    Id = id.ToString()
                });

            return row == null
                ? null
                : Map(row);
        }

        public List<CustomerQualityRequirement> GetByProject(
            Guid projectId)
        {
            using var connection =
                new SqliteConnection(_connectionString);

            var rows = connection.Query(
                @"SELECT *
                  FROM CustomerQualityRequirements
                  WHERE ProjectId = @ProjectId
                  ORDER BY RequirementNumber",
                new
                {
                    ProjectId = projectId.ToString()
                });

            var result =
                new List<CustomerQualityRequirement>();

            foreach (var row in rows)
                result.Add(Map(row));

            return result;
        }

        public List<CustomerQualityRequirement> GetAll()
        {
            using var connection =
                new SqliteConnection(_connectionString);

            var rows = connection.Query(
                @"SELECT *
                  FROM CustomerQualityRequirements
                  ORDER BY ProjectId, RequirementNumber");

            var result =
                new List<CustomerQualityRequirement>();

            foreach (var row in rows)
                result.Add(Map(row));

            return result;
        }

        public void Update(CustomerQualityRequirement item)
        {
            using var connection =
                new SqliteConnection(_connectionString);

            connection.Execute(
                @"UPDATE CustomerQualityRequirements
                  SET
                    ProjectId = @ProjectId,
                    RequirementNumber = @RequirementNumber,
                    Category = @Category,
                    Description = @Description,
                    SourceDocument = @SourceDocument,
                    SourceClause = @SourceClause,
                    Revision = @Revision,
                    Mandatory = @Mandatory,
                    AcceptanceCriteria = @AcceptanceCriteria,
                    VerificationMethod = @VerificationMethod,
                    ResponsibleParty = @ResponsibleParty,
                    RequiredEvidence = @RequiredEvidence,
                    Status = @Status,
                    CreatedOn = @CreatedOn,
                    CreatedBy = @CreatedBy,
                    ApprovedOn = @ApprovedOn,
                    ApprovedBy = @ApprovedBy,
                    EffectiveDate = @EffectiveDate,
                    SupersededOn = @SupersededOn,
                    PreviousRevisionId = @PreviousRevisionId,
                    IsActive = @IsActive
                  WHERE Id = @Id",
                ToParameters(item));
        }

        public void SaveRevision(
            CustomerQualityRequirement previous,
            CustomerQualityRequirement revision)
        {
            if (previous == null)
            {
                throw new ArgumentNullException(nameof(previous));
            }

            if (revision == null)
            {
                throw new ArgumentNullException(nameof(revision));
            }

            if (previous.Id == Guid.Empty)
            {
                throw new ArgumentException(
                    "Previous customer quality requirement ID cannot be empty.",
                    nameof(previous));
            }

            if (revision.Id == Guid.Empty)
            {
                throw new ArgumentException(
                    "New customer quality requirement revision ID cannot be empty.",
                    nameof(revision));
            }

            if (previous.ProjectId != revision.ProjectId)
            {
                throw new InvalidOperationException(
                    "Previous and new customer quality requirement revisions must belong to the same project.");
            }

            if (!string.Equals(
                    previous.RequirementNumber,
                    revision.RequirementNumber,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Previous and new customer quality requirement revisions must have the same requirement number.");
            }

            if (revision.PreviousRevisionId != previous.Id)
            {
                throw new InvalidOperationException(
                    "The new customer quality requirement revision must reference the previous revision.");
            }

            if (!previous.IsActive)
            {
                throw new InvalidOperationException(
                    "The previous customer quality requirement revision is not active.");
            }

            if (string.IsNullOrWhiteSpace(revision.Revision))
            {
                throw new ArgumentException(
                    "New customer quality requirement revision is required.",
                    nameof(revision));
            }

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
UPDATE CustomerQualityRequirements
SET
    IsActive = 0,
    Status = @Status,
    SupersededOn = @SupersededOn
WHERE Id = @Id
  AND IsActive = 1;";

                    deactivate.Parameters.AddWithValue(
                        "@Status",
                        (int)CustomerQualityRequirementStatus.Superseded);

                    deactivate.Parameters.AddWithValue(
                        "@SupersededOn",
                        DateTime.Now.ToString("O"));

                    deactivate.Parameters.AddWithValue(
                        "@Id",
                        previous.Id.ToString());

                    var affected =
                        deactivate.ExecuteNonQuery();

                    if (affected != 1)
                    {
                        throw new InvalidOperationException(
                            "The previous customer quality requirement revision could not be deactivated.");
                    }
                }

                using (var insert =
                    connection.CreateCommand())
                {
                    insert.Transaction = transaction;

                    insert.CommandText = @"
INSERT INTO CustomerQualityRequirements
(
    Id,
    ProjectId,
    RequirementNumber,
    Category,
    Description,
    SourceDocument,
    SourceClause,
    Revision,
    Mandatory,
    AcceptanceCriteria,
    VerificationMethod,
    ResponsibleParty,
    RequiredEvidence,
    Status,
    CreatedOn,
    CreatedBy,
    ApprovedOn,
    ApprovedBy,
    EffectiveDate,
    SupersededOn,
    PreviousRevisionId,
    IsActive
)
VALUES
(
    @Id,
    @ProjectId,
    @RequirementNumber,
    @Category,
    @Description,
    @SourceDocument,
    @SourceClause,
    @Revision,
    @Mandatory,
    @AcceptanceCriteria,
    @VerificationMethod,
    @ResponsibleParty,
    @RequiredEvidence,
    @Status,
    @CreatedOn,
    @CreatedBy,
    @ApprovedOn,
    @ApprovedBy,
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
                        "@RequirementNumber",
                        revision.RequirementNumber);

                    insert.Parameters.AddWithValue(
                        "@Category",
                        (object?)revision.Category ?? DBNull.Value);

                    insert.Parameters.AddWithValue(
                        "@Description",
                        revision.Description);

                    insert.Parameters.AddWithValue(
                        "@SourceDocument",
                        (object?)revision.SourceDocument ?? DBNull.Value);

                    insert.Parameters.AddWithValue(
                        "@SourceClause",
                        (object?)revision.SourceClause ?? DBNull.Value);

                    insert.Parameters.AddWithValue(
                        "@Revision",
                        (object?)revision.Revision ?? DBNull.Value);

                    insert.Parameters.AddWithValue(
                        "@Mandatory",
                        revision.Mandatory ? 1 : 0);

                    insert.Parameters.AddWithValue(
                        "@AcceptanceCriteria",
                        (object?)revision.AcceptanceCriteria ?? DBNull.Value);

                    insert.Parameters.AddWithValue(
                        "@VerificationMethod",
                        (object?)revision.VerificationMethod ?? DBNull.Value);

                    insert.Parameters.AddWithValue(
                        "@ResponsibleParty",
                        (object?)revision.ResponsibleParty ?? DBNull.Value);

                    insert.Parameters.AddWithValue(
                        "@RequiredEvidence",
                        (object?)revision.RequiredEvidence ?? DBNull.Value);

                    insert.Parameters.AddWithValue(
                        "@Status",
                        (int)revision.Status);

                    insert.Parameters.AddWithValue(
                        "@CreatedOn",
                        revision.CreatedOn.ToString("O"));

                    insert.Parameters.AddWithValue(
                        "@CreatedBy",
                        (object?)revision.CreatedBy ?? DBNull.Value);

                    insert.Parameters.AddWithValue(
                        "@ApprovedOn",
                        revision.ApprovedOn.HasValue
                            ? revision.ApprovedOn.Value.ToString("O")
                            : DBNull.Value);

                    insert.Parameters.AddWithValue(
                        "@ApprovedBy",
                        (object?)revision.ApprovedBy ?? DBNull.Value);

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
                    {
                        throw new InvalidOperationException(
                            "The new customer quality requirement revision could not be inserted.");
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
                @"DELETE FROM CustomerQualityRequirements
                  WHERE Id = @Id",
                new
                {
                    Id = id.ToString()
                });
        }

        private static object ToParameters(
            CustomerQualityRequirement item)
        {
            return new
            {
                Id = item.Id.ToString(),
                ProjectId = item.ProjectId.ToString(),
                item.RequirementNumber,
                item.Category,
                item.Description,
                item.SourceDocument,
                item.SourceClause,
                item.Revision,
                Mandatory = item.Mandatory ? 1 : 0,
                item.AcceptanceCriteria,
                item.VerificationMethod,
                item.ResponsibleParty,
                item.RequiredEvidence,
                Status = (int)item.Status,
                CreatedOn = item.CreatedOn.ToString("O"),
                item.CreatedBy,
                ApprovedOn =
                    item.ApprovedOn.HasValue
                        ? item.ApprovedOn.Value.ToString("O")
                        : null,
                item.ApprovedBy,
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

        private static CustomerQualityRequirement Map(
            dynamic row)
        {
            return new CustomerQualityRequirement
            {
                Id =
                    Guid.Parse(
                        (string)row.Id),

                ProjectId =
                    Guid.Parse(
                        (string)row.ProjectId),

                RequirementNumber =
                    row.RequirementNumber?.ToString()
                    ?? string.Empty,

                Category =
                    row.Category?.ToString()
                    ?? string.Empty,

                Description =
                    row.Description?.ToString()
                    ?? string.Empty,

                SourceDocument =
                    row.SourceDocument?.ToString()
                    ?? string.Empty,

                SourceClause =
                    row.SourceClause?.ToString()
                    ?? string.Empty,

                Revision =
                    row.Revision?.ToString()
                    ?? string.Empty,

                Mandatory =
                    Convert.ToBoolean(
                        row.Mandatory),

                AcceptanceCriteria =
                    row.AcceptanceCriteria?.ToString()
                    ?? string.Empty,

                VerificationMethod =
                    row.VerificationMethod?.ToString()
                    ?? string.Empty,

                ResponsibleParty =
                    row.ResponsibleParty?.ToString()
                    ?? string.Empty,

                RequiredEvidence =
                    row.RequiredEvidence?.ToString()
                    ?? string.Empty,

                Status =
                    (CustomerQualityRequirementStatus)
                    Convert.ToInt32(row.Status),

                CreatedOn =
                    DateTime.Parse(
                        row.CreatedOn.ToString()),

                CreatedBy =
                    row.CreatedBy?.ToString()
                    ?? string.Empty,

                ApprovedOn =
                    row.ApprovedOn == null
                        ? null
                        : DateTime.Parse(
                            row.ApprovedOn.ToString()),

                ApprovedBy =
                    row.ApprovedBy?.ToString()
                    ?? string.Empty,

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


