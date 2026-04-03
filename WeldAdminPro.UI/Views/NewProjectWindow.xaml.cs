using System.Windows;
using WeldAdminPro.UI.ViewModels;

namespace WeldAdminPro.UI.Views
{
	public partial class NewProjectWindow : Window
	{
		public NewProjectWindow(NewProjectViewModel viewModel)
		{
			InitializeComponent();
			DataContext = viewModel;
		}
	}
}
