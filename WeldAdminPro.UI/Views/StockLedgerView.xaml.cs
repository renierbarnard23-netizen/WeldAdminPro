using System.Windows.Controls;
using WeldAdminPro.UI.ViewModels;

namespace WeldAdminPro.UI.Views
{
	public partial class StockLedgerView : UserControl
	{
		public StockLedgerView()
		{
			InitializeComponent();
			DataContext = new StockLedgerViewModel();
		}
	}
}
