using Microsoft.Data.Sqlite;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Data.Repositories
{
    public class StockTransactionRepository
    {
        private string _connectionString => $"Data Source={DatabasePath.Get()}";

        // =========================
        // ADD TRANSACTION
        // =========================
        public void AddTransaction(StockTransaction tx)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
INSERT INTO StockTransactions 
(Id, StockItemId, ProjectId, TransactionDate, Quantity, Type, UnitCost, Reference, BalanceAfter)
VALUES 
($id, $stockItemId, $projectId, $date, $qty, $type, $cost, $ref, $balance);";

            cmd.Parameters.AddWithValue("$id", tx.Id.ToString());
            cmd.Parameters.AddWithValue("$stockItemId", tx.StockItemId.ToString());
            cmd.Parameters.AddWithValue("$projectId", (object?)tx.ProjectId?.ToString() ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$date", tx.TransactionDate.ToString("o"));
            cmd.Parameters.AddWithValue("$qty", tx.Quantity);
            cmd.Parameters.AddWithValue("$type", tx.Type);
            cmd.Parameters.AddWithValue("$cost", (double)tx.UnitCost);
            cmd.Parameters.AddWithValue("$ref", tx.Reference ?? "");
            cmd.Parameters.AddWithValue("$balance", tx.BalanceAfter);

            cmd.ExecuteNonQuery();
        }

        // =========================
        // GET ALL
        // =========================
        public List<StockTransaction> GetAllTransactions()
        {
            var list = new List<StockTransaction>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT 
                    st.Id,
                    st.StockItemId,
                    st.ProjectId,
                    st.TransactionDate,
                    st.Quantity,
                    st.Type,
                    st.UnitCost,
                    st.Reference,
                    st.BalanceAfter,
                    p.ProjectName,
                    si.ItemCode,
                    si.Description
                FROM StockTransactions st
                LEFT JOIN Projects p ON st.ProjectId = p.Id
                LEFT JOIN StockItems si ON st.StockItemId = si.Id
                ORDER BY st.TransactionDate;"; ;

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(Map(reader));
            }

            return list;
        }

        // =========================
        // UPDATE BALANCE
        // =========================
        public void UpdateTransactionBalance(Guid id, int balance)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
UPDATE StockTransactions 
SET BalanceAfter = $balance
WHERE Id = $id;";

            cmd.Parameters.AddWithValue("$id", id.ToString());
            cmd.Parameters.AddWithValue("$balance", balance);

            cmd.ExecuteNonQuery();
        }

        // =========================
        // BY PROJECT
        // =========================
        public List<StockTransaction> GetProjectTransactions(Guid projectId)
        {
            return GetAllTransactions()
                .Where(t => t.ProjectId == projectId)
                .ToList();
        }

        // =========================
        // ISSUED MATERIALS
        // =========================
        public List<StockTransaction> GetIssuedMaterials(Guid projectId)
        {
            return GetAllTransactions()
                .Where(t => t.Type == "OUT" && t.ProjectId == projectId)
                .ToList();
        }

        // =========================
        // RETURNABLE ITEMS
        // =========================
        public List<ReturnableItemDto> GetReturnableItems(Guid projectId)
        {
            var transactions =
                GetProjectTransactions(projectId);

            var list =
                transactions
                    .GroupBy(x => x.StockItemId)
                    .Select(g =>
                    {
                        var issued =
                            g.Where(x =>
                                    x.Type == "OUT")
                             .Sum(x => x.Quantity);

                        var returned =
                            g.Where(x =>
                                    x.Type == "IN" ||
                                    x.Type == "RET")
                             .Sum(x => x.Quantity);

                        var remaining =
                            issued - returned;

                        if (remaining <= 0)
                            return null;

                        var latest =
                            g.Last();

                        return new ReturnableItemDto
                        {
                            StockItemId =
                                g.Key,

                            Quantity =
                                remaining,

                            UnitCost =
                                latest.UnitCost,

                            ItemCode =
                                latest.ItemCode,

                            Description =
                                latest.ItemDescription
                        };
                    })
                    .Where(x => x != null)
                    .Cast<ReturnableItemDto>()
                    .ToList();

            return list;
        }

        // =========================
        // DATE RANGE
        // =========================
        public List<StockTransaction> GetTransactionsByDateRange(DateTime? from, DateTime? to)
        {
            var list = GetAllTransactions();

            if (from.HasValue)
                list = list.Where(t => t.TransactionDate >= from.Value).ToList();

            if (to.HasValue)
                list = list.Where(t => t.TransactionDate <= to.Value).ToList();

            return list;
        }

        // =========================
        // MAPPER
        // =========================
        private StockTransaction Map(SqliteDataReader reader)
        {
            return new StockTransaction
            {
                Id = Guid.Parse(reader.GetString(0)),
                StockItemId = Guid.Parse(reader.GetString(1)),
                ProjectId = reader.IsDBNull(2) ? null : Guid.Parse(reader.GetString(2)),
                TransactionDate = DateTime.Parse(reader.GetString(3)),
                Quantity = reader.GetInt32(4),
                Type = reader.GetString(5),
                UnitCost = (decimal)reader.GetDouble(6),
                Reference = reader.IsDBNull(7) ? "" : reader.GetString(7),
                BalanceAfter = reader.IsDBNull(8) ? 0 : reader.GetInt32(8),

                // Already added
                ProjectName = reader.IsDBNull(9) ? null : reader.GetString(9),

                // 🔥 ADD THESE
                ItemCode = reader.IsDBNull(10) ? "" : reader.GetString(10),
                ItemDescription = reader.IsDBNull(11) ? "" : reader.GetString(11)
            };
        }
    }
}