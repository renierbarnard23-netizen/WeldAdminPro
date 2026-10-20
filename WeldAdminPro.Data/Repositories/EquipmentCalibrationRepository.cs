using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Data.Repositories
{
    public class EquipmentCalibrationRepository
    {
        private readonly string _connectionString;
        public EquipmentCalibrationRepository(string connectionString) => _connectionString = connectionString;

        public List<EquipmentCalibration> GetByEquipment(Guid equipmentId)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"
SELECT Id, EquipmentId, CompanyId, CertificateNumber, CalibrationDate, ExpiryDate,
       IsActive, IsApproved, ApprovedBy, ApprovedDate, Provider, Standard, Notes,
       DocumentVaultFileId, CreatedDate
FROM EquipmentCalibrations
WHERE EquipmentId = $EquipmentId
ORDER BY CalibrationDate DESC, CreatedDate DESC;";
            command.Parameters.AddWithValue("$EquipmentId", equipmentId.ToString());
            using var reader = command.ExecuteReader();
            return Read(reader);
        }

        public EquipmentCalibration? GetById(Guid id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"SELECT * FROM EquipmentCalibrations WHERE Id = $Id LIMIT 1;";
            command.Parameters.AddWithValue("$Id", id.ToString());
            using var reader = command.ExecuteReader();
            var records = Read(reader);
            return records.Count > 0 ? records[0] : null;
        }

        public EquipmentCalibration? GetValidForDate(Guid equipmentId, DateTime inspectionDate)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"
SELECT * FROM EquipmentCalibrations
WHERE EquipmentId = $EquipmentId
  AND IsActive = 1
  AND IsApproved = 1
  AND CalibrationDate <= $InspectionDate
  AND ExpiryDate >= $InspectionDate
ORDER BY CalibrationDate DESC, CreatedDate DESC
LIMIT 1;";
            command.Parameters.AddWithValue("$EquipmentId", equipmentId.ToString());
            command.Parameters.AddWithValue("$InspectionDate", inspectionDate.Date.ToString("yyyy-MM-dd"));
            using var reader = command.ExecuteReader();
            var records = Read(reader);
            return records.Count > 0 ? records[0] : null;
        }

        public void Add(EquipmentCalibration calibration)
        {
            Validate(calibration);
            if (calibration.Id == Guid.Empty) calibration.Id = Guid.NewGuid();
            calibration.CreatedDate = calibration.CreatedDate == default ? DateTime.UtcNow : calibration.CreatedDate;

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"
INSERT INTO EquipmentCalibrations
(Id, EquipmentId, CompanyId, CertificateNumber, CalibrationDate, ExpiryDate,
 IsActive, IsApproved, ApprovedBy, ApprovedDate, Provider, Standard, Notes,
 DocumentVaultFileId, CreatedDate)
VALUES
($Id, $EquipmentId, $CompanyId, $CertificateNumber, $CalibrationDate, $ExpiryDate,
 $IsActive, $IsApproved, $ApprovedBy, $ApprovedDate, $Provider, $Standard, $Notes,
 $DocumentVaultFileId, $CreatedDate);";
            AddParameters(command, calibration);
            command.ExecuteNonQuery();
        }

        public void Approve(Guid id, string approvedBy)
        {
            if (id == Guid.Empty) throw new ArgumentException("Calibration ID is required.");
            if (string.IsNullOrWhiteSpace(approvedBy)) throw new ArgumentException("Approving user is required.");
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"
UPDATE EquipmentCalibrations
SET IsApproved = 1, ApprovedBy = $ApprovedBy, ApprovedDate = $ApprovedDate
WHERE Id = $Id;";
            command.Parameters.AddWithValue("$Id", id.ToString());
            command.Parameters.AddWithValue("$ApprovedBy", approvedBy);
            command.Parameters.AddWithValue("$ApprovedDate", DateTime.UtcNow.ToString("O"));
            if (command.ExecuteNonQuery() == 0) throw new InvalidOperationException("Calibration record could not be found.");
        }

        private static void Validate(EquipmentCalibration c)
        {
            if (c.EquipmentId == Guid.Empty) throw new ArgumentException("Equipment is required.");
            if (c.CompanyId == Guid.Empty) throw new ArgumentException("Calibration company is required.");
            if (string.IsNullOrWhiteSpace(c.CertificateNumber)) throw new ArgumentException("Calibration certificate number is required.");
            if (c.ExpiryDate < c.CalibrationDate) throw new ArgumentException("Calibration expiry date cannot precede calibration date.");
        }

        private static void AddParameters(SqliteCommand command, EquipmentCalibration c)
        {
            command.Parameters.AddWithValue("$Id", c.Id.ToString());
            command.Parameters.AddWithValue("$EquipmentId", c.EquipmentId.ToString());
            command.Parameters.AddWithValue("$CompanyId", c.CompanyId.ToString());
            command.Parameters.AddWithValue("$CertificateNumber", c.CertificateNumber.Trim());
            command.Parameters.AddWithValue("$CalibrationDate", c.CalibrationDate.Date.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("$ExpiryDate", c.ExpiryDate.Date.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("$IsActive", c.IsActive ? 1 : 0);
            command.Parameters.AddWithValue("$IsApproved", c.IsApproved ? 1 : 0);
            command.Parameters.AddWithValue("$ApprovedBy", c.ApprovedBy ?? "");
            command.Parameters.AddWithValue("$ApprovedDate", c.ApprovedDate?.ToString("O") ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$Provider", c.Provider ?? "");
            command.Parameters.AddWithValue("$Standard", c.Standard ?? "");
            command.Parameters.AddWithValue("$Notes", c.Notes ?? "");
            command.Parameters.AddWithValue("$DocumentVaultFileId", c.DocumentVaultFileId?.ToString() ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$CreatedDate", c.CreatedDate.ToString("O"));
        }

        private static List<EquipmentCalibration> Read(SqliteDataReader reader)
        {
            var list = new List<EquipmentCalibration>();
            while (reader.Read())
            {
                list.Add(new EquipmentCalibration
                {
                    Id = Guid.Parse(reader["Id"].ToString()!),
                    EquipmentId = Guid.Parse(reader["EquipmentId"].ToString()!),
                    CompanyId = Guid.Parse(reader["CompanyId"].ToString()!),
                    CertificateNumber = reader["CertificateNumber"]?.ToString() ?? "",
                    CalibrationDate = DateTime.Parse(reader["CalibrationDate"].ToString()!),
                    ExpiryDate = DateTime.Parse(reader["ExpiryDate"].ToString()!),
                    IsActive = Convert.ToInt32(reader["IsActive"]) == 1,
                    IsApproved = Convert.ToInt32(reader["IsApproved"]) == 1,
                    ApprovedBy = reader["ApprovedBy"]?.ToString() ?? "",
                    ApprovedDate = reader["ApprovedDate"] == DBNull.Value ? null : DateTime.Parse(reader["ApprovedDate"].ToString()!),
                    Provider = reader["Provider"]?.ToString() ?? "",
                    Standard = reader["Standard"]?.ToString() ?? "",
                    Notes = reader["Notes"]?.ToString() ?? "",
                    DocumentVaultFileId = reader["DocumentVaultFileId"] == DBNull.Value ? null : Guid.Parse(reader["DocumentVaultFileId"].ToString()!),
                    CreatedDate = DateTime.Parse(reader["CreatedDate"].ToString()!)
                });
            }
            return list;
        }
    }
}
