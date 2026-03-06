using System.Windows.Controls;
using WeldAdminPro.UI.ViewModels;

namespace WeldAdminPro.UI.Views
{
	public partial class HomeView : UserControl
	{
		public HomeView()
		{
			InitializeComponent();
			DataContext = new HomeViewModel();
		}
	}
}