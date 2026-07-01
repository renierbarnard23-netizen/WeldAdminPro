using System;
using System.CodeDom.Compiler;
using System.Collections;
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
using WeldAdminPro.Core.Execution;
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
            Debug.WriteLine("HOMEVIEW CONSTRUCTOR");

            InitializeComponent();

            Debug.WriteLine("AFTER INITIALIZE");

            Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    Debug.WriteLine("BEGIN INVOKE");

                    Debug.WriteLine(
                        $"DataContext = {DataContext?.GetType().Name ?? "NULL"}");

                    LoadGantt();

                    Debug.WriteLine("LOAD GANTT FINISHED");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"GANTT ERROR: {ex}");
                }
            });

            Loaded += HomeView_Loaded;
        }

        private void HomeView_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            Debug.WriteLine("HOMEVIEW LOADED");

            if (DataContext is HomeViewModel vm)
            {
                Debug.WriteLine("VM FOUND");

                vm.ProductionChanged -= Vm_ProductionChanged;
                vm.ProductionChanged += Vm_ProductionChanged;
            }
            else
            {
                Debug.WriteLine("VM NULL");
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

            var list =
                vm.Production.ProductionQueue.ToList();

            list.Remove(droppedItem);
            list.Insert(0, droppedItem);

            vm.Production.ProductionQueue =
                new ObservableCollection<ProductionQueueItem>(list);
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
                            Height = GanttCanvas.Height,
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
            Debug.WriteLine("LOAD GANTT");

            if (DataContext == null)
            {
                Debug.WriteLine("DATACONTEXT NULL");
                return;
            }

            var vm = DataContext as HomeViewModel;

            Debug.WriteLine("LOAD GANTT CALLED");

            if (vm == null)
            {
                Debug.WriteLine("VM NULL");
                return;
            }

            if (vm.Production == null)
            {
                Debug.WriteLine("PRODUCTION NULL");
                return;
            }

            Debug.WriteLine($"QUEUE COUNT = {vm.Production.ProductionQueue.Count}");
            Debug.WriteLine("LOAD GANTT");
            Debug.WriteLine($"DataContext = {DataContext?.GetType().Name}");
            Debug.WriteLine("LOAD GANTT CALLED");
            Debug.WriteLine($"DATACONTEXT NULL = {DataContext == null}");
            Debug.WriteLine($"TYPE = {DataContext?.GetType().Name}");

            if (_isLoadingGantt)
                return;

            _isLoadingGantt = true;

                try
            {
                var statusService =
                    new ProductionStatusService();

                statusService.RefreshStatuses();

                if (vm == null)
                    return;

                var queue =
                    vm.Production.ProductionQueue
                    .OrderBy(x => x.Priority)
                    .ToList();

                Debug.WriteLine($"QUEUE COUNT = {queue.Count}");

                foreach (var q in queue)
                {
                    Debug.WriteLine($"QUEUE ITEM {q.WorkOrderNumber} Priority={q.Priority}");
                }

                Debug.WriteLine(
                    $"GANTT QUEUE COUNT = {queue.Count}");

                foreach (var q in queue)
                {
                    Debug.WriteLine(
                        $"BAR -> {q.WorkOrderNumber}");
                }

                if (!queue.Any())
                    return;

                Debug.WriteLine($"GANTT ITEMS: {queue.Count}");

                foreach (var q in queue)
                {
                    Debug.WriteLine(
                        $"GANTT ORDER: " +
                        $"{q.Priority} - " +
                        $"{q.WorkOrderNumber}");
                }

                GanttCanvas.Children.Clear();
                GanttLabels.Children.Clear();

                if (!queue.Any())
                    return;

                Debug.WriteLine(
                    $"QUEUE COUNT = {queue.Count}");

                double pixelsPerDay = 80;
                double headerHeight = 25;

                DateTime firstDate =
                    DateTime.Today;

                DateTime lastDate =
                    DateTime.Today.AddDays(queue.Count + 2);

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
                        Y2 = queue.Count * 30 + 50,
                        Stroke = Brushes.Red,
                        StrokeThickness = 2
                    };

                GanttCanvas.Children.Add(todayLine);

                int row = 0;

                Debug.WriteLine( 
                    $"BEFORE LOOP: {queue.Count}");

                foreach (var item in queue)
                {

                    Debug.WriteLine(
                        $"DRAWING {item.WorkOrderNumber}");

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
                        (item.Priority - 1)
                        * pixelsPerDay;

                    double width = pixelsPerDay;

                    Brush color =
                        Brushes.SteelBlue;

                    if (workOrder != null)
                    {
                        switch (workOrder.Status)
                        {
                            case WorkOrderStatus.Ready:
                                color = Brushes.ForestGreen;
                                break;

                            case WorkOrderStatus.InProduction:
                                color = Brushes.DodgerBlue;
                                break;

                            case WorkOrderStatus.Paused:
                                color = Brushes.DarkOrange;
                                break;

                            case WorkOrderStatus.Completed:
                                color = Brushes.Gray;
                                break;
                        }

                        if (workOrder.BlockReason != BlockReason.None)
                        {
                            color = Brushes.Firebrick;
                        }

                        if (workOrder.DueDate < DateTime.Today &&
                            workOrder.Status != WorkOrderStatus.Completed)
                        {
                            color = Brushes.OrangeRed;
                        }

                        if (workOrder?.DueDate < DateTime.Today &&
                                workOrder.Status != WorkOrderStatus.Completed)
                        {
                            color = Brushes.OrangeRed;
                        }
                    }

                    var bar =
                        new Border
                        {
                            Width = width,
                            Height = 24,
                            Background = color,
                            CornerRadius = new CornerRadius(6)
                        };

                    bar.Cursor = Cursors.Hand;

                    bar.MouseLeftButtonDown +=
                        (s, e) =>
                        {
                            if (DataContext is HomeViewModel vm)
                            {
                                var selected =
                                    vm.Production.ProductionQueue
                                        .FirstOrDefault(
                                            x => x.WorkOrderNumber ==
                                                item.WorkOrderNumber);

                                if (selected != null)
                                {
                                    vm.SelectedWorkOrder =
                    selected;
                                }
                            }
                        };

                    Canvas.SetLeft(
                        bar,
                        startOffset);

                    Canvas.SetTop(
                        bar,
                        headerHeight +
                        (row * 30));

                    Debug.WriteLine(
                        $"ADDING BAR {item.WorkOrderNumber} " +
                        $"X={startOffset} " +
                        $"Y={headerHeight + (row * 30)}");

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
                        $"Priority: {item.Priority}\n" +
                        $"Status: {workOrder?.Status}\n" +
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