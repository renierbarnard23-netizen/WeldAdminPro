using System;
using System.Linq;
using WeldAdminPro.Core.Analytics.Production;
using WeldAdminPro.Core.Enums;
using WeldAdminPro.Core.Execution;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
	public class ProductionControlTowerService
	{
		private readonly WorkOrderRepository _workOrderRepository;
		private readonly ProductionBlockService _blockService;

		public ProductionControlTowerService()
		{
			_workOrderRepository = new WorkOrderRepository();
			_blockService = new ProductionBlockService();
		}

        public ProductionControlTowerModel GetControlTower()
        {
            var workOrders =
                _workOrderRepository
                    .GetAll()
                    .ToList();

            var model =
                new ProductionControlTowerModel();

            var blockedOrders =
                _blockService
                    .GetBlockedWorkOrders()
                    .Count;

            model.BlockedOrders =
                blockedOrders;

            if (model.BlockedOrders > 0)
            {
                model.Alerts.Add(
                    new ProductionAlert
                    {
                        Severity = "Critical",
                        Message =
                            $"{model.BlockedOrders} work orders are blocked."
                    });
            }

            model.RunningOrders =
                workOrders.Count(x =>
                    x.Status ==
                    WorkOrderStatus.InProduction);

            model.CompletedToday =
                workOrders.Count(x =>
                    x.Status ==
                    WorkOrderStatus.Completed
                    &&
                    x.CompletedOn.HasValue
                    &&
                    x.CompletedOn.Value.Date ==
                    DateTime.Today);

            model.ReadyOrders =
                Math.Max(
                    0,
                    workOrders.Count(x =>
                        x.Status ==
                        WorkOrderStatus.Ready)
                    -
                    blockedOrders);

            model.TotalWorkOrders =
                workOrders.Count;

            model.ReadyToStartToday =
                workOrders.Count(w =>
                    w.Status == WorkOrderStatus.Ready &&
                    w.BlockReason == BlockReason.None);

            model.HighPriorityOrders =
                workOrders.Count(
                    w => w.Priority >= 8);

            model.OverdueWorkOrders =
                workOrders.Count(
                    w =>
                        w.DueDate.HasValue &&
                        w.DueDate.Value.Date <
                        DateTime.Today &&
                        w.Status !=
                        WorkOrderStatus.Completed);

            var startOfWeek =
                DateTime.Today.AddDays(
                    -(int)DateTime.Today.DayOfWeek);

            model.CompletedThisWeek =
                workOrders.Count(w =>
                    w.Status ==
                        WorkOrderStatus.Completed &&
                    w.CompletedOn.HasValue &&
                    w.CompletedOn.Value >= startOfWeek);

            model.ProductionEfficiency =
                model.TotalWorkOrders == 0
                    ? 100
                    : Math.Round(
                        (double)
                        (model.TotalWorkOrders -
                        model.BlockedOrders)
                        /
                        model.TotalWorkOrders
                        * 100,
                    1);           

            if (model.MaterialShortages > 0)
            {
                model.Alerts.Add(
                    new ProductionAlert
                    {
                        Severity = "Warning",
                        Message =
                            $"{model.MaterialShortages} material shortages detected."
                    });
            }
            
            //--------------------------------------------------
            // Capacity
            //--------------------------------------------------

            const double totalCapacityPerDay = 32;

            var scheduledHours =
                workOrders
                    .Where(x =>
                        x.Status ==
                        WorkOrderStatus.InProduction)
                    .Sum(x =>
                        x.EstimatedHours);

            model.CapacityLoad =
                totalCapacityPerDay <= 0
                    ? 0
                    : Math.Round(
                        scheduledHours
                        /
                        totalCapacityPerDay
                        *
                        100,
                        2);

            //--------------------------------------------------
            // Deadline Risks
            //--------------------------------------------------

            model.DeadlineRisks =
                workOrders.Count(x =>
                    x.DueDate.HasValue
                    &&
                    x.DueDate.Value.Date <
                        DateTime.Today
                    &&
                    x.Status !=
                        WorkOrderStatus.Completed);

            if (model.DeadlineRisks > 0)
            {
                model.Alerts.Add(
                    new ProductionAlert
                    {
                        Severity = "Warning",
                        Message =
                            $"{model.DeadlineRisks} work orders are at risk of missing deadlines."
                    });
            }

            model.DelayedWorkOrders =
                model.DeadlineRisks;

            //--------------------------------------------------
            // Material Shortages
            //--------------------------------------------------

            model.MaterialShortages =
                blockedOrders;

            //--------------------------------------------------
            // Repairs
            //--------------------------------------------------

            model.ActiveRepairs = 0;

            //--------------------------------------------------
            // Reservations
            //--------------------------------------------------

            model.OverdueReservations = 0;

            //--------------------------------------------------
            // Bottlenecks
            //--------------------------------------------------

            model.BottleneckCount =
                workOrders.Count(x =>
                    x.Status ==
                        WorkOrderStatus.InProduction
                    &&
                    x.EstimatedHours >
                        16);

            //--------------------------------------------------
            // System Status
            //--------------------------------------------------

            if (model.BlockedOrders > 0)
            {
                model.Alerts.Add(
                    new ProductionAlert
                    {
                        Severity = "Critical",
                        Message =
                            $"{model.BlockedOrders} work orders are blocked."
                    });
            }

            if (model.DeadlineRisks > 0)
            {
                model.Alerts.Add(
                    new ProductionAlert
                    {
                        Severity = "Warning",
                        Message =
                            $"{model.DeadlineRisks} work orders are at risk."
                    });
            }

            if (model.MaterialShortages > 0)
            {
                model.Alerts.Add(
                    new ProductionAlert
                    {
                        Severity = "Warning",
                        Message =
                            $"{model.MaterialShortages} material shortages detected."
                    });
            }

            int score = 100;

            score -= model.BlockedOrders * 10;
            score -= model.MaterialShortages * 5;
            score -= model.DeadlineRisks * 5;
            score -= model.BottleneckCount * 5;

            model.HealthScore =
                Math.Max(score, 0);

            return model;
        }
    }
}