using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Analytics.Executive;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
	public class ExecutiveKpiService
	{
		private readonly StockRepository _stockRepository;
		private readonly ProjectRepository _projectRepository;
		private readonly StockTransactionRepository _transactionRepository;

		public ExecutiveKpiService()
		{
			_stockRepository = new StockRepository();
			_projectRepository = new ProjectRepository();
			_transactionRepository = new StockTransactionRepository();
		}

		public List<ExecutiveKpi> BuildKpis(int inventoryHealthScore)
		{
			var items = _stockRepository.GetAll();
			var projects = _projectRepository.GetAll();
			var transactions = _transactionRepository.GetAllTransactions();

			var inventoryValue = items.Sum(i => (decimal)i.Quantity * i.AverageUnitCost);

			var materialSpend = transactions
				.Where(t => t.Type == "OUT")
				.Sum(t => (decimal)t.Quantity * t.UnitCost);

			return new List<ExecutiveKpi>
			{
				new ExecutiveKpi
				{
					Title = "Inventory Health",
					Value = $"{inventoryHealthScore}%",
					Description = "Overall inventory system health"
				},

				new ExecutiveKpi
				{
					Title = "Active Projects",
					Value = projects.Count().ToString(),
					Description = "Total projects in system"
				},

				new ExecutiveKpi
				{
					Title = "Inventory Value",
					Value = $"R {inventoryValue:N0}",
					Description = "Current value of stock"
				},

				new ExecutiveKpi
				{
					Title = "Material Spend",
					Value = $"R {materialSpend:N0}",
					Description = "Total materials issued"
				}
			};
		}
	}
}