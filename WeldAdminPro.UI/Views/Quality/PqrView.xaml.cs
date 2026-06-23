using System.Windows.Controls;
using WeldAdminPro.UI.ViewModels.Quality;

namespace WeldAdminPro.UI.Views.Quality
{
    public partial class PqrView : UserControl
    {
        public PqrView()
        {
            InitializeComponent();
            DataContext = new PqrViewModel();
        }
    }
}