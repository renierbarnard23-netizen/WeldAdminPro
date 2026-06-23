using System.Windows.Controls;
using WeldAdminPro.UI.ViewModels;

namespace WeldAdminPro.UI.Views
{
    public partial class ProjectComplianceDashboard : UserControl
    {
        public ProjectComplianceDashboard()
        {
            InitializeComponent();
            DataContext = new ProjectComplianceDashboardViewModel();
        }
    }
}