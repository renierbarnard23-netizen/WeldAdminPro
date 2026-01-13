using System.Windows;
using WeldAdminPro.UI.ViewModels;

namespace WeldAdminPro.UI.Views
{
    public partial class NewProjectWindow : Window
    {
        public NewProjectWindow(NewProjectViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;

            vm.RequestClose += () =>
            {
                DialogResult = true;
                Close();
            };
        }
    }
}
