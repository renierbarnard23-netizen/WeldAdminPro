using System.Windows.Controls;
using WeldAdminPro.UI.ViewModels;

namespace WeldAdminPro.UI.Views
{
	public partial class ExecutiveMaterialTrendsView : UserControl
	{
		public ExecutiveMaterialTrendsView()
		{
			InitializeComponent();
			DataContext = new ExecutiveMaterialTrendsViewModel();
		}
	}
}