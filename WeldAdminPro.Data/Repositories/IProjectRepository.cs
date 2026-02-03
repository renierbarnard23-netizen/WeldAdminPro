using System;
using System.Collections.Generic;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Data.Repositories
{
	public interface IProjectRepository
	{
		IEnumerable<Project> GetAll();
		Project? GetById(Guid id);

		void Add(Project project);
		void Update(Project project);
		void Delete(Guid id);

		int GetNextJobNumber();
		void InitializeJobNumber(int startingNumber);
	}
}
