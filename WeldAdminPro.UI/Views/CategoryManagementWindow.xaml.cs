using System.Windows;
using WeldAdminPro.UI.ViewModels;

namespace WeldAdminPro.UI.Views
{
	public partial class CategoryManagementWindow : Window
	{
		public CategoryManagementWindow()
		{
			InitializeComponent();
			DataContext = new CategoryManagementViewModel();
		}

		private void Close_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
