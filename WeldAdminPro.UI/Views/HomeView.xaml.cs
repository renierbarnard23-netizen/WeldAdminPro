using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WeldAdminPro.Core.Analytics.Production;
using WeldAdminPro.UI.ViewModels;
using System.Windows.Shapes;
using System.Windows.Media;
using WeldAdminPro.Data.Services;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.UI.Views
{
	public partial class HomeView : UserControl
	{
		public HomeView()
		{
			InitializeComponent();
			DataContext = new HomeViewModel();
			LoadGantt();
		}
		private ProductionQueueItem? _draggedItem;

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
		private void LoadGantt()
		{
			var statusService = new ProductionStatusService();
			statusService.RefreshStatuses();

			var service = new ProductionScheduleService();
			var schedule = service.GetSchedule();

			GanttCanvas.Children.Clear();
			GanttLabels.Children.Clear();

			double pixelsPerDay = 80;

			int row = 0;

			DateTime firstDate = schedule.Min(x => x.StartDate);

			foreach (var item in schedule)
			{
				// Label
				var label = new TextBlock
				{
					Text = item.WorkOrderNumber,
					Margin = new Thickness(0, 10, 0, 10),
					FontWeight = FontWeights.Bold
				};

				GanttLabels.Children.Add(label);

				double startOffset =
					(item.StartDate - firstDate).TotalDays * pixelsPerDay;

				double width =
					item.DurationDays * pixelsPerDay;

				var bar = new Rectangle
				{
					Width = width,
					Height = 20,
					Fill = new SolidColorBrush(Color.FromRgb(70, 130, 180)),
					RadiusX = 3,
					RadiusY = 3
				};

				Canvas.SetLeft(bar, startOffset);
				Canvas.SetTop(bar, row * 30);

				GanttCanvas.Children.Add(bar);

				row++;
			}
		}
	}
}