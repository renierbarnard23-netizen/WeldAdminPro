using System.Windows.Controls;
using WeldAdminPro.UI.ViewModels.Quality;

namespace WeldAdminPro.UI.Views.Quality
{
    public partial class NcrManagementView
        : UserControl
    {
        public NcrManagementView()
        {
            InitializeComponent();

            DataContext =
                new NcrManagementViewModel();
        }
    }
}