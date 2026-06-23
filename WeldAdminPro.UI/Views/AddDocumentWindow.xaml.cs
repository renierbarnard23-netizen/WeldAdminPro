using System.Windows;
using WeldAdminPro.UI.ViewModels;

namespace WeldAdminPro.UI.Views
{
    public partial class AddDocumentWindow
        : Window
    {
        public AddDocumentWindow()
        {
            InitializeComponent();

            var vm =
                new AddDocumentViewModel();

            vm.OnSaved += _ =>
            {
                DialogResult = true;

                Close();
            };

            DataContext = vm;
        }
    }
}
