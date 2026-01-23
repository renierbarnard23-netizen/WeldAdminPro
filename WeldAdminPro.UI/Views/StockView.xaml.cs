using System.Windows;
using System.Windows.Controls;
using WeldAdminPro.UI.ViewModels;

namespace WeldAdminPro.UI.Views
{
	public partial class StockView : UserControl
	{
		public StockView()
		{
			InitializeComponent();
			DataContext = new StockViewModel();
		}

		private void ManageCategories_Click(object sender, RoutedEventArgs e)
		{
			var window = new CategoryManagementWindow
			{
				Owner = Window.GetWindow(this)
			};

			window.ShowDialog();

			// ✅ Re-create ViewModel to force full refresh
			DataContext = new StockViewModel();
		}
	}
}
