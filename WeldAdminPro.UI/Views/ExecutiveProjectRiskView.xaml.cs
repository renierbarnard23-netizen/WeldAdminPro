using System.Windows.Controls;
using WeldAdminPro.UI.ViewModels;

namespace WeldAdminPro.UI.Views
{
	public partial class ExecutiveProjectRiskView : UserControl
	{
		public ExecutiveProjectRiskView()
		{
			InitializeComponent();
			DataContext = new ExecutiveProjectRiskViewModel();
		}
	}
}