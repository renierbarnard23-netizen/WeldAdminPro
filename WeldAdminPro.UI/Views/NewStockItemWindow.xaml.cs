using System.Windows;
using WeldAdminPro.UI.ViewModels;

namespace WeldAdminPro.UI.Views
{
	public partial class NewStockItemWindow : Window
	{
		private readonly NewStockItemViewModel _vm;

		public NewStockItemWindow(NewStockItemViewModel vm)
		{
			InitializeComponent();
			DataContext = vm;
			_vm = vm;

			Activated += OnActivated;
		}

		private void OnActivated(object? sender, System.EventArgs e)
		{
			_vm.RefreshCategories();
		}
	}
}
