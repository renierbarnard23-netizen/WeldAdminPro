using System.Windows.Controls;
using WeldAdminPro.UI.ViewModels;

namespace WeldAdminPro.UI.Views
{
	public partial class MaterialCostDriversView : UserControl
	{
		public MaterialCostDriversView()
		{
			InitializeComponent();
			DataContext = new MaterialCostDriversViewModel();
		}
	}
}