using System.Windows.Controls;
using WeldAdminPro.UI.ViewModels.Quality;

namespace WeldAdminPro.UI.Views.Quality
{
    public partial class TurnoverGovernanceView
        : UserControl
    {
        public TurnoverGovernanceView()
        {
            InitializeComponent();

            DataContext =
                new TurnoverGovernanceViewModel();
        }
    }
}