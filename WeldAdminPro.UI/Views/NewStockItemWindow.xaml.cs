using System.Windows;
using WeldAdminPro.UI.ViewModels;

namespace WeldAdminPro.UI.Views
{
	public partial class NewStockItemWindow : Window
	{
		public NewStockItemWindow(NewStockItemViewModel vm)
		{
			InitializeComponent();
			DataContext = vm;
		}
	}
}
