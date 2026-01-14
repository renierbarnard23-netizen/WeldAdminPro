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
    }
}
