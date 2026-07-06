using System;
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Analytics.Production;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Data.Services.ProductionEngine;
using WeldAdminPro.Data.Services;

namespace WeldAdminPro.Data.Services.Production
{
    public class ProductionApplicationService
    {
        private readonly WorkOrderRepository _repository;
        private readonly WeldAdminPro.Data.Services.ProductionAdvisorService _advisor;
        private readonly WeldAdminPro.Data.Services.ProductionReadinessService _readinessService;
        private readonly WeldAdminPro.Data.Services.ProductionAIPlannerService _planner;
        private readonly ProductionScheduleService _scheduleService;
        private readonly WeldAdminPro.Data.Services.ProductionCapacityService _capacityService;
        private readonly ProductionDelayPredictionService _delayPredictionService;
        private readonly WeldAdminPro.Data.Services.ProductionEngine.TimelineEngine _timelineEngine;


        public ProductionApplicationService()
        {
            _repository = new WorkOrderRepository();
            _advisor = new WeldAdminPro.Data.Services.ProductionAdvisorService();
            _readinessService =
                new WeldAdminPro.Data.Services.ProductionReadinessService(
                    new WorkOrderRepository(),
                    new WeldAdminPro.Data.Services.WorkOrderShortageDetectionService());
            _planner =
                new WeldAdminPro.Data.Services.ProductionAIPlannerService();
            _scheduleService =
                new WeldAdminPro.Data.Services.ProductionScheduleService();
            _capacityService =
                new WeldAdminPro.Data.Services.ProductionCapacityService(
                    new WorkOrderRepository());
            _delayPredictionService =
                new ProductionDelayPredictionService();
            _timelineEngine =
                new WeldAdminPro.Data.Services.ProductionEngine.TimelineEngine();
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
    }
}