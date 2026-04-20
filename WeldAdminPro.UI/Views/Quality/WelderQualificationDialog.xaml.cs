using System.Windows;

namespace WeldAdminPro.UI.Views.Quality
{
    public partial class WelderQualificationDialog : Window
    {
        public WelderQualificationDialog()
        {
            InitializeComponent();

            if (DataContext is ViewModels.WelderQualificationDialogViewModel vm)
                vm.CloseAction = Close;
        }
    }
}