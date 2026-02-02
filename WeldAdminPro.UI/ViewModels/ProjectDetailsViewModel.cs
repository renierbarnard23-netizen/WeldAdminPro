using System;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.ViewModels
{
	public partial class ProjectDetailsViewModel : ObservableObject
	{
		private readonly IProjectRepository _repository;

		[ObservableProperty]
		private Project project;

		public ProjectDetailsViewModel(Project project)
		{
			Project = project ?? throw new ArgumentNullException(nameof(project));
			_repository = new ProjectRepository();
		}

		[RelayCommand]
		private void Save()
		{
			_repository.Update(Project);
			Close();
		}

		[RelayCommand]
		private void Close()
		{
			foreach (Window window in Application.Current.Windows)
			{
				if (window.DataContext == this)
				{
					window.Close();
					break;
				}
			}
		}
	}
}
