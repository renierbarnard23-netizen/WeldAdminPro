using System.Windows;
using WeldAdminPro.UI.ViewModels;

namespace WeldAdminPro.UI.Views
{
	public partial class ProjectDetailsWindow : Window
	{
		public ProjectDetailsWindow(ProjectDetailsViewModel vm)
		{
			InitializeComponent();
			DataContext = vm;
			vm.RequestClose += () => Close();
		}
	}
}
