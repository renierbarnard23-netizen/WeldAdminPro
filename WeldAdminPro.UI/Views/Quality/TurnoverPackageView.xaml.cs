using System.Windows.Controls;
using WeldAdminPro.UI.ViewModels.Quality;

namespace WeldAdminPro.UI.Views.Quality
{
    public partial class TurnoverPackageView
        : UserControl
    {
        public TurnoverPackageView()
        {
            InitializeComponent();

            DataContext =
                new TurnoverPackageViewModel();
        }
    }
}