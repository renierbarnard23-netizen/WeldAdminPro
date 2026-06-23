using System.Windows.Controls;

namespace WeldAdminPro.UI.Views
{
    public partial class QualityView : UserControl
    {
        public QualityView()
        {
            InitializeComponent();

            var vm = new QualityViewModel();
            DataContext = vm;

            vm.Load();
        }
    }
}