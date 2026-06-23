using System.Windows.Controls;
using WeldAdminPro.UI.ViewModels;

namespace WeldAdminPro.UI.Views.Production
{
    public partial class ProductionControlTowerView
        : UserControl
    {
        public ProductionControlTowerView()
        {
            InitializeComponent();

            var vm =
                new ProductionControlTowerViewModel();

            vm.Load();

            DataContext = vm;
        }
    }
}