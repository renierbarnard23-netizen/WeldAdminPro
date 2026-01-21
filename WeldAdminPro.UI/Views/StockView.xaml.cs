using System.Windows;
using System.Windows.Controls;
using WeldAdminPro.UI.ViewModels;
using WeldAdminPro.UI.Views;

namespace WeldAdminPro.UI.Views
{
    public partial class StockView : UserControl
    {
        public StockView()
        {
            InitializeComponent();
            DataContext = new StockViewModel();
        }
        
		private void ManageCategories_Click(object sender, RoutedEventArgs e)
		{
			var window = new CategoryManagementWindow
			{
				Owner = Window.GetWindow(this)
			};

			window.ShowDialog();

			if (DataContext is StockViewModel vm)
			{
				vm.LoadCategories();
				vm.Reload();
			}
		}

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }

}