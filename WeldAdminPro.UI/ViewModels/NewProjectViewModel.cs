using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.ViewModels
{
	public partial class NewProjectViewModel : ObservableObject
	{
		private readonly IProjectRepository _repository;

		[ObservableProperty]
		private Project project;

		public ObservableCollection<ProjectStatus> Statuses { get; } =
			new(Enum.GetValues<ProjectStatus>());

		public NewProjectViewModel()
		{
			_repository = new ProjectRepository();

			project = new Project
			{
				Status = ProjectStatus.Planned
			};
		}

		[RelayCommand]
		private void Save()
		{
			_repository.Add(Project);
			RequestClose?.Invoke();
		}

		[RelayCommand]
		private void Cancel()
		{
			RequestClose?.Invoke();
		}

		public event Action? RequestClose;
	}
}
