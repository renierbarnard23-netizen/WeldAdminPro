using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WeldAdminPro.Core.Analytics.Production;
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
		private ProductionQueueItem _draggedItem;

		private void Queue_MouseDown(object sender, MouseButtonEventArgs e)
		{
			var grid = sender as DataGrid;

			_draggedItem = grid?.SelectedItem as ProductionQueueItem;

			if (_draggedItem != null)
			{
				DragDrop.DoDragDrop(grid, _draggedItem, DragDropEffects.Move);
			}
		}

		private void Queue_Drop(object sender, DragEventArgs e)
		{
			if (!e.Data.GetDataPresent(typeof(ProductionQueueItem)))
				return;

			var droppedItem = e.Data.GetData(typeof(ProductionQueueItem)) as ProductionQueueItem;

			var vm = DataContext as HomeViewModel;

			if (vm == null || droppedItem == null)
				return;

			var list = vm.ProductionQueue.ToList();

			list.Remove(droppedItem);
			list.Insert(0, droppedItem);

			vm.ProductionQueue =
				new System.Collections.ObjectModel.ObservableCollection<ProductionQueueItem>(list);
		}
	}
}