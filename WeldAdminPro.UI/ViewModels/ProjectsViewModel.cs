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

		private readonly ObservableCollection<Project> _allProjects = new();

		[ObservableProperty]
		private ObservableCollection<Project> projects = new();

		[ObservableProperty]
		private Project? selectedProject;

		[ObservableProperty]
		private string searchText = string.Empty;

		public ProjectsViewModel()
		{
			_repository = new ProjectRepository();
			LoadProjects();
		}

		// =========================
		// LOAD + BASE SORT
		// =========================
		[RelayCommand]
		private void LoadProjects()
		{
			var ordered = _repository
				.GetAll()
				.OrderBy(p => p.JobNumber)
				.ToList();

			_allProjects.Clear();
			foreach (var p in ordered)
				_allProjects.Add(p);

			ApplySearchFilter();
		}

		// =========================
		// SEARCH (IN-MEMORY ONLY)
		// =========================
		partial void OnSearchTextChanged(string value)
		{
			ApplySearchFilter();
		}

		private void ApplySearchFilter()
		{
			var text = SearchText?.Trim().ToLower() ?? string.Empty;

			var filtered = string.IsNullOrWhiteSpace(text)
				? _allProjects
				: _allProjects.Where(p =>
					   p.JobNumber.ToString().Contains(text)
					|| p.ProjectName.ToLower().Contains(text)
					|| p.Client.ToLower().Contains(text)
					|| p.ClientRepresentative.ToLower().Contains(text)
					|| p.QuoteNumber.ToLower().Contains(text)
					|| p.OrderNumber.ToLower().Contains(text)
				);

			Projects.Clear();
			foreach (var p in filtered)
				Projects.Add(p);
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
