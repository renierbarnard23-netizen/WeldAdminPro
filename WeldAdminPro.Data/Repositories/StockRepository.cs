using Microsoft.Data.Sqlite;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Data.Repositories
{
    public class StockRepository
    {
        private readonly string _connectionString =
            "Data Source=weldadmin.db";

        public StockRepository()
        {
            EnsureDatabase();
        }

        private void EnsureDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // ---- STOCK ITEMS TABLE ----
            var createItems = connection.CreateCommand();
            createItems.CommandText =
            """
            CREATE TABLE IF NOT EXISTS StockItems (
                Id TEXT PRIMARY KEY,
                StockCode TEXT NOT NULL,
                Description TEXT NOT NULL,
                Category TEXT,
                Unit TEXT,
                MinimumLevel REAL,
                IsActive INTEGER
            );
            """;
            createItems.ExecuteNonQuery();

            // ---- STOCK MOVEMENTS TABLE ----
            var createMovements = connection.CreateCommand();
            createMovements.CommandText =
            """
            CREATE TABLE IF NOT EXISTS StockMovements (
                Id TEXT PRIMARY KEY,
                StockItemId TEXT NOT NULL,
                MovementType TEXT NOT NULL,
                Quantity REAL NOT NULL,
                MovementDate TEXT NOT NULL,
                Reference TEXT,
                Notes TEXT
            );
            """;
            createMovements.ExecuteNonQuery();
        }

        // ===============================
        // STOCK ITEMS
        // ===============================

        public List<StockItem> GetAllItems()
        {
            var list = new List<StockItem>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText =
            """
            SELECT
                Id,
                StockCode,
                Description,
                Category,
                Unit,
                MinimumLevel,
                IsActive
            FROM StockItems;
            """;

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new StockItem
                {
                    Id = Guid.Parse(reader.GetString(0)),
                    StockCode = reader.GetString(1),
                    Description = reader.GetString(2),
                    Category = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    Unit = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    MinimumLevel = reader.IsDBNull(5) ? 0 : reader.GetDouble(5),
                    IsActive = reader.GetInt32(6) == 1
                });
            }

            return list;
        }

        public void AddItem(StockItem item)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText =
            """
            INSERT INTO StockItems (
                Id,
                StockCode,
                Description,
                Category,
                Unit,
                MinimumLevel,
                IsActive
            )
            VALUES (
                $id,
                $code,
                $desc,
                $cat,
                $unit,
                $min,
                $active
            );
            """;

            cmd.Parameters.AddWithValue("$id", item.Id.ToString());
            cmd.Parameters.AddWithValue("$code", item.StockCode);
            cmd.Parameters.AddWithValue("$desc", item.Description);
            cmd.Parameters.AddWithValue("$cat", item.Category);
            cmd.Parameters.AddWithValue("$unit", item.Unit);
            cmd.Parameters.AddWithValue("$min", item.MinimumLevel);
            cmd.Parameters.AddWithValue("$active", item.IsActive ? 1 : 0);

            cmd.ExecuteNonQuery();
        }

        // ===============================
        // STOCK MOVEMENTS
        // ===============================

        public void AddMovement(StockMovement movement)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText =
            """
            INSERT INTO StockMovements (
                Id,
                StockItemId,
                MovementType,
                Quantity,
                MovementDate,
                Reference,
                Notes
            )
            VALUES (
                $id,
                $item,
                $type,
                $qty,
                $date,
                $ref,
                $notes
            );
            """;

            cmd.Parameters.AddWithValue("$id", movement.Id.ToString());
            cmd.Parameters.AddWithValue("$item", movement.StockItemId.ToString());
            cmd.Parameters.AddWithValue("$type", movement.MovementType);
            cmd.Parameters.AddWithValue("$qty", movement.Quantity);
            cmd.Parameters.AddWithValue("$date", movement.MovementDate.ToString("o"));
            cmd.Parameters.AddWithValue("$ref", movement.Reference ?? "");
            cmd.Parameters.AddWithValue("$notes", movement.Notes ?? "");

            cmd.ExecuteNonQuery();
        }

        public double GetCurrentStock(Guid stockItemId)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText =
            """
            SELECT IFNULL(SUM(
                CASE
                    WHEN MovementType = 'IN' THEN Quantity
                    WHEN MovementType = 'OUT' THEN -Quantity
                    ELSE Quantity
                END
            ), 0)
            FROM StockMovements
            WHERE StockItemId = $id;
            """;

            cmd.Parameters.AddWithValue("$id", stockItemId.ToString());

            return Convert.ToDouble(cmd.ExecuteScalar());
        }
    }
}
