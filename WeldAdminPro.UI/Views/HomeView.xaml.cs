using System;
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using WeldAdminPro.Core.Analytics.Production;
using WeldAdminPro.Core.Enums;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Data.Services;
using WeldAdminPro.UI.ViewModels;

namespace WeldAdminPro.UI.Views
{
	public partial class HomeView : UserControl
	{
        public HomeView()
        {
            InitializeComponent();

            Loaded += HomeView_Loaded;
        }

        private void HomeView_Loaded(
			object sender,
				RoutedEventArgs e)
        {
            if (DataContext is HomeViewModel vm)
            {
                vm.ProductionChanged -= Vm_ProductionChanged;
                vm.ProductionChanged += Vm_ProductionChanged;
            }

            LoadGantt();
        }

        private void Vm_ProductionChanged()
        {
            Dispatcher.Invoke(() =>
            {
                LoadGantt();
            });
        }

        private ProductionQueueItem? _draggedItem;
        private bool _isLoadingGantt;

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

		

        private void DrawTimelineGrid(
    DateTime startDate,
    int days)
        {
            double dayWidth = 80;

            for (int i = 0; i < days; i++)
            {
                var x = i * dayWidth;

                var date =
                    startDate.AddDays(i);

                if (date.DayOfWeek ==
                        DayOfWeek.Saturday ||
                    date.DayOfWeek ==
                        DayOfWeek.Sunday)
                {
                    var rect =
                        new Rectangle
                        {
                            Width = dayWidth,
                            Height =
                                GanttCanvas.Height,
                            Fill =
                                new SolidColorBrush(
                                    Color.FromArgb(
                                        20,
                                        200,
                                        200,
                                        200))
                        };

                    Canvas.SetLeft(rect, x);
                    Canvas.SetTop(rect, 0);

                    GanttCanvas.Children.Add(rect);
                }

                var line =
                    new Line
                    {
                        X1 = x,
                        Y1 = 0,
                        X2 = x,
                        Y2 = 600,
                        Stroke =
                            Brushes.LightGray,
                        StrokeThickness = 1
                    };

                GanttCanvas.Children.Add(line);

                var label =
                    new TextBlock
                    {
                        Text =
                            date.ToString("dd MMM"),
                        FontSize = 12
                    };

                Canvas.SetLeft(
                    label,
                    x + 5);

                Canvas.SetTop(
                    label,
                    2);

                GanttCanvas.Children.Add(label);
            }
        }

        public void RefreshDashboard()
        {
            var vm =
                DataContext as HomeViewModel;

            vm?.RefreshProductionSystem();

            LoadGantt();
        }

        private void LoadGantt()
        {
            if (_isLoadingGantt)
                return;

            _isLoadingGantt = true;

            try
            {
                var statusService =
                    new ProductionStatusService();

                statusService.RefreshStatuses();

                var service =
                    new ProductionScheduleService();

                var schedule =
                    service.GetSchedule();

                GanttCanvas.Children.Clear();
                GanttLabels.Children.Clear();

                if (!schedule.Any())
                    return;

                double pixelsPerDay = 80;
                double headerHeight = 25;

                DateTime firstDate =
                    schedule.Min(x => x.StartDate);

                DateTime lastDate =
                    schedule.Max(x => x.EndDate);

                int totalDays =
                    (lastDate - firstDate).Days + 2;

                var repo =
                    new WorkOrderRepository();

                var workOrders =
                    repo.GetAll();

                DrawTimelineGrid(
                    firstDate,
                    totalDays);

                // TODAY INDICATOR
                double todayX =
                    (DateTime.Today - firstDate)
                    .TotalDays *
                    pixelsPerDay;

                var todayLine =
                    new Line
                    {
                        X1 = todayX,
                        X2 = todayX,
                        Y1 = 0,
                        Y2 = schedule.Count * 30 + 50,
                        Stroke = Brushes.Red,
                        StrokeThickness = 2
                    };

                GanttCanvas.Children.Add(todayLine);

                int row = 0;

                foreach (var item in schedule)
                {
                    var workOrder =
                        workOrders.FirstOrDefault(
                            w =>
                                w.WorkOrderNumber ==
                                item.WorkOrderNumber);

                    var label =
                        new TextBlock
                        {
                            Text =
                                item.WorkOrderNumber,
                            Margin =
                                new Thickness(
                                    0,
                                    10,
                                    0,
                                    10),
                            FontWeight =
                                FontWeights.Bold
                        };

                    GanttLabels.Children.Add(label);

                    double startOffset =
                        (item.StartDate -
                         firstDate)
                        .TotalDays *
                        pixelsPerDay;

                    double duration =
                        Math.Max(
                            1,
                            (item.EndDate -
                             item.StartDate)
                            .TotalDays);

                    double width =
                        duration *
                        pixelsPerDay;

                    Brush color =
                        Brushes.SteelBlue;

                    if (workOrder != null)
                    {
                        switch (workOrder.Status)
                        {
                            case WorkOrderStatus.Ready:
                                color =
                                    Brushes.Green;
                                break;

                            case WorkOrderStatus.InProduction:
                                color =
                                    Brushes.DodgerBlue;
                                break;

                            case WorkOrderStatus.Completed:
                                color =
                                    Brushes.Gray;
                                break;
                        }

                        if (workOrder?.DueDate < DateTime.Today &&
                                workOrder.Status != WorkOrderStatus.Completed)
                        {
                            color = Brushes.OrangeRed;
                        }
                    }

                    var bar =
                        new Rectangle
                        {
                            Width = width,
                            Height = 20,
                            Fill = color,
                            RadiusX = 3,
                            RadiusY = 3
                        };

                    Canvas.SetLeft(
                        bar,
                        startOffset);

                    Canvas.SetTop(
                        bar,
                        headerHeight +
                        (row * 30));

                    GanttCanvas.Children.Add(bar);

                    var text =
                    new TextBlock
                    {
                        Text = item.WorkOrderNumber,
                        FontSize = 10,
                        Foreground = Brushes.White
                     };

                    Canvas.SetLeft(
                        text,
                        startOffset + 5);

                    Canvas.SetTop(
                        text,
                        headerHeight +
                        (row * 30) + 2);

                    GanttCanvas.Children.Add(text);

                    ToolTipService.SetToolTip(
                        bar,
                        $"{item.WorkOrderNumber}\n" +
                        $"Status: {workOrder?.Status}\n" +
                        $"Start: {item.StartDate:d}\n" +
                        $"End: {item.EndDate:d}\n" +
                        $"Hours: {workOrder?.EstimatedHours}");

                    row++;
                }
            }
            finally
            {
                _isLoadingGantt = false;
            }
        }
    }
}