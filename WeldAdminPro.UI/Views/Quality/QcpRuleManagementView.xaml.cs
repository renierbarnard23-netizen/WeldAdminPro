using System.Windows.Controls;
using WeldAdminPro.UI.ViewModels.Quality;

namespace WeldAdminPro.UI.Views.Quality
{
    public partial class QcpRuleManagementView
        : UserControl
    {
        public QcpRuleManagementView()
        {
            InitializeComponent();

            DataContext =
                new QcpRuleManagementViewModel();
        }
    }
}