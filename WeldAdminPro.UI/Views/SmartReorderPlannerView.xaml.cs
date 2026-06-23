using System.Windows.Controls;
using WeldAdminPro.UI.ViewModels;

namespace WeldAdminPro.UI.Views
{
	public partial class SmartReorderPlannerView : UserControl
	{
		public SmartReorderPlannerView()
		{
			InitializeComponent();
			DataContext = new SmartReorderPlannerViewModel();
		}
	}
}
