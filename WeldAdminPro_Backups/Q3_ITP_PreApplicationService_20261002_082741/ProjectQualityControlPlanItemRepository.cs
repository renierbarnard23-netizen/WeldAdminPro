using System;
using Microsoft.Data.Sqlite;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Data.Repositories
{
    public class ProjectQualityControlPlanItemRepository
    {
        private string _connectionString =>
            $"Data Source={DatabasePath.Get()}";

        // =====================================================
        // ADD ITEM
        // =====================================================

        public void Add(ProjectQualityControlPlanItem item)
        {
            using var connection = new SqliteConnection(_connectionString);

            connection.Open();

            var cmd = connection.CreateCommand();

            cmd.CommandText = @"
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
    $id,
    $planId,
    $sequence,
    $activity,
    $requirementId,
    $acceptanceCriteria,
    $verificationMethod,
    $responsibleParty,
    $inspectionStage,
    $holdPointCategory,
    $holdPointType,
    $requiredNdtMethod,
    $mandatory,
    $requiredEvidence,
    $notes
);";

            AddParameters(cmd, item);

            cmd.ExecuteNonQuery();
        }

        // =====================================================
        // UPDATE ITEM
        // =====================================================

        public void Update(ProjectQualityControlPlanItem item)
        {
            using var connection = new SqliteConnection(_connectionString);

            connection.Open();

            var cmd = connection.CreateCommand();

            cmd.CommandText = @"
UPDATE ProjectQualityControlPlanItems
SET
    ProjectQualityControlPlanId = $planId,
    SequenceNumber = $sequence,
    Activity = $activity,
    CustomerQualityRequirementId = $requirementId,
    AcceptanceCriteria = $acceptanceCriteria,
    VerificationMethod = $verificationMethod,
    ResponsibleParty = $responsibleParty,
    InspectionStage = $inspectionStage,
    HoldPointCategory = $holdPointCategory,
    HoldPointType = $holdPointType,
    RequiredNdtMethod = $requiredNdtMethod,
    Mandatory = $mandatory,
    RequiredEvidence = $requiredEvidence,
    Notes = $notes
WHERE Id = $id;";

            AddParameters(cmd, item);

            cmd.ExecuteNonQuery();
        }

        // =====================================================
        // GET ITEM
        // =====================================================

        public ProjectQualityControlPlanItem? GetById(Guid id)
        {
            using var connection = new SqliteConnection(_connectionString);

            connection.Open();

            var cmd = connection.CreateCommand();

            cmd.CommandText = @"
SELECT
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
FROM ProjectQualityControlPlanItems
WHERE Id = $id;";

            cmd.Parameters.AddWithValue("$id", id.ToString());

            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return null;

            return Map(reader);
        }

        // =====================================================
        // GET ITEMS BY QCP
        // =====================================================

        public List<ProjectQualityControlPlanItem> GetByPlan(Guid planId)
        {
            var list = new List<ProjectQualityControlPlanItem>();

            using var connection = new SqliteConnection(_connectionString);

            connection.Open();

            var cmd = connection.CreateCommand();

            cmd.CommandText = @"
SELECT
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
FROM ProjectQualityControlPlanItems
WHERE ProjectQualityControlPlanId = $planId
ORDER BY SequenceNumber, Id;";

            cmd.Parameters.AddWithValue("$planId", planId.ToString());

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(Map(reader));
            }

            return list;
        }

        // =====================================================
        // DELETE ITEM
        // =====================================================

        public void Delete(Guid id)
        {
            using var connection = new SqliteConnection(_connectionString);

            connection.Open();

            var cmd = connection.CreateCommand();

            cmd.CommandText = @"
DELETE FROM ProjectQualityControlPlanItems
WHERE Id = $id;";

            cmd.Parameters.AddWithValue("$id", id.ToString());

            cmd.ExecuteNonQuery();
        }

        // =====================================================
        // PARAMETERS
        // =====================================================

        private static void AddParameters(
            SqliteCommand cmd,
            ProjectQualityControlPlanItem item)
        {
            cmd.Parameters.AddWithValue("$id", item.Id.ToString());
            cmd.Parameters.AddWithValue(
                "$planId",
                item.ProjectQualityControlPlanId.ToString());

            cmd.Parameters.AddWithValue(
                "$sequence",
                item.SequenceNumber);

            cmd.Parameters.AddWithValue(
                "$activity",
                item.Activity);

            cmd.Parameters.AddWithValue(
                "$requirementId",
                item.CustomerQualityRequirementId.HasValue
                    ? item.CustomerQualityRequirementId.Value.ToString()
                    : DBNull.Value);

            cmd.Parameters.AddWithValue(
                "$acceptanceCriteria",
                item.AcceptanceCriteria);

            cmd.Parameters.AddWithValue(
                "$verificationMethod",
                item.VerificationMethod);

            cmd.Parameters.AddWithValue(
                "$responsibleParty",
                item.ResponsibleParty);

            cmd.Parameters.AddWithValue(
                "$inspectionStage",
                item.InspectionStage);

            cmd.Parameters.AddWithValue(
                "$holdPointCategory",
                item.HoldPointCategory.HasValue
                    ? (int)item.HoldPointCategory.Value
                    : DBNull.Value);

            cmd.Parameters.AddWithValue(
                "$holdPointType",
                item.HoldPointType.HasValue
                    ? (int)item.HoldPointType.Value
                    : DBNull.Value);

            cmd.Parameters.AddWithValue(
                "$requiredNdtMethod",
                item.RequiredNdtMethod.HasValue
                    ? (int)item.RequiredNdtMethod.Value
                    : DBNull.Value);

            cmd.Parameters.AddWithValue(
                "$mandatory",
                item.Mandatory ? 1 : 0);

            cmd.Parameters.AddWithValue(
                "$requiredEvidence",
                item.RequiredEvidence);

            cmd.Parameters.AddWithValue(
                "$notes",
                item.Notes);
        }

        // =====================================================
        // MAP
        // =====================================================

        private static ProjectQualityControlPlanItem Map(
            SqliteDataReader reader)
        {
            return new ProjectQualityControlPlanItem
            {
                Id = Guid.Parse(reader.GetString(0)),

                ProjectQualityControlPlanId =
                    Guid.Parse(reader.GetString(1)),

                SequenceNumber =
                    reader.GetInt32(2),

                Activity =
                    reader.GetString(3),

                CustomerQualityRequirementId =
                    reader.IsDBNull(4)
                        ? null
                        : Guid.Parse(reader.GetString(4)),

                AcceptanceCriteria =
                    reader.GetString(5),

                VerificationMethod =
                    reader.GetString(6),

                ResponsibleParty =
                    reader.GetString(7),

                InspectionStage =
                    reader.GetString(8),

                HoldPointCategory =
                    reader.IsDBNull(9)
                        ? null
                        : (HoldPointCategory)reader.GetInt32(9),

                HoldPointType =
                    reader.IsDBNull(10)
                        ? null
                        : (HoldPointType)reader.GetInt32(10),

                RequiredNdtMethod =
                    reader.IsDBNull(11)
                        ? null
                        : (NdtMethodType)reader.GetInt32(11),

                Mandatory =
                    reader.GetInt32(12) == 1,

                RequiredEvidence =
                    reader.GetString(13),

                Notes =
                    reader.GetString(14)
            };
        }
    }
}
