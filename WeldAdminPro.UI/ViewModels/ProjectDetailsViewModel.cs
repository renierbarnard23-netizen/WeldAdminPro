using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.ViewModels
{
	public partial class ProjectDetailsViewModel : ObservableObject
	{
		private readonly IProjectRepository _repository;

		public Project Project { get; }

		// ✅ THIS is what populates the Status dropdown
		public IReadOnlyList<ProjectStatus> Statuses { get; }

		public event Action? RequestClose;

		public ProjectDetailsViewModel(Project project)
		{
			_repository = new ProjectRepository();
			Project = project;

			// ✅ Populate enum values explicitly
			Statuses = Enum
				.GetValues(typeof(ProjectStatus))
				.Cast<ProjectStatus>()
				.ToList();
		}

		[RelayCommand]
		private void Save()
		{
			_repository.Update(Project);
			RequestClose?.Invoke();
		}

		[RelayCommand]
		private void Cancel()
		{
			RequestClose?.Invoke();
		}
	}
}
