using System;
using System.Windows.Controls;
using WeldAdminPro.UI.ViewModels.Quality;

namespace WeldAdminPro.UI.Views.Quality
{
    public partial class HoldPointPanel : UserControl
    {
        private readonly HoldPointViewModel _vm;

        public HoldPointPanel(Guid weldId)
        {
            InitializeComponent();

            _vm = new HoldPointViewModel();

            DataContext = _vm;

            _vm.Load(weldId);
        }
    }
}