using System.Windows.Controls;
using WeldAdminPro.UI.ViewModels.Quality;

namespace WeldAdminPro.UI.Views.Quality
{
    public partial class WeldControlTowerView
        : UserControl
    {
        public WeldControlTowerView()
        {
            InitializeComponent();

            DataContext =
                new WeldControlTowerViewModel();
        }
    }
}