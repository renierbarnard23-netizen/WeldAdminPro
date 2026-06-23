using System.Windows.Controls;
using WeldAdminPro.UI.ViewModels;

namespace WeldAdminPro.UI.Views
{
	public partial class ProjectProfitabilityView : UserControl
	{
		public ProjectProfitabilityView()
		{
			InitializeComponent();

			DataContext = new ProjectProfitabilityViewModel();
		}
	}
}