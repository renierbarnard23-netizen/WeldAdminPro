using System;
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Analytics.Production;
using WeldAdminPro.Core.Execution;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Data.Services;
using WeldAdminPro.Data.Services.ProductionEngine;

using WeldAdminPro.Core.Security.Abstractions;
using WeldAdminPro.Core.Security;
namespace WeldAdminPro.Data.Services.Production
{
    public class ProductionApplicationService
    {
    private readonly ICurrentUserContext _currentUser;
        private readonly IPermissionAuthorizationService _permissionAuthorization;
        private readonly IProjectAccessAuthorizationService _projectAccessAuthorization;
        private readonly WorkOrderRepository _repository;
        private readonly ProductionScheduleService _scheduleService;
        private readonly ProductionDelayPredictionService _delayPredictionService;
        private readonly TimelineEngine _timelineEngine;
        private readonly ProductionAdvisorService _advisor;
        private readonly ProductionReadinessService _readinessService;
        private readonly ProductionAIPlannerService _planner;
        private readonly ProductionCapacityService _capacityService;
        private readonly ProjectRepository _projectRepository;
        private readonly StockRepository _stockRepository;
        private readonly WorkOrderMaterialRepository _workOrderMaterialRepository;
        private readonly PersistentReservationService _reservationService;
        private readonly StockProjectTransactionService _stockService;

        public ProductionApplicationService
            (
                WorkOrderRepository repository,
                ProjectRepository projectRepository,
                StockRepository stockRepository,
                WorkOrderMaterialRepository workOrderMaterialRepository,
                PersistentReservationService reservationService,
                ProductionAdvisorService advisor,
                ProductionReadinessService readinessService,
                ProductionAIPlannerService planner,
                ProductionScheduleService scheduleService,
                ProductionCapacityService capacityService,
                ProductionDelayPredictionService delayPredictionService,
                TimelineEngine timelineEngine,
                StockProjectTransactionService stockService,
                
                ICurrentUserContext currentUser,
                IPermissionAuthorizationService permissionAuthorization,
                IProjectAccessAuthorizationService projectAccessAuthorization
             )
        {
            _currentUser = currentUser;
            _permissionAuthorization = permissionAuthorization
                ?? throw new ArgumentNullException(nameof(permissionAuthorization));
            _projectAccessAuthorization = projectAccessAuthorization
                ?? throw new ArgumentNullException(nameof(projectAccessAuthorization));
            _repository = repository;
            _advisor = advisor;
            _readinessService = readinessService;
            _planner = planner;
            _scheduleService = scheduleService;
            _capacityService = capacityService;
            _delayPredictionService = delayPredictionService;
            _timelineEngine = timelineEngine;
            _projectRepository = projectRepository;
            _stockRepository = stockRepository;
            _workOrderMaterialRepository = workOrderMaterialRepository;
            _reservationService = reservationService;
            _stockService = stockService;
        }

        // Temporary compatibility constructor

        // =====================================================
        // Queries
        // =====================================================

