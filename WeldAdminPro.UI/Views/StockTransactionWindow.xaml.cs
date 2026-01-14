using System.Windows;

namespace WeldAdminPro.UI.Views
{
    public partial class StockTransactionWindow : Window
    {
        public StockTransactionWindow(object vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
