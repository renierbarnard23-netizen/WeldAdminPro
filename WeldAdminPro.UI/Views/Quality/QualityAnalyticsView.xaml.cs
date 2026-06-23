using System.Windows.Controls;
using WeldAdminPro.UI.ViewModels.Quality;

namespace WeldAdminPro.UI.Views.Quality
{
    public partial class QualityAnalyticsView
        : UserControl
    {
        public QualityAnalyticsView()
        {
            InitializeComponent();

            DataContext =
                new QualityAnalyticsViewModel();
        }
    }
}