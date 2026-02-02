using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.UI.Views;

namespace WeldAdminPro.UI.ViewModels
{
	public partial class ProjectsViewModel : ObservableObject
	{
		private readonly IProjectRepository _repository;

		[ObservableProperty]
		private ObservableCollection<Project> projects = new();

		[ObservableProperty]
		private Project? selectedProject;

		public ProjectsViewModel()
		{
			_repository = new ProjectRepository();
			LoadProjects();
		}

		[RelayCommand]
		private void LoadProjects()
		{
			var ordered = _repository.GetAll()
				.OrderBy(p => p.Status switch
				{
					ProjectStatus.Active => 0,
					ProjectStatus.Planned => 1,
					ProjectStatus.OnHold => 2,
					ProjectStatus.Completed => 3,
					ProjectStatus.Cancelled => 4,
					_ => 99
				})
				.ThenBy(p => p.StartDate)
				.ThenBy(p => p.JobNumber)
				.ToList();

			Projects = new ObservableCollection<Project>(ordered);
		}

		[RelayCommand(CanExecute = nameof(CanEditProject))]
		private void EditProject()
		{
			if (SelectedProject == null)
				return;

			var vm = new ProjectDetailsViewModel(SelectedProject);
			var window = new ProjectDetailsWindow(vm)
			{
				Owner = System.Windows.Application.Current.MainWindow
			};

			window.ShowDialog();
			LoadProjects();
		}

		[RelayCommand]
		private void NewProject()
		{
			var vm = new NewProjectViewModel();
			var window = new NewProjectWindow(vm)
			{
				Owner = System.Windows.Application.Current.MainWindow
			};

			window.ShowDialog();
			LoadProjects();
		}

		private bool CanEditProject() => SelectedProject != null;

		partial void OnSelectedProjectChanged(Project? value)
		{
			EditProjectCommand.NotifyCanExecuteChanged();
		}
	}
}
