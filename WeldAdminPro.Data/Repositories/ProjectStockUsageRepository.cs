using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data;

namespace WeldAdminPro.Data.Repositories
{
	public class ProjectStockUsageRepository
	{
		private readonly ApplicationDbContext _context;

		public ProjectStockUsageRepository()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseSqlite($"Data Source={DatabasePath.Get()}")
				.Options;

			_context = new ApplicationDbContext(options);
		}

		public void Add(ProjectStockUsage usage)
		{
			_context.ProjectStockUsages.Add(usage);
			_context.SaveChanges();
		}

		public IEnumerable<ProjectStockUsage> GetByProjectId(Guid projectId)
		{
			return _context.ProjectStockUsages
				.Where(x => x.ProjectId == projectId)
				.OrderByDescending(x => x.IssuedOn)
				.ToList();
		}

		public IEnumerable<ProjectStockUsage> GetByStockItemId(Guid stockItemId)
		{
			return _context.ProjectStockUsages
				.Where(x => x.StockItemId == stockItemId)
				.OrderByDescending(x => x.IssuedOn)
				.ToList();
		}
	}
}
