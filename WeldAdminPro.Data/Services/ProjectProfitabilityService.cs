using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
	public class ProjectProfitabilityService
	{
		private readonly StockRepository _stockRepository;
		private readonly ProjectRepository _projectRepository;

		public ProjectProfitabilityService()
		{
			_stockRepository = new StockRepository();
			_projectRepository = new ProjectRepository();
		}

		public List<ProjectProfitability> GetProjectProfitability()
		{
			var projects = _projectRepository.GetAll();
			var transactions = _stockRepository.GetAllTransactions();

			var results = new List<ProjectProfitability>();

			foreach (var project in projects)
			{
				var materialCost = transactions
					.Where(t => t.ProjectId == project.Id && t.Type == "OUT")
					.Sum(t => t.TransactionValue);

				results.Add(new ProjectProfitability
				{
					ProjectId = project.Id,
					ProjectName = project.ProjectName,
					Revenue = project.Budget,
					MaterialCost = materialCost,
					LabourCost = 0 // will improve later
				});
			}

			return results;
		}
	}
}