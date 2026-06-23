using System.Windows.Controls;
using WeldAdminPro.UI.ViewModels.Analytics;

namespace WeldAdminPro.UI.Views.Analytics
{
    public partial class WelderAnalyticsView
        : UserControl
    {
        public WelderAnalyticsView()
        {
            InitializeComponent();

            DataContext =
                new WelderAnalyticsViewModel();
        }
    }
}