using System.Windows.Controls;
using WeldAdminPro.UI.ViewModels.Quality;

namespace WeldAdminPro.UI.Views.Quality
{
    public partial class NcrDashboardView
        : UserControl
    {
        public NcrDashboardView()
        {
            InitializeComponent();

            DataContext =
                new NcrDashboardViewModel();
        }
    }
}