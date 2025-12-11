using System.Windows.Controls;
using System.Windows;

namespace WeldAdminPro.UI.Views
{
    public partial class ProjectsView : UserControl
    {
        public ProjectsView()
        {
            InitializeComponent();
        }

        private void OnSearchTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (DataContext is ViewModels.ProjectsViewModel vm && sender is TextBox tb)
            {
                vm.Filter = tb.Text;
            }
        }
    }
}
