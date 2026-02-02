using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.UI.Views;

namespace WeldAdminPro.UI.ViewModels
{
	public enum ProjectStatusFilter
	{
		All,
		Active,
		Planned,
		OnHold,
		Completed
	}

	public partial class ProjectsViewModel : ObservableObject
	{
		private readonly IProjectRepository _repository;
		private readonly ICollectionView _projectsView;

		public ObservableCollection<Project> Projects { get; } = new();

		public ICollectionView ProjectsView => _projectsView;

		[ObservableProperty]
		private Project? selectedProject;

		[ObservableProperty]
		private ProjectStatusFilter selectedStatusFilter = ProjectStatusFilter.All;

		public ProjectsViewModel()
		{
			_repository = new ProjectRepository();

			_projectsView = CollectionViewSource.GetDefaultView(Projects);
			_projectsView.Filter = ApplyStatusFilter;

			ApplyDefaultSorting();
			LoadProjects();
		}

		// =========================
		// SORTING
		// =========================
		private void ApplyDefaultSorting()
		{
			_projectsView.SortDescriptions.Clear();

			// Status priority (Active first)
			_projectsView.SortDescriptions.Add(
				new SortDescription(nameof(Project.StatusSortOrder),
					ListSortDirection.Ascending));

			// Start Date
			_projectsView.SortDescriptions.Add(
				new SortDescription(nameof(Project.StartDate),
					ListSortDirection.Ascending));

			// End Date
			_projectsView.SortDescriptions.Add(
				new SortDescription(nameof(Project.EndDate),
					ListSortDirection.Ascending));

			// Job Number
			_projectsView.SortDescriptions.Add(
				new SortDescription(nameof(Project.JobNumber),
					ListSortDirection.Ascending));
		}

		// =========================
		// FILTERING
		// =========================
		private bool ApplyStatusFilter(object obj)
		{
			if (obj is not Project project)
				return false;

			return SelectedStatusFilter switch
			{
				ProjectStatusFilter.Active => project.Status == ProjectStatus.Active,
				ProjectStatusFilter.Planned => project.Status == ProjectStatus.Planned,
				ProjectStatusFilter.OnHold => project.Status == ProjectStatus.OnHold,
				ProjectStatusFilter.Completed => project.Status == ProjectStatus.Completed,
				_ => true
			};
		}

		partial void OnSelectedStatusFilterChanged(ProjectStatusFilter value)
		{
			_projectsView.Refresh();
		}

		// =========================
		// LOAD
		// =========================
		[RelayCommand]
		private void LoadProjects()
		{
			Projects.Clear();

			foreach (var project in _repository.GetAll())
				Projects.Add(project);

			_projectsView.Refresh();
		}

		// =========================
		// COMMANDS
		// =========================
		[RelayCommand]
		private void ShowAll() => SelectedStatusFilter = ProjectStatusFilter.All;

		[RelayCommand]
		private void ShowActive() => SelectedStatusFilter = ProjectStatusFilter.Active;

		[RelayCommand]
		private void ShowPlanned() => SelectedStatusFilter = ProjectStatusFilter.Planned;

		[RelayCommand]
		private void ShowOnHold() => SelectedStatusFilter = ProjectStatusFilter.OnHold;

		[RelayCommand]
		private void ShowCompleted() => SelectedStatusFilter = ProjectStatusFilter.Completed;

		[RelayCommand]
		private void NewProject()
		{
			var vm = new NewProjectViewModel();

			var window = new NewProjectWindow(vm)
			{
				Owner = System.Windows.Application.Current.MainWindow,
				Title = "New Project"
			};

			window.ShowDialog();
			LoadProjects();
		}

		[RelayCommand(CanExecute = nameof(CanEditProject))]
		private void EditProject()
		{
			if (SelectedProject == null)
				return;

			var vm = new ProjectDetailsViewModel(SelectedProject);

			var window = new ProjectDetailsWindow(vm)
			{
				Owner = System.Windows.Application.Current.MainWindow,
				Title = "Project Details"
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
