using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
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
		private readonly ICollectionView _projectsView;

		public ObservableCollection<Project> Projects { get; } = new();

		public ICollectionView ProjectsView => _projectsView;

		[ObservableProperty]
		private Project? selectedProject;

		public ProjectsViewModel()
		{
			_repository = new ProjectRepository();

			_projectsView = CollectionViewSource.GetDefaultView(Projects);
			ApplyDefaultSorting();

			LoadProjects();
		}

		// =========================
		// SORTING
		// =========================
		private void ApplyDefaultSorting()
		{
			_projectsView.SortDescriptions.Clear();

			// 1️⃣ Status priority (Active first)
			_projectsView.SortDescriptions.Add(
				new SortDescription(nameof(Project.StatusSortOrder),
									ListSortDirection.Ascending));

			// 2️⃣ Start Date (nulls last)
			_projectsView.SortDescriptions.Add(
				new SortDescription(nameof(Project.StartDate),
									ListSortDirection.Ascending));

			// 3️⃣ End Date
			_projectsView.SortDescriptions.Add(
				new SortDescription(nameof(Project.EndDate),
									ListSortDirection.Ascending));

			// 4️⃣ Job Number
			_projectsView.SortDescriptions.Add(
				new SortDescription(nameof(Project.JobNumber),
									ListSortDirection.Ascending));
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
