using System;
using System.Windows;
using WeldAdminPro.Data.Services;

namespace WeldAdminPro.UI.Views
{
	public partial class ProjectMaterialBreakdownWindow : Window
	{
		public ProjectMaterialBreakdownWindow(Guid projectId)
		{
			InitializeComponent();

			var service = new ProjectCostingService();

			DataContext = service.GetProjectMaterialBreakdown(projectId);
		}
	}
}