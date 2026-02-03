using System;
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

		private ObservableCollection<Project> _allProjects = new();

		[ObservableProperty]
		private ObservableCollection<Project> projects = new();

		[ObservableProperty]
		private Project? selectedProject;

		[ObservableProperty]
		private string searchText = string.Empty;

		public bool IsSearchActive => !string.IsNullOrWhiteSpace(SearchText);

		public ProjectsViewModel()
		{
			_repository = new ProjectRepository();
			LoadProjects();
		}

		// =========================
		// LOAD + STATUS-AWARE SORT
		// =========================
		[RelayCommand]
		private void LoadProjects()
		{
			var today = DateTime.Today;

			var sorted = _repository
				.GetAll()
				.OrderBy(p => GetStatusSortOrder(p, today))
				.ThenBy(p => p.JobNumber)
				.ToList();

			_allProjects = new ObservableCollection<Project>(sorted);
			ApplySearch();
		}

		// =========================
		// DERIVED STATUS (TEXT)
		// =========================
		public static string GetStatus(Project p)
		{
			var today = DateTime.Today;

			if (p.StartDate.HasValue && p.StartDate.Value.Date > today)
				return "Planned";

			if (p.StartDate.HasValue &&
				p.StartDate.Value.Date <= today &&
				(!p.EndDate.HasValue || p.EndDate.Value.Date >= today))
				return "Active";

			if (!p.StartDate.HasValue)
				return "Unscheduled";

			if (p.EndDate.HasValue && p.EndDate.Value.Date < today)
				return "Completed";

			return string.Empty;
		}

		private static int GetStatusSortOrder(Project p, DateTime today)
		{
			if (p.StartDate.HasValue && p.StartDate.Value.Date > today)
				return 1; // Planned

			if (p.StartDate.HasValue &&
				p.StartDate.Value.Date <= today &&
				(!p.EndDate.HasValue || p.EndDate.Value.Date >= today))
				return 0; // Active

			if (!p.StartDate.HasValue)
				return 2; // Unscheduled

			if (p.EndDate.HasValue && p.EndDate.Value.Date < today)
				return 3; // Completed

			return 99;
		}

		// =========================
		// SEARCH
		// =========================
		partial void OnSearchTextChanged(string value)
		{
			ApplySearch();
			OnPropertyChanged(nameof(IsSearchActive));
			ClearSearchCommand.NotifyCanExecuteChanged();
		}

		private void ApplySearch()
		{
			if (string.IsNullOrWhiteSpace(SearchText))
			{
				Projects = new ObservableCollection<Project>(_allProjects);
				return;
			}

			var term = SearchText.Trim().ToLower();

			var filtered = _allProjects.Where(p =>
				   p.JobNumber.ToString().Contains(term)
				|| p.ProjectName.ToLower().Contains(term)
				|| p.Client.ToLower().Contains(term)
			);

			Projects = new ObservableCollection<Project>(filtered);
		}

		[RelayCommand(CanExecute = nameof(IsSearchActive))]
		private void ClearSearch()
		{
			SearchText = string.Empty;
			Projects = new ObservableCollection<Project>(_allProjects);
		}

		// =========================
		// UI COMMANDS
		// =========================
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
