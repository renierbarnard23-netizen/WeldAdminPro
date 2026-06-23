using System.Windows.Controls;
using WeldAdminPro.UI.ViewModels;

namespace WeldAdminPro.UI.Views.Quality
{
    public partial class WeldTraceabilityView
        : UserControl
    {
        public WeldTraceabilityView()
        {
            InitializeComponent();

            DataContext =
                new WeldTraceabilityViewModel();
        }
    }
}
