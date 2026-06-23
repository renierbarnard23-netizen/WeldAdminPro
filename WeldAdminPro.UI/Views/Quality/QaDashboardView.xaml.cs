using System.Windows.Controls;
using WeldAdminPro.UI.ViewModels.Quality;

namespace WeldAdminPro.UI.Views.Quality
{
    public partial class QaDashboardView : UserControl
    {
        public QaDashboardView(
        QaDashboardViewModel viewModel)
        {
            InitializeComponent();

            DataContext = viewModel;
        }
    }

}