        public List<WorkOrder> GetAllWorkOrders()
        {
            var workOrders = _repository
                .GetAll()
                .OrderBy(w => w.Priority)
                .ToList();

            if (string.Equals(_currentUser.Role, "Administrator", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(_currentUser.Role, "Admin", StringComparison.OrdinalIgnoreCase))
                return workOrders;

            return workOrders
                .Where(IsWorkOrderAccessible)
                .ToList();
        }

        public WorkOrder? GetWorkOrder(Guid id)
        {
            var workOrder = _repository.GetById(id);

            if (workOrder == null)
                return null;

            if (!IsWorkOrderAccessible(workOrder))
                return null;

            return workOrder;
        }

        public WorkOrderReadiness? GetWorkOrderReadiness(Guid id)
        {
            EnsureWorkOrderAccess(id);
            return _readinessService
                .GetWorkOrderReadiness()
                .FirstOrDefault(r => r.WorkOrderId == id);
        }

        public AIProductionRecommendation? GetWorkOrderRecommendation(Guid id)
        {
            EnsureWorkOrderAccess(id);
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
        public List<StockItem> GetStockItems()
        {
            return _stockRepository
                .GetAll()
                .ToList();
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

            if (!string.Equals(_currentUser.Role, "Administrator", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(_currentUser.Role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                workOrders = workOrders
                    .Where(IsWorkOrderAccessible)
                    .ToList();
            }

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
            var projects = _projectRepository
                .GetAll()
                .ToList();

            if (string.Equals(_currentUser.Role, "Administrator", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(_currentUser.Role, "Admin", StringComparison.OrdinalIgnoreCase))
                return projects;

            if (!_currentUser.IsAuthenticated ||
                !_currentUser.CompanyId.HasValue ||
                _currentUser.CompanyId.Value == Guid.Empty)
                return new List<Project>();

            return projects
                .Where(p =>
                    p.CompanyId.HasValue &&
                    p.CompanyId.Value == _currentUser.CompanyId.Value)
                .ToList();
        }

        // =====================================================
        // Commands
        // =====================================================

        public async Task<bool> StartWorkOrderAsync(Guid id)
        {
            await EnsureWorkOrderAccessAsync(
                id,
                PermissionKeys.Production.Start);

            var workOrder = _repository.GetById(id);

            if (workOrder == null)
                return false;

            workOrder.Status = WorkOrderStatus.InProduction;
            workOrder.IsPaused = false;
            workOrder.ActualStartTime ??= DateTime.Now;

            _repository.Update(workOrder);

            return true;
        }

        public async Task<bool> PauseWorkOrderAsync(Guid id)
        {
            await EnsureWorkOrderAccessAsync(
                id,
                PermissionKeys.Production.EditWorkOrder);

            var workOrder = _repository.GetById(id);

            if (workOrder == null)
                return false;

            workOrder.Status = WorkOrderStatus.Paused;
            workOrder.IsPaused = true;

            _repository.Update(workOrder);

            return true;
        }

        public async Task<bool> CompleteWorkOrderAsync(Guid id)
        {
            await EnsureWorkOrderAccessAsync(
                id,
                PermissionKeys.Production.Complete);

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

        public async Task<bool> CancelWorkOrderAsync(Guid id)
        {
            await EnsureWorkOrderAccessAsync(
                id,
                PermissionKeys.Production.EditWorkOrder);

            var workOrder = _repository.GetById(id);

            if (workOrder == null)
                return false;

            workOrder.Status = WorkOrderStatus.Cancelled;
            workOrder.IsPaused = false;

            _repository.Update(workOrder);

            return true;
        }

        public async Task<bool> CreateWorkOrderAsync(WorkOrder workOrder)
        {
            if (workOrder == null)
                throw new ArgumentNullException(nameof(workOrder));

            await EnsureProjectAccessAsync(
                workOrder.ProjectId,
                PermissionKeys.Production.CreateWorkOrder);

            _repository.Add(workOrder);

            return true;
        }

        public async Task<bool> UpdateWorkOrderAsync(WorkOrder workOrder)
        {
            if (workOrder == null)
                throw new ArgumentNullException(nameof(workOrder));

            await EnsureWorkOrderAccessAsync(
                workOrder.Id,
                PermissionKeys.Production.EditWorkOrder);

            _repository.Update(workOrder);

            return true;
        }
        public async Task<bool> AddWorkOrderMaterialAsync(
            WorkOrderMaterial material)
        {
            if (material == null)
                throw new ArgumentNullException(nameof(material));

            await EnsureWorkOrderAccessAsync(
                material.WorkOrderId,
                PermissionKeys.Production.IssueMaterial);

            _workOrderMaterialRepository.Add(material);

            _reservationService.Reserve(material.WorkOrderId);

            return true;
        }

        public async Task<bool> IssueMaterialAsync(
            Project project,
            StockItem stockItem,
            int quantity,
            string issuedBy)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));

            await EnsureProjectAccessAsync(
                project.Id,
                PermissionKeys.Production.IssueMaterial);

            _stockService.IssueStock(
                project,
                stockItem,
                quantity,
                issuedBy);

            return true;
        }



        private async Task EnsureProjectAccessAsync(
            Guid projectId,
            string permissionKey)
        {
            if (!_currentUser.IsAuthenticated ||
                string.IsNullOrWhiteSpace(_currentUser.UserId))
            {
                throw new UnauthorizedAccessException(
                    "Authentication is required.");
            }

            var hasPermission =
                await _permissionAuthorization.HasPermissionAsync(
                    _currentUser.UserId,
                    _currentUser.Role,
                    permissionKey);

            if (!hasPermission)
            {
                throw new UnauthorizedAccessException(
                    $"You do not have permission to perform '{permissionKey}'.");
            }

            var project = _projectRepository.GetById(projectId);

            if (project == null)
            {
                throw new UnauthorizedAccessException(
                    "The requested project does not exist or is not accessible.");
            }

            var hasProjectAccess =
                await _projectAccessAuthorization.CanAccessProjectAsync(
                    _currentUser.UserId,
                    project);

            if (!hasProjectAccess)
            {
                throw new UnauthorizedAccessException(
                    "The requested project is not accessible.");
            }
        }

        private async Task EnsureWorkOrderAccessAsync(
            Guid id,
            string permissionKey)
        {
            if (!_currentUser.IsAuthenticated ||
                string.IsNullOrWhiteSpace(_currentUser.UserId))
            {
                throw new UnauthorizedAccessException(
                    "Authentication is required.");
            }

            var hasPermission =
                await _permissionAuthorization.HasPermissionAsync(
                    _currentUser.UserId,
                    _currentUser.Role,
                    permissionKey);

            if (!hasPermission)
            {
                throw new UnauthorizedAccessException(
                    $"You do not have permission to perform '{permissionKey}'.");
            }

            var workOrder = _repository.GetById(id);

            if (workOrder == null)
            {
                throw new UnauthorizedAccessException(
                    "The requested work order does not exist or is not accessible.");
            }

            var project = _projectRepository.GetById(workOrder.ProjectId);

            if (project == null)
            {
                throw new UnauthorizedAccessException(
                    "The requested project does not exist or is not accessible.");
            }

            var hasProjectAccess =
                await _projectAccessAuthorization.CanAccessProjectAsync(
                    _currentUser.UserId,
                    project);

            if (!hasProjectAccess)
            {
                throw new UnauthorizedAccessException(
                    "The requested work order is not accessible.");
            }
        }

        private bool IsWorkOrderAccessible(WorkOrder workOrder)
        {
            if (!_currentUser.IsAuthenticated)
                return false;

            if (string.Equals(_currentUser.Role, "Administrator", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(_currentUser.Role, "Admin", StringComparison.OrdinalIgnoreCase))
                return true;

            if (!_currentUser.CompanyId.HasValue ||
                _currentUser.CompanyId.Value == Guid.Empty)
                return false;

            var project = _projectRepository.GetById(workOrder.ProjectId);

            return project != null &&
                   project.CompanyId.HasValue &&
                   project.CompanyId.Value == _currentUser.CompanyId.Value;
        }

        private void EnsureProjectAccess(Guid projectId)
        {
            if (!_currentUser.IsAuthenticated)
                throw new UnauthorizedAccessException(
                    "Authentication is required.");

            if (string.Equals(_currentUser.Role, "Administrator", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(_currentUser.Role, "Admin", StringComparison.OrdinalIgnoreCase))
                return;

            if (!_currentUser.CompanyId.HasValue ||
                _currentUser.CompanyId.Value == Guid.Empty)
                throw new UnauthorizedAccessException(
                    "A valid company context is required.");

            var project = _projectRepository.GetById(projectId);

            if (project == null ||
                !project.CompanyId.HasValue ||
                project.CompanyId.Value != _currentUser.CompanyId.Value)
                throw new UnauthorizedAccessException(
                    "You do not have access to this project.");
        }

        private void EnsureWorkOrderAccess(Guid id)
        {
            if (!_currentUser.IsAuthenticated)
                throw new UnauthorizedAccessException(
                    "Authentication is required.");

            var workOrder = _repository.GetById(id);

            if (workOrder == null)
                throw new UnauthorizedAccessException(
                    "The requested work order does not exist or is not accessible.");

            if (string.Equals(_currentUser.Role, "Administrator", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(_currentUser.Role, "Admin", StringComparison.OrdinalIgnoreCase))
                return;

            if (!_currentUser.CompanyId.HasValue ||
                _currentUser.CompanyId.Value == Guid.Empty)
                throw new UnauthorizedAccessException(
                    "A valid company context is required.");

            var project = _projectRepository.GetById(workOrder.ProjectId);

            if (project == null)
                throw new UnauthorizedAccessException(
                    "The requested work order project does not exist or is not accessible.");

            if (!project.CompanyId.HasValue ||
                project.CompanyId.Value != _currentUser.CompanyId.Value)
                throw new UnauthorizedAccessException(
                    "You do not have access to this work order.");
        }
    }
    }

