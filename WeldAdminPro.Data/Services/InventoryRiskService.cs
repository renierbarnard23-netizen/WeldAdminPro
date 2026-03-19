using System;
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
	public class InventoryRiskService
	{
		private readonly StockRepository _stockRepository;
		private readonly StockTransactionRepository _transactionRepository;

		public InventoryRiskService()
		{
			_stockRepository = new StockRepository();
			_transactionRepository = new StockTransactionRepository();
		}

		// =========================================================
		// NEGATIVE STOCK DETECTION
		// =========================================================

		public List<Guid> GetNegativeStockItems()
		{
			var items = _stockRepository.GetAll();

			return items
				.Where(i => i.Quantity < 0)
				.Select(i => i.Id)
				.ToList();
		}

		// =========================================================
		// DEAD STOCK DETECTION
		// =========================================================

		public List<Guid> GetDeadStockItems(int days = 90)
		{
			var items = _stockRepository.GetAll();
			var transactions = _transactionRepository.GetAllTransactions();

			var cutoff = DateTime.Now.AddDays(-days);

			return items
				.Where(item =>
					!transactions.Any(t =>
						t.StockItemId == item.Id &&
						t.TransactionDate >= cutoff))
				.Select(i => i.Id)
				.ToList();
		}

		// =========================================================
		// HIGH CONSUMPTION SPIKE
		// =========================================================

		public List<Guid> DetectConsumptionSpikes(int days = 30)
		{
			var transactions = _transactionRepository.GetAllTransactions();

			var cutoff = DateTime.Now.AddDays(-days);

			var grouped = transactions
				.Where(t => t.Type == "OUT")
				.GroupBy(t => t.StockItemId);

			var result = new List<Guid>();

			foreach (var group in grouped)
			{
				var recent = group
					.Where(t => t.TransactionDate >= cutoff)
					.Sum(t => t.Quantity);

				var historical = group
					.Where(t => t.TransactionDate < cutoff)
					.Sum(t => t.Quantity);

				if (historical > 0 && recent > historical * 0.5)
					result.Add(group.Key);
			}

			return result;
		}

		// =========================================================
		// CRITICAL INVENTORY CONCENTRATION
		// =========================================================

		public List<Guid> DetectCriticalStockConcentration()
		{
			var items = _stockRepository.GetAll();

			var totalValue = items.Sum(i => (decimal)i.Quantity * i.AverageUnitCost);

			if (totalValue == 0)
				return new List<Guid>();

			return items
				.Where(i => ((decimal)i.Quantity * i.AverageUnitCost) / totalValue > 0.2m)
				.Select(i => i.Id)
				.ToList();
		}
	}
}