using System.Windows;
using WeldAdminPro.UI.ViewModels;

namespace WeldAdminPro.UI.Views
{
	public partial class FixDuplicateItemCodesWindow : Window
	{
		public FixDuplicateItemCodesWindow()
		{
			InitializeComponent();

			var vm = new FixDuplicateItemCodesViewModel();
			vm.RequestClose += Close;

			DataContext = vm;
		}
	}
}
