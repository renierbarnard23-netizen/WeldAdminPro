using System.Windows;
using WeldAdminPro.UI.ViewModels;

namespace WeldAdminPro.UI.Views
{
	public partial class StockInWindow : Window
	{
		public StockInWindow()
		{
			InitializeComponent();
			DataContext = new StockInViewModel();
		}
	}
}
