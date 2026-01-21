using System.Windows;
using WeldAdminPro.Core.Models;
using WeldAdminPro.UI.ViewModels;

namespace WeldAdminPro.UI.Views
{
	public partial class StockTransactionHistoryWindow : Window
	{
		public StockTransactionHistoryWindow(StockItem item)
		{
			InitializeComponent();
			DataContext = new StockTransactionHistoryViewModel(item);
		}
	}
}
