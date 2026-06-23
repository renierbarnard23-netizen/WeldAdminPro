using System.Windows.Controls;
using WeldAdminPro.UI.ViewModels;

namespace WeldAdminPro.UI.Views.Quality
{
    public partial class WelderQualificationView : UserControl
    {
        public WelderQualificationView()
        {
            InitializeComponent();

            DataContext =
                new WelderQualificationViewModel();
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}