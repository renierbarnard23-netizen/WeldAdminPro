using System;
using System.Collections.Generic;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Data.Repositories
{
	public interface IProjectRepository
	{
		IEnumerable<Project> GetAll();
		Project? GetById(Guid id);

		/// <summary>
		/// Creates a new project and assigns the next JobNumber automatically.
		/// </summary>
		void Add(Project project);

		void Update(Project project);
		void Delete(Guid id);

		/// <summary>
		/// Returns the next job number WITHOUT incrementing it.
		/// Used for display only.
		/// </summary>
		int GetNextJobNumber();

		/// <summary>
		/// Sets the starting job number ONCE.
		/// If already set, this call should be ignored or throw.
		/// </summary>
		void InitializeJobNumber(int startingNumber);
	}
}
