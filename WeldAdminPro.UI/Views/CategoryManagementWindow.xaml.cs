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
	}
}
