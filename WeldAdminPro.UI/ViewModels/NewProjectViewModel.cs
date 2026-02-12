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

				// ✅ REQUIRED DEFAULTS
				Status = ProjectStatus.Active,
				IsInvoiced = false,
				CreatedOn = DateTime.Now,
				LastModifiedOn = DateTime.Now,

				// 🔹 Financial Defaults
				Budget = 0m,
				ActualCost = 0m,
				CommittedCost = 0m,
				IsArchived = false
			};
		}

		[RelayCommand]
		private void Save()
		{
			// ================= HARD VALIDATION =================

			if (string.IsNullOrWhiteSpace(Project.ProjectName))
			{
				MessageBox.Show("Project Name is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			if (string.IsNullOrWhiteSpace(Project.Client))
			{
				MessageBox.Show("Client is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			if (Project.Budget < 0)
			{
				MessageBox.Show("Budget cannot be negative.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			// ================= NULL-SAFETY (CRITICAL) =================
			// SQLite DOES NOT allow null parameters

			Project.ClientRepresentative ??= string.Empty;
			Project.QuoteNumber ??= string.Empty;
			Project.OrderNumber ??= string.Empty;
			Project.Material ??= string.Empty;
			Project.AssignedTo ??= string.Empty;
			Project.InvoiceNumber ??= string.Empty;

			// ================= SAVE =================

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
