using System.Windows.Controls;
using WeldAdminPro.UI.ViewModels.Quality;

namespace WeldAdminPro.UI.Views.Quality
{
    public partial class CapaManagementView
        : UserControl
    {
        public CapaManagementView()
        {
            InitializeComponent();

            DataContext =
                new CapaManagementViewModel();
        }
    }
}