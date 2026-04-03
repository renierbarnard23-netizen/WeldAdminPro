using System.Windows;
using WeldAdminPro.Core.Models;
using WeldAdminPro.UI.ViewModels;

namespace WeldAdminPro.UI.Views
{
	public partial class PurchaseOrderReviewWindow : Window
	{
		public PurchaseOrderReviewWindow(PurchaseOrder po)
		{
			InitializeComponent();

			var vm = new PurchaseOrderReviewViewModel(po);
			vm.RequestClose += () => Close();

			DataContext = vm;
		}
	}
}
