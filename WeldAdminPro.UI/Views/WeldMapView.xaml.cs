using System.Windows.Controls;
using WeldAdminPro.UI.ViewModels;

namespace WeldAdminPro.UI.Views
{
    public partial class WeldMapView
        : UserControl
    {
        public WeldMapView()
        {
            InitializeComponent();

            DataContext =
                new WeldMapViewModel();
        }
    }
}