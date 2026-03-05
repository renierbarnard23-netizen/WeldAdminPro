using System;
using System.Windows;
using System.Windows.Controls;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.Views
{
	public partial class WorkOrdersView : UserControl
	{
		private readonly WorkOrderRepository _repository = new WorkOrderRepository();

		public WorkOrdersView()
		{
			InitializeComponent();
			LoadWorkOrders();
		}


		private void NewWorkOrder_Click(object sender, RoutedEventArgs e)
		{
			var window = new NewWorkOrderWindow(LoadWorkOrders);

			var result = window.ShowDialog();

			if (result == true)
			{
				LoadWorkOrders();   // 🔥 refresh the grid
			}
		}

		private void IssueMaterial_Click(object sender, RoutedEventArgs e)
		{
			var window = new IssueMaterialWindow();
			window.ShowDialog();
		}
		private void LoadWorkOrders()
		{
			WorkOrdersGrid.ItemsSource = _repository.GetAll();
		}
	}
}