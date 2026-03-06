using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Sqlite;
using Dapper;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Data.Repositories
{
	public class StockTransactionRepository
	{
		private readonly string _connectionString;

		public StockTransactionRepository(string connectionString)
		{
			_connectionString = connectionString;
		}

		private SqliteConnection CreateConnection()
		{
			return new SqliteConnection(_connectionString);
		}

		// ======================================================
		// TRANSACTION METHODS WILL BE MOVED HERE
		// ======================================================

		// AddTransaction
		// GetAllTransactions
		// GetProjectTransactions
		// GetTransactionsByDateRange
		// GetIssuedMaterials
		// GetReturnableItems

	}
}