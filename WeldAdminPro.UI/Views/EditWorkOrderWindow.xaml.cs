using System;
using System.Windows;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.Views
{
	public partial class EditWorkOrderWindow : Window
	{
		private readonly WorkOrder _workOrder;
		private readonly WorkOrderRepository _repository = new();

		public EditWorkOrderWindow(WorkOrder workOrder)
		{
			InitializeComponent();

			_workOrder = workOrder;

			// 🔥 STEP 3 — LOAD EXISTING DATA
			DescriptionBox.Text = workOrder.Description;
			EstimatedHoursBox.Text = workOrder.EstimatedHours.ToString();
			DueDatePicker.SelectedDate = workOrder.DueDate;
		}

		// 🔥 STEP 4 — SAVE CHANGES
		private void Save_Click(object sender, RoutedEventArgs e)
		{
			_workOrder.Description = DescriptionBox.Text;

			_workOrder.EstimatedHours =
				double.TryParse(EstimatedHoursBox.Text, out var h) ? h : 8;

			_workOrder.DueDate = DueDatePicker.SelectedDate;

			_repository.Update(_workOrder);

			DialogResult = true;
			Close();
		}
	}
}