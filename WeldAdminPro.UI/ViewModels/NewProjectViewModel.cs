using System;
using System.Windows;
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

		public NewProjectViewModel()
		{
			_repository = new ProjectRepository();

			project = new Project
			{
				Id = Guid.NewGuid(),
				IsInvoiced = false
			};
		}

		[RelayCommand]
		private void Save()
		{
			try
			{
				_repository.Add(Project);
				CloseWindow();
			}
			catch (Exception ex)
			{
				MessageBox.Show(
					$"Failed to save project.\n\n{ex.Message}",
					"Error",
					MessageBoxButton.OK,
					MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		private void Cancel() => CloseWindow();

		private void CloseWindow()
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
