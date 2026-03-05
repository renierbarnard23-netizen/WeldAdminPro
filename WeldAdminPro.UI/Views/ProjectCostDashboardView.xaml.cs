using System.Windows.Controls;
using WeldAdminPro.Core.Models;
using WeldAdminPro.UI.ViewModels;

namespace WeldAdminPro.UI.Views
{
	public partial class ProjectCostDashboardView : UserControl
	{
		public ProjectCostDashboardView()
		{
			InitializeComponent();
			DataContext = new ProjectCostDashboardViewModel();
		}
		private void ProjectGrid_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (sender is DataGrid grid && grid.SelectedItem is ProjectCostSummary project)
			{
				var window = new ProjectMaterialBreakdownWindow(project.ProjectId);
				window.ShowDialog();
			}
		}
	}
}