using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Interfaces;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Core.Security;
using WeldAdminPro.Core.Security.Abstractions;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services.Quality
{
    public class InspectionTestPlanInspectionExecutionApplicationService
    {
        private readonly InspectionTestPlanInspectionExecutionRepository _repository;
        private readonly InspectionTestPlanRepository _itpRepository;
        private readonly InspectionTestPlanItemRepository _itemRepository;
        private readonly IWeldRepository _weldRepository;
        private readonly ProjectRepository _projectRepository;
        private readonly ICurrentUserContext _currentUser;
        private readonly AuditService _auditService;
        private readonly IPermissionAuthorizationService _permissionAuthorization;
        private readonly IProjectAccessAuthorizationService _projectAccessAuthorization;

        public InspectionTestPlanInspectionExecutionApplicationService(
            InspectionTestPlanInspectionExecutionRepository repository,
            InspectionTestPlanRepository itpRepository,
            InspectionTestPlanItemRepository itemRepository,
            IWeldRepository weldRepository,
            ProjectRepository projectRepository,
            ICurrentUserContext currentUser,
            AuditService auditService,
            IPermissionAuthorizationService permissionAuthorization,
            IProjectAccessAuthorizationService projectAccessAuthorization)
        {
            _repository = repository
                ?? throw new ArgumentNullException(nameof(repository));

            _itpRepository = itpRepository
                ?? throw new ArgumentNullException(nameof(itpRepository));

            _itemRepository = itemRepository
                ?? throw new ArgumentNullException(nameof(itemRepository));

            _weldRepository = weldRepository
                ?? throw new ArgumentNullException(nameof(weldRepository));

            _projectRepository = projectRepository
                ?? throw new ArgumentNullException(nameof(projectRepository));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));

            _auditService = auditService
                ?? throw new ArgumentNullException(nameof(auditService));

            _permissionAuthorization = permissionAuthorization
                ?? throw new ArgumentNullException(nameof(permissionAuthorization));

            _projectAccessAuthorization = projectAccessAuthorization
                ?? throw new ArgumentNullException(nameof(projectAccessAuthorization));
        }

        public async Task<List<InspectionTestPlanInspectionExecution>> GetByItemAsync(
            Guid itemId)
        {
            await EnsurePermissionAsync();

            var item = await EnsureItemAccessAsync(itemId);
            await EnsurePlanAccessAsync(item.InspectionTestPlanId);

            return _repository.GetByItem(itemId);
        }

        public async Task<List<InspectionTestPlanInspectionExecution>> GetByWeldAsync(
            Guid weldId)
        {
            await EnsurePermissionAsync();

            var weld = await EnsureWeldExistsAsync(weldId);
            await EnsureProjectAccessAsync(weld.ProjectId);

            return _repository.GetByWeld(weldId);
        }

        public async Task<InspectionTestPlanInspectionExecution?> GetByIdAsync(
            Guid executionId)
        {
            await EnsurePermissionAsync();

            if (executionId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Inspection execution ID cannot be empty.",
                    nameof(executionId));
            }

            var execution = _repository.GetById(executionId);

            if (execution == null)
                return null;

            var item = await EnsureItemAccessAsync(
                execution.InspectionTestPlanItemId);

            await EnsurePlanAccessAsync(
                item.InspectionTestPlanId);

            return execution;
        }

        public async Task<InspectionTestPlanInspectionExecution> CreateAsync(
            Guid itemId,
            Guid weldId,
            DateTime? inspectionDate,
            string inspectorName,
            string findings,
            string remarks)
        {
            await EnsurePermissionAsync();

            if (itemId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Inspection Test Plan item ID cannot be empty.",
                    nameof(itemId));
            }

            if (weldId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Weld ID cannot be empty.",
                    nameof(weldId));
            }

            var item = await EnsureItemAccessAsync(itemId);
            var plan = await EnsurePlanAccessAsync(
                item.InspectionTestPlanId);

            var weld = await EnsureWeldExistsAsync(weldId);

            await EnsureProjectAccessAsync(plan.ProjectId);

            if (weld.ProjectId != plan.ProjectId)
            {
                throw new UnauthorizedAccessException(
                    "The weld does not belong to the ITP project.");
            }

            if (string.IsNullOrWhiteSpace(inspectorName))
            {
                throw new ArgumentException(
                    "Inspector name is required.",
                    nameof(inspectorName));
            }

            var now = DateTime.UtcNow;

            var execution = new InspectionTestPlanInspectionExecution
            {
                Id = Guid.NewGuid(),
                InspectionTestPlanItemId = item.Id,
                WeldId = weld.Id,
                Status = InspectionTestPlanInspectionExecutionStatus.Pending,
                Result = InspectionTestPlanInspectionResult.Pending,
                InspectionDate = inspectionDate,
                InspectorName = inspectorName.Trim(),
                Findings = findings?.Trim() ?? string.Empty,
                Remarks = remarks?.Trim() ?? string.Empty,
                ExecutedBy = _currentUser.Username ?? _currentUser.UserId,
                ExecutedOn = now,
                CompletedBy = null,
                CompletedOn = null
            };

            _repository.Add(execution);

            _auditService.Log(
                "ITP Inspection Execution Created",
                "Quality.ITP",
                $"ITP inspection execution {execution.Id} created for item {item.SequenceNumber} on {plan.ItpNumber} Rev {plan.Revision}, weld {weld.Id}.");

            return execution;
        }

        public async Task CompleteAsync(
            Guid executionId,
            InspectionTestPlanInspectionResult result,
            DateTime? inspectionDate,
            string inspectorName,
            string findings,
            string remarks)
        {
            await EnsurePermissionAsync();

            var execution = await EnsureExecutionAccessAsync(
                executionId);

            if (execution.Status !=
                InspectionTestPlanInspectionExecutionStatus.Pending)
            {
                throw new InvalidOperationException(
                    $"Inspection execution cannot be completed because its current status is '{execution.Status}'. Only Pending executions can be completed.");
            }

            if (result ==
                InspectionTestPlanInspectionResult.Pending)
            {
                throw new ArgumentException(
                    "A completed inspection must have an inspection result other than Pending.",
                    nameof(result));
            }

            if (string.IsNullOrWhiteSpace(inspectorName))
            {
                throw new ArgumentException(
                    "Inspector name is required.",
                    nameof(inspectorName));
            }

            execution.Status =
                InspectionTestPlanInspectionExecutionStatus.Completed;

            execution.Result = result;
            execution.InspectionDate = inspectionDate;
            execution.InspectorName = inspectorName.Trim();
            execution.Findings = findings?.Trim() ?? string.Empty;
            execution.Remarks = remarks?.Trim() ?? string.Empty;
            execution.CompletedBy =
                _currentUser.Username ?? _currentUser.UserId;
            execution.CompletedOn = DateTime.UtcNow;

            _repository.Update(execution);

            _auditService.Log(
                "ITP Inspection Execution Completed",
                "Quality.ITP",
                $"ITP inspection execution {execution.Id} completed with result {execution.Result}.");
        }

        public async Task CancelAsync(
            Guid executionId,
            string reason)
        {
            await EnsurePermissionAsync();

            var execution = await EnsureExecutionAccessAsync(
                executionId);

            if (execution.Status !=
                InspectionTestPlanInspectionExecutionStatus.Pending)
            {
                throw new InvalidOperationException(
                    $"Inspection execution cannot be cancelled because its current status is '{execution.Status}'. Only Pending executions can be cancelled.");
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new ArgumentException(
                    "Cancellation reason is required.",
                    nameof(reason));
            }

            execution.Status =
                InspectionTestPlanInspectionExecutionStatus.Cancelled;

            execution.Remarks = reason.Trim();
            execution.CompletedBy =
                _currentUser.Username ?? _currentUser.UserId;
            execution.CompletedOn = DateTime.UtcNow;

            _repository.Update(execution);

            _auditService.Log(
                "ITP Inspection Execution Cancelled",
                "Quality.ITP",
                $"ITP inspection execution {execution.Id} cancelled. Reason: {execution.Remarks}");
        }

        private async Task<InspectionTestPlanInspectionExecution>
            EnsureExecutionAccessAsync(Guid executionId)
        {
            if (executionId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Inspection execution ID cannot be empty.",
                    nameof(executionId));
            }

            var execution = _repository.GetById(executionId);

            if (execution == null)
            {
                throw new KeyNotFoundException(
                    $"Inspection execution '{executionId}' was not found.");
            }

            var item = await EnsureItemAccessAsync(
                execution.InspectionTestPlanItemId);

            await EnsurePlanAccessAsync(
                item.InspectionTestPlanId);

            var weld = await EnsureWeldExistsAsync(
                execution.WeldId);

            if (weld.ProjectId !=
                (await EnsurePlanAccessAsync(item.InspectionTestPlanId)).ProjectId)
            {
                throw new UnauthorizedAccessException(
                    "The inspection execution weld does not belong to the ITP project.");
            }

            return execution;
        }

        private async Task<InspectionTestPlanItem>
            EnsureItemAccessAsync(Guid itemId)
        {
            if (itemId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Inspection Test Plan item ID cannot be empty.",
                    nameof(itemId));
            }

            var item = _itemRepository.GetById(itemId);

            if (item == null)
            {
                throw new KeyNotFoundException(
                    $"Inspection Test Plan item '{itemId}' was not found.");
            }

            await EnsurePlanAccessAsync(
                item.InspectionTestPlanId);

            return item;
        }

        private async Task<InspectionTestPlan>
            EnsurePlanAccessAsync(Guid planId)
        {
            if (planId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Inspection Test Plan ID cannot be empty.",
                    nameof(planId));
            }

            var plan = _itpRepository.GetById(planId);

            if (plan == null)
            {
                throw new KeyNotFoundException(
                    $"Inspection Test Plan '{planId}' was not found.");
            }

            await EnsureProjectAccessAsync(plan.ProjectId);

            return plan;
        }

        private async Task<Project>
            EnsureProjectAccessAsync(Guid projectId)
        {
            if (projectId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Project ID cannot be empty.",
                    nameof(projectId));
            }

            var project = _projectRepository.GetById(projectId);

            if (project == null)
            {
                throw new KeyNotFoundException(
                    $"Project '{projectId}' was not found.");
            }

            if (!_currentUser.IsAuthenticated ||
                string.IsNullOrWhiteSpace(_currentUser.UserId))
            {
                throw new UnauthorizedAccessException(
                    "Authentication is required.");
            }

            if (!await _projectAccessAuthorization.CanAccessProjectAsync(
                    _currentUser.UserId,
                    project))
            {
                throw new UnauthorizedAccessException(
                    "User does not have access to the requested project.");
            }

            return project;
        }

        private async Task<Weld>
            EnsureWeldExistsAsync(Guid weldId)
        {
            if (weldId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Weld ID cannot be empty.",
                    nameof(weldId));
            }

            var weld = await _weldRepository.GetByIdAsync(weldId);

            if (weld == null)
            {
                throw new KeyNotFoundException(
                    $"Weld '{weldId}' was not found.");
            }

            return weld;
        }

        private async Task EnsurePermissionAsync()
        {
            if (!_currentUser.IsAuthenticated)
            {
                throw new UnauthorizedAccessException(
                    "User is not authenticated.");
            }

            if (string.IsNullOrWhiteSpace(_currentUser.UserId))
            {
                throw new UnauthorizedAccessException(
                    "Authenticated user identity is required.");
            }

            if (string.IsNullOrWhiteSpace(_currentUser.Role))
            {
                throw new UnauthorizedAccessException(
                    "User does not have an assigned role.");
            }

            var hasPermission =
                await _permissionAuthorization.HasPermissionAsync(
                    _currentUser.UserId,
                    _currentUser.Role,
                    PermissionKeys.Quality.ITP);

            if (!hasPermission)
            {
                throw new UnauthorizedAccessException(
                    "User does not have permission to manage ITP inspection executions.");
            }
        }
    }
}


