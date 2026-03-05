using System.Windows.Controls;
using WeldAdminPro.UI.ViewModels;

namespace WeldAdminPro.UI.Views
{
	public partial class ExecutiveStockForecastView : UserControl
	{
		public ExecutiveStockForecastView()
		{
			InitializeComponent();
			DataContext = new ExecutiveStockForecastViewModel();
		}
	}
}