using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Data.Repositories
{
    public class InspectionTestPlanItemRepository
    {
        private string _connectionString =>
            $"Data Source={DatabasePath.Get()}";

        // =====================================================
        // ADD ITEM
        // =====================================================

        public void Add(InspectionTestPlanItem item)
        {
            using var connection = new SqliteConnection(_connectionString);

            connection.Open();

            var cmd = connection.CreateCommand();

            cmd.CommandText = @"
INSERT INTO InspectionTestPlanItems
(
    Id,
    InspectionTestPlanId,
    SequenceNumber,
    QcpRevisionId,
    QcpItemId,
    CustomerQualityRequirementId,
    Activity,
    AcceptanceCriteria,
    VerificationMethod,
    ResponsibleParty,
    InspectionStage,
    HoldPointCategory,
    HoldPointType,
    RequiredNdtMethod,
    Mandatory,
    RequiredEvidence,
    ExecutionType,
    Notes
)
VALUES
(
    $id,
    $planId,
    $sequence,
    $qcpRevisionId,
    $qcpItemId,
    $requirementId,
    $activity,
    $acceptanceCriteria,
    $verificationMethod,
    $responsibleParty,
    $inspectionStage,
    $holdPointCategory,
    $holdPointType,
    $requiredNdtMethod,
    $mandatory,
    $requiredEvidence,
    $executionType,
    $notes
);";

            AddParameters(cmd, item);

            cmd.ExecuteNonQuery();
        }

        // =====================================================
        // UPDATE ITEM
        // =====================================================

        public void Update(InspectionTestPlanItem item)
        {
            using var connection = new SqliteConnection(_connectionString);

            connection.Open();

            var cmd = connection.CreateCommand();

            cmd.CommandText = @"
UPDATE InspectionTestPlanItems
SET
    InspectionTestPlanId = $planId,
    SequenceNumber = $sequence,
    QcpRevisionId = $qcpRevisionId,
    QcpItemId = $qcpItemId,
    CustomerQualityRequirementId = $requirementId,
    Activity = $activity,
    AcceptanceCriteria = $acceptanceCriteria,
    VerificationMethod = $verificationMethod,
    ResponsibleParty = $responsibleParty,
    InspectionStage = $inspectionStage,
    HoldPointCategory = $holdPointCategory,
    HoldPointType = $holdPointType,
    RequiredNdtMethod = $requiredNdtMethod,
    Mandatory = $mandatory,
    RequiredEvidence = $requiredEvidence,
    ExecutionType = $executionType,
    Notes = $notes
WHERE Id = $id;";

            AddParameters(cmd, item);

            cmd.ExecuteNonQuery();
        }

        // =====================================================
        // GET ITEM
        // =====================================================

        public InspectionTestPlanItem? GetById(Guid id)
        {
            using var connection = new SqliteConnection(_connectionString);

            connection.Open();

            var cmd = connection.CreateCommand();

            cmd.CommandText = @"
SELECT
    Id,
    InspectionTestPlanId,
    SequenceNumber,
    QcpRevisionId,
    QcpItemId,
    CustomerQualityRequirementId,
    Activity,
    AcceptanceCriteria,
    VerificationMethod,
    ResponsibleParty,
    InspectionStage,
    HoldPointCategory,
    HoldPointType,
    RequiredNdtMethod,
    Mandatory,
    RequiredEvidence,
    ExecutionType,
    Notes
FROM InspectionTestPlanItems
WHERE Id = $id;";

            cmd.Parameters.AddWithValue("$id", id.ToString());

            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return null;

            return Map(reader);
        }

        // =====================================================
        // GET ITEMS BY ITP
        // =====================================================

        public List<InspectionTestPlanItem> GetByPlan(Guid planId)
        {
            var list = new List<InspectionTestPlanItem>();

            using var connection = new SqliteConnection(_connectionString);

            connection.Open();

            var cmd = connection.CreateCommand();

            cmd.CommandText = @"
SELECT
    Id,
    InspectionTestPlanId,
    SequenceNumber,
    QcpRevisionId,
    QcpItemId,
    CustomerQualityRequirementId,
    Activity,
    AcceptanceCriteria,
    VerificationMethod,
    ResponsibleParty,
    InspectionStage,
    HoldPointCategory,
    HoldPointType,
    RequiredNdtMethod,
    Mandatory,
    RequiredEvidence,
    ExecutionType,
    Notes
FROM InspectionTestPlanItems
WHERE InspectionTestPlanId = $planId
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
DELETE FROM InspectionTestPlanItems
WHERE Id = $id;";

            cmd.Parameters.AddWithValue("$id", id.ToString());

            cmd.ExecuteNonQuery();
        }

        // =====================================================
        // PARAMETERS
        // =====================================================

        private static void AddParameters(
            SqliteCommand cmd,
            InspectionTestPlanItem item)
        {
            cmd.Parameters.AddWithValue(
                "$id",
                item.Id.ToString());

            cmd.Parameters.AddWithValue(
                "$planId",
                item.InspectionTestPlanId.ToString());

            cmd.Parameters.AddWithValue(
                "$sequence",
                item.SequenceNumber);

            cmd.Parameters.AddWithValue(
                "$qcpRevisionId",
                item.QcpRevisionId.ToString());

            cmd.Parameters.AddWithValue(
                "$qcpItemId",
                item.QcpItemId.ToString());

            cmd.Parameters.AddWithValue(
                "$requirementId",
                item.CustomerQualityRequirementId.HasValue
                    ? item.CustomerQualityRequirementId.Value.ToString()
                    : DBNull.Value);

            cmd.Parameters.AddWithValue(
                "$activity",
                item.Activity);

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
                "$executionType",
                item.ExecutionType);

            cmd.Parameters.AddWithValue(
                "$notes",
                item.Notes);
        }

        // =====================================================
        // MAP
        // =====================================================

        private static InspectionTestPlanItem Map(
            SqliteDataReader reader)
        {
            return new InspectionTestPlanItem
            {
                Id =
                    Guid.Parse(reader.GetString(0)),

                InspectionTestPlanId =
                    Guid.Parse(reader.GetString(1)),

                SequenceNumber =
                    reader.GetInt32(2),

                QcpRevisionId =
                    Guid.Parse(reader.GetString(3)),

                QcpItemId =
                    Guid.Parse(reader.GetString(4)),

                CustomerQualityRequirementId =
                    reader.IsDBNull(5)
                        ? null
                        : Guid.Parse(reader.GetString(5)),

                Activity =
                    reader.GetString(6),

                AcceptanceCriteria =
                    reader.GetString(7),

                VerificationMethod =
                    reader.GetString(8),

                ResponsibleParty =
                    reader.GetString(9),

                InspectionStage =
                    reader.GetString(10),

                HoldPointCategory =
                    reader.IsDBNull(11)
                        ? null
                        : (HoldPointCategory)reader.GetInt32(11),

                HoldPointType =
                    reader.IsDBNull(12)
                        ? null
                        : (HoldPointType)reader.GetInt32(12),

                RequiredNdtMethod =
                    reader.IsDBNull(13)
                        ? null
                        : (NdtMethodType)reader.GetInt32(13),

                Mandatory =
                    reader.GetInt32(14) == 1,

                RequiredEvidence =
                    reader.GetString(15),

                ExecutionType =
                    reader.GetString(16),

                Notes =
                    reader.GetString(17)
            };
        }
    }
}
