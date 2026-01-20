using System.Windows.Controls;
using WeldAdminPro.UI.ViewModels;

namespace WeldAdminPro.UI.Views
{
    public partial class StockTransactionHistoryView : UserControl
    {
        public StockTransactionHistoryView()
        {
            InitializeComponent();
            DataContext = new StockTransactionHistoryViewModel();
        }
    }
}
