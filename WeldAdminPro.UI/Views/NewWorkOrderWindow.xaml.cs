using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.Views
{
	public partial class NewWorkOrderWindow : Window
	{
		private readonly WorkOrderRepository _repository;
		private readonly Action _refresh;

		private readonly WorkOrderRepository _workOrderRepository = new();
		private readonly ProjectRepository _projectRepository = new();

		public NewWorkOrderWindow(Action refresh)
		{
			InitializeComponent();

			_repository = new WorkOrderRepository();
			_refresh = refresh;

			LoadProjects();

			StatusBox.SelectedIndex = 0;

			// Default due date (7 days)
			DueDatePicker.SelectedDate = DateTime.Today.AddDays(7);

			DescriptionBox.Focus();
		}

		private void LoadProjects()
		{
			var projects = _projectRepository.GetAll();

			ProjectBox.ItemsSource = projects;
		}

		private void Create_Click(object sender, RoutedEventArgs e)
		{
			if (ProjectBox.SelectedItem == null)
			{
				MessageBox.Show("Please select a project.");
				return;
			}

			var project = (Project)ProjectBox.SelectedItem;

			var workOrder = new WorkOrder
			{
				Id = Guid.NewGuid(),
				ProjectId = project.Id,
				Description = DescriptionBox.Text,
				EstimatedHours = double.TryParse(EstimatedHoursBox.Text, out var hours) ? hours : 8,
				Status = WorkOrderStatus.Ready,
				CreatedOn = DateTime.Now,
				DueDate = DueDatePicker.SelectedDate
			};

			_workOrderRepository.Add(workOrder);

			MessageBox.Show("Work Order created.");

			DialogResult = true;

			_refresh?.Invoke();

			Close();
		}

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				Create_Click(this, new RoutedEventArgs());
			}

			if (e.Key == Key.Escape)
			{
				Close();
			}
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}