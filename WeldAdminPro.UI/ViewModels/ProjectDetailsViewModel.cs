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

		public IReadOnlyList<ProjectStatus> Statuses { get; }

		public event Action? RequestClose;

		public ProjectDetailsViewModel(Project project)
		{
			_repository = new ProjectRepository();
			Project = project;

			Statuses = Enum
				.GetValues(typeof(ProjectStatus))
				.Cast<ProjectStatus>()
				.ToList();
		}

		public ProjectStatus Status
		{
			get => Project.Status;
			set
			{
				if (Project.Status != value)
				{
					Project.Status = value;
					OnPropertyChanged(nameof(Status));
					OnPropertyChanged(nameof(IsEditable));
				}
			}
		}

		/// <summary>
		/// Only Completed projects are locked
		/// </summary>
		public bool IsEditable => Project.Status != ProjectStatus.Completed;

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
