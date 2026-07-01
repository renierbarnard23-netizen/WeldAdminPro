using System;
using System.Windows;
using System.Windows.Input;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Data.Services;

namespace WeldAdminPro.UI.Views
{
	public partial class IssueMaterialWindow : Window
	{
		private readonly WorkOrderRepository _workOrderRepository = new();
		private readonly StockRepository _stockRepository = new();

		public IssueMaterialWindow()
		{
			InitializeComponent();

			LoadWorkOrders();
			LoadMaterials();
		}

		private void LoadWorkOrders()
		{
			var repo = new WorkOrderRepository();

			WorkOrderBox.ItemsSource = repo.GetAll();
		}


		private void LoadMaterials()
		{
			var items = _stockRepository.GetAll();
			MaterialBox.ItemsSource = items;
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
		private void MaterialBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			if (MaterialBox.SelectedItem == null)
				return;

			var item = (StockItem)MaterialBox.SelectedItem;

			var stock = _stockRepository.GetCurrentStock(item.Id);

			StockAvailableText.Text = stock.ToString();
		}

		private void Issue_Click(object sender, RoutedEventArgs e)
		{
			if (MaterialBox.SelectedItem == null)
			{
				MessageBox.Show("Please select a material.");
				return;
			}

			if (WorkOrderBox.SelectedItem == null)
			{
				MessageBox.Show("Please select a work order.");
				return;
			}

			if (!int.TryParse(QuantityBox.Text, out int qty) || qty <= 0)
			{
				MessageBox.Show("Enter a valid quantity greater than zero.");
				return;
			}

			var item = (StockItem)MaterialBox.SelectedItem;
			var workOrder = (WorkOrder)WorkOrderBox.SelectedItem;

			var repo = new StockRepository();
			int available = _stockRepository.GetCurrentStock(item.Id);

			if (qty > available)
			{
				MessageBox.Show($"Not enough stock.\nAvailable: {available}");
				return;
			}

            var projectRepo = new ProjectRepository();
            var project = projectRepo.GetById(workOrder.ProjectId);

            if (project == null)
            {
                MessageBox.Show("Project not found.");
                return;
            }

            var stockService = new StockProjectTransactionService();

            stockService.IssueStock(
                project,
                item,
                qty,
                Environment.UserName);

            MessageBox.Show("Material issued successfully.");
            Close();
		}
		private void QuantityBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			e.Handled = !char.IsDigit(e.Text, 0);
		}
		
	}
}