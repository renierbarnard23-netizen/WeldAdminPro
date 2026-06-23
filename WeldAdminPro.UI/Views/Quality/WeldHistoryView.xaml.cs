using System;
using System.Windows.Controls;
using WeldAdminPro.UI.ViewModels.Quality;

namespace WeldAdminPro.UI.Views.Quality
{
    public partial class WeldHistoryView
    : UserControl
    {
        public WeldHistoryView(
        Guid weldId)
        {
            InitializeComponent();

        DataContext =
            new WeldHistoryViewModel(
                weldId);
        }
    }
}
