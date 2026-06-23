using System.Windows.Controls;
using WeldAdminPro.UI.ViewModels;

namespace WeldAdminPro.UI.Views
{
    public partial class ProjectsView : UserControl
    {
        public ProjectsView(
            ProjectsViewModel viewModel)
        {
            InitializeComponent();

            DataContext = viewModel;
        }
    }
}
