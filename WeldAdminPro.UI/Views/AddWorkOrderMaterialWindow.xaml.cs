using System;
using System.Windows;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.UI.Views
{
	public partial class AddWorkOrderMaterialWindow : Window
	{
		private readonly Guid _workOrderId;

		private readonly StockRepository _stockRepository = new();
		private readonly WorkOrderMaterialRepository _repository = new();

		public AddWorkOrderMaterialWindow(Guid workOrderId)
		{
			InitializeComponent();

			_workOrderId = workOrderId;

			LoadMaterials();
		}

		private void LoadMaterials()
		{
			MaterialBox.ItemsSource = _stockRepository.GetAll();
		}

		private void Add_Click(object sender, RoutedEventArgs e)
		{
			if (MaterialBox.SelectedItem == null)
			{
				MessageBox.Show("Select a material.");
				return;
			}

			if (!double.TryParse(QuantityBox.Text, out double qty))
			{
				MessageBox.Show("Enter a valid quantity.");
				return;
			}

			var stock = (StockItem)MaterialBox.SelectedItem;

			var material = new WorkOrderMaterial
			{
				Id = Guid.NewGuid(),
				WorkOrderId = _workOrderId,
				ItemCode = stock.ItemCode,
				RequiredQuantity = qty
			};

			_repository.Add(material);

			MessageBox.Show("Material added to work order.");

			Close();
		}
	}
}