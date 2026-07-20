using System;
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Analytics.Production;
using WeldAdminPro.Core.Execution;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Data.Services;
using WeldAdminPro.Data.Services.ProductionEngine;

namespace WeldAdminPro.Data.Services.Production
{
    public class ProductionApplicationService
    {
        private readonly WorkOrderRepository _repository;
        private readonly ProductionScheduleService _scheduleService;
        private readonly ProductionDelayPredictionService _delayPredictionService;
        private readonly TimelineEngine _timelineEngine;
        private readonly ProductionAdvisorService _advisor;
        private readonly ProductionReadinessService _readinessService;
        private readonly ProductionAIPlannerService _planner;
        private readonly ProductionCapacityService _capacityService;
        private readonly ProjectRepository _projectRepository;

        public ProductionApplicationService
            (
                WorkOrderRepository repository,
                ProjectRepository projectRepository,
                ProductionAdvisorService advisor,
                ProductionReadinessService readinessService,
                ProductionAIPlannerService planner,
                ProductionScheduleService scheduleService,
                ProductionCapacityService capacityService,
                ProductionDelayPredictionService delayPredictionService,
                TimelineEngine timelineEngine)
        {
            _repository = repository;
            _advisor = advisor;
            _readinessService = readinessService;
            _planner = planner;
            _scheduleService = scheduleService;
            _capacityService = capacityService;
            _delayPredictionService = delayPredictionService;
            _timelineEngine = timelineEngine;
            _projectRepository = projectRepository;
        }

        // Temporary compatibility constructor
        public ProductionApplicationService()
            : this(
                new WorkOrderRepository(),
                new ProjectRepository(),
                new ProductionAdvisorService(),
                new ProductionReadinessService(
                    new WorkOrderRepository(),
                    new WorkOrderShortageDetectionService()),
                new ProductionAIPlannerService(),
                new ProductionScheduleService(),
                new ProductionCapacityService(new WorkOrderRepository()),
                new ProductionDelayPredictionService(),
                new TimelineEngine())
        {
        }

        // =====================================================
        // Queries
        // =====================================================

        public List<WorkOrder> GetAllWorkOrders()
        {
            return _repository
                .GetAll()
                .OrderBy(w => w.Priority)
                .ToList();
        }

        public WorkOrder? GetWorkOrder(Guid id)
        {
            return _repository.GetById(id);
        }

        public WorkOrderReadiness? GetWorkOrderReadiness(Guid id)
        {
            return _readinessService
                .GetWorkOrderReadiness()
                .FirstOrDefault(r => r.WorkOrderId == id);
        }

        public AIProductionRecommendation? GetWorkOrderRecommendation(Guid id)
        {
            var workOrder = _repository.GetById(id);

            if (workOrder == null)
                return null;

            return _planner
                .GetRecommendations()
                .FirstOrDefault(r =>
                    r.WorkOrderNumber == workOrder.WorkOrderNumber);
        }

        public List<ProductionScheduleItem> GetSchedule()
        {
            return _scheduleService.GetSchedule();
        }

        public List<ProductionCapacityForecast> GetCapacityForecast()
        {
            return _capacityService.GetCapacityForecast();
        }

        public List<ProductionDelayPrediction> GetDelayPredictions()
        {
            return _delayPredictionService.PredictDelays();
        }

        public List<ProductionGanttItem> GetTimeline()
        {
            var snapshot =
                new ProductionSnapshot();

            _timelineEngine.Evaluate(snapshot);

            return snapshot.Timeline;
        }

        public List<WorkOrder> GetActiveWorkOrders()
        {
            var workOrders = _repository
                .GetAll()
                .Where(x =>
                    x.Status != WorkOrderStatus.Completed &&
                    x.Status != WorkOrderStatus.Cancelled)
                .ToList();

            var readiness = _readinessService.GetWorkOrderReadiness();

            foreach (var wo in workOrders)
            {
                var state = readiness.FirstOrDefault(r => r.WorkOrderId == wo.Id);

                if (state == null)
                    continue;

                if (state.IsReady)
                {
                    wo.BlockReason = BlockReason.None;
                    wo.BlockMessage = "Ready";
                }
                else
                {
                    wo.BlockReason = BlockReason.MaterialShortage;
                    wo.BlockMessage = state.Reason;
                }
            }

            return workOrders;
        }

        public List<Project> GetProjects()
        {
            return _projectRepository
                .GetAll()
                .ToList();
        }

        // =====================================================
        // Commands
        // =====================================================

        public bool StartWorkOrder(Guid id)
        {
            var workOrder = _repository.GetById(id);

            if (workOrder == null)
                return false;

            workOrder.Status = WorkOrderStatus.InProduction;

            workOrder.IsPaused = false;

            workOrder.ActualStartTime ??= DateTime.Now;

            _repository.Update(workOrder);

            return true;
        }

        public bool PauseWorkOrder(Guid id)
        {
            var workOrder = _repository.GetById(id);

            if (workOrder == null)
                return false;

            workOrder.Status = WorkOrderStatus.Paused;

            workOrder.IsPaused = true;

            _repository.Update(workOrder);

            return true;
        }

        public bool CompleteWorkOrder(Guid id)
        {
            var workOrder = _repository.GetById(id);

            if (workOrder == null)
                return false;

            workOrder.Status = WorkOrderStatus.Completed;

            workOrder.IsPaused = false;

            workOrder.CompletedOn = DateTime.Now;

            workOrder.ActualEndTime = DateTime.Now;

            if (workOrder.ActualStartTime.HasValue)
            {
                workOrder.ActualHours =
                    (DateTime.Now - workOrder.ActualStartTime.Value)
                    .TotalHours;
            }

            _repository.Update(workOrder);

            return true;
        }

            public bool CancelWorkOrder(Guid id)
            {
                var workOrder = _repository.GetById(id);

                if (workOrder == null)
                    return false;

                workOrder.Status = WorkOrderStatus.Cancelled;
                workOrder.IsPaused = false;

                _repository.Update(workOrder);

                return true;
            }

            public bool CreateWorkOrder(WorkOrder workOrder)
            {
                _repository.Add(workOrder);
                return true;
            }

            public bool UpdateWorkOrder(WorkOrder workOrder)
            {
                _repository.Update(workOrder);
                return true;
            }


}
    }
