using System;
using Microsoft.Data.Sqlite;

namespace WeldAdminPro.Data.Migrations
{
	public static class Migration_StockQuantityConstraint
	{
		public static void Apply(SqliteConnection connection)
		{
			using var transaction = connection.BeginTransaction();

			// 1️⃣ Detect invalid legacy data
			using (var checkCmd = connection.CreateCommand())
			{
				checkCmd.Transaction = transaction;
				checkCmd.CommandText =
					"SELECT COUNT(*) FROM StockItems WHERE Quantity < 0;";

				var count = Convert.ToInt32(checkCmd.ExecuteScalar());
				if (count > 0)
				{
					throw new InvalidOperationException(
						"Database contains stock items with negative quantity. " +
						"Fix the data before applying stock constraints.");
				}
			}

			// 2️⃣ Rename old table
			using (var renameCmd = connection.CreateCommand())
			{
				renameCmd.Transaction = transaction;
				renameCmd.CommandText =
					"ALTER TABLE StockItems RENAME TO StockItems_Old;";
				renameCmd.ExecuteNonQuery();
			}

			// 3️⃣ Create new constrained table
			using (var createCmd = connection.CreateCommand())
			{
				createCmd.Transaction = transaction;
				createCmd.CommandText = @"
                    CREATE TABLE StockItems (
                        Id TEXT PRIMARY KEY,
                        ItemCode TEXT NOT NULL,
                        Description TEXT NOT NULL,
                        Unit TEXT NOT NULL,
                        Quantity INTEGER NOT NULL CHECK (Quantity >= 0)
                    );";
				createCmd.ExecuteNonQuery();
			}

			// 4️⃣ Copy data
			using (var copyCmd = connection.CreateCommand())
			{
				copyCmd.Transaction = transaction;
				copyCmd.CommandText = @"
                    INSERT INTO StockItems (Id, ItemCode, Description, Unit, Quantity)
                    SELECT Id, ItemCode, Description, Unit, Quantity
                    FROM StockItems_Old;";
				copyCmd.ExecuteNonQuery();
			}

			// 5️⃣ Drop old table
			using (var dropCmd = connection.CreateCommand())
			{
				dropCmd.Transaction = transaction;
				dropCmd.CommandText =
					"DROP TABLE StockItems_Old;";
				dropCmd.ExecuteNonQuery();
			}

			transaction.Commit();
		}
	}
}
