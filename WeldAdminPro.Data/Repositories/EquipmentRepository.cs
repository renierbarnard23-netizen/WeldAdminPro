using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Data.Repositories
{
    public class EquipmentRepository
    {
        private readonly string _connectionString;

        public EquipmentRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<Equipment> GetByCompany(Guid companyId, bool activeOnly = false)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"
SELECT Id, CompanyId, EquipmentNumber, Description, EquipmentType,
       Manufacturer, Model, SerialNumber, IsActive, CalibrationRequired,
       CalibrationIntervalDays, Notes, CreatedDate, LastModifiedDate
FROM Equipment
WHERE CompanyId = $CompanyId
  AND ($ActiveOnly = 0 OR IsActive = 1)
ORDER BY EquipmentNumber;";
            command.Parameters.AddWithValue("$CompanyId", companyId.ToString());
            command.Parameters.AddWithValue("$ActiveOnly", activeOnly ? 1 : 0);
            using var reader = command.ExecuteReader();
            return ReadEquipment(reader);
        }

        public Equipment? GetById(Guid id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"
SELECT Id, CompanyId, EquipmentNumber, Description, EquipmentType,
       Manufacturer, Model, SerialNumber, IsActive, CalibrationRequired,
       CalibrationIntervalDays, Notes, CreatedDate, LastModifiedDate
FROM Equipment
WHERE Id = $Id
LIMIT 1;";
            command.Parameters.AddWithValue("$Id", id.ToString());
            using var reader = command.ExecuteReader();
            var equipment = ReadEquipment(reader);
            return equipment.Count > 0 ? equipment[0] : null;
        }

        public void Add(Equipment equipment)
        {
            Validate(equipment);
            if (equipment.Id == Guid.Empty) equipment.Id = Guid.NewGuid();
            equipment.EquipmentNumber = equipment.EquipmentNumber.Trim().ToUpperInvariant();
            equipment.CreatedDate = equipment.CreatedDate == default ? DateTime.UtcNow : equipment.CreatedDate;

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"
INSERT INTO Equipment
(Id, CompanyId, EquipmentNumber, Description, EquipmentType, Manufacturer,
 Model, SerialNumber, IsActive, CalibrationRequired, CalibrationIntervalDays,
 Notes, CreatedDate, LastModifiedDate)
VALUES
($Id, $CompanyId, $EquipmentNumber, $Description, $EquipmentType, $Manufacturer,
 $Model, $SerialNumber, $IsActive, $CalibrationRequired, $CalibrationIntervalDays,
 $Notes, $CreatedDate, $LastModifiedDate);";
            AddParameters(command, equipment);
            command.ExecuteNonQuery();
        }

        public void Update(Equipment equipment)
        {
            Validate(equipment);
            equipment.EquipmentNumber = equipment.EquipmentNumber.Trim().ToUpperInvariant();
            equipment.LastModifiedDate = DateTime.UtcNow;

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"
UPDATE Equipment
SET EquipmentNumber = $EquipmentNumber,
    Description = $Description,
    EquipmentType = $EquipmentType,
    Manufacturer = $Manufacturer,
    Model = $Model,
    SerialNumber = $SerialNumber,
    IsActive = $IsActive,
    CalibrationRequired = $CalibrationRequired,
    CalibrationIntervalDays = $CalibrationIntervalDays,
    Notes = $Notes,
    LastModifiedDate = $LastModifiedDate
WHERE Id = $Id AND CompanyId = $CompanyId;";
            AddParameters(command, equipment);
            if (command.ExecuteNonQuery() == 0)
                throw new InvalidOperationException("Equipment could not be found or is not owned by the company.");
        }

        private static void Validate(Equipment equipment)
        {
            if (equipment.CompanyId == Guid.Empty) throw new ArgumentException("Equipment company is required.");
            if (string.IsNullOrWhiteSpace(equipment.EquipmentNumber)) throw new ArgumentException("Equipment number is required.");
            if (string.IsNullOrWhiteSpace(equipment.Description)) throw new ArgumentException("Equipment description is required.");
            if (equipment.CalibrationIntervalDays < 1) throw new ArgumentException("Calibration interval must be at least one day.");
        }

        private static void AddParameters(SqliteCommand command, Equipment e)
        {
            command.Parameters.AddWithValue("$Id", e.Id.ToString());
            command.Parameters.AddWithValue("$CompanyId", e.CompanyId.ToString());
            command.Parameters.AddWithValue("$EquipmentNumber", e.EquipmentNumber);
            command.Parameters.AddWithValue("$Description", e.Description ?? "");
            command.Parameters.AddWithValue("$EquipmentType", e.EquipmentType ?? "");
            command.Parameters.AddWithValue("$Manufacturer", e.Manufacturer ?? "");
            command.Parameters.AddWithValue("$Model", e.Model ?? "");
            command.Parameters.AddWithValue("$SerialNumber", e.SerialNumber ?? "");
            command.Parameters.AddWithValue("$IsActive", e.IsActive ? 1 : 0);
            command.Parameters.AddWithValue("$CalibrationRequired", e.CalibrationRequired ? 1 : 0);
            command.Parameters.AddWithValue("$CalibrationIntervalDays", e.CalibrationIntervalDays);
            command.Parameters.AddWithValue("$Notes", e.Notes ?? "");
            command.Parameters.AddWithValue("$CreatedDate", e.CreatedDate.ToString("O"));
            command.Parameters.AddWithValue("$LastModifiedDate", e.LastModifiedDate?.ToString("O") ?? (object)DBNull.Value);
        }

        private static List<Equipment> ReadEquipment(SqliteDataReader reader)
        {
            var list = new List<Equipment>();
            while (reader.Read())
            {
                list.Add(new Equipment
                {
                    Id = Guid.Parse(reader["Id"].ToString()!),
                    CompanyId = Guid.Parse(reader["CompanyId"].ToString()!),
                    EquipmentNumber = reader["EquipmentNumber"]?.ToString() ?? "",
                    Description = reader["Description"]?.ToString() ?? "",
                    EquipmentType = reader["EquipmentType"]?.ToString() ?? "",
                    Manufacturer = reader["Manufacturer"]?.ToString() ?? "",
                    Model = reader["Model"]?.ToString() ?? "",
                    SerialNumber = reader["SerialNumber"]?.ToString() ?? "",
                    IsActive = Convert.ToInt32(reader["IsActive"]) == 1,
                    CalibrationRequired = Convert.ToInt32(reader["CalibrationRequired"]) == 1,
                    CalibrationIntervalDays = Convert.ToInt32(reader["CalibrationIntervalDays"]),
                    Notes = reader["Notes"]?.ToString() ?? "",
                    CreatedDate = DateTime.Parse(reader["CreatedDate"].ToString()!),
                    LastModifiedDate = reader["LastModifiedDate"] == DBNull.Value ? null : DateTime.Parse(reader["LastModifiedDate"].ToString()!)
                });
            }
            return list;
        }
    }
}
