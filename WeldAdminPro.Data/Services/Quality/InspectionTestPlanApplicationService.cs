using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WeldAdminPro.Core.Interfaces;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Core.Security;
using WeldAdminPro.Core.Security.Abstractions;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services.Quality
{
    public class InspectionTestPlanApplicationService
    {
        private readonly InspectionTestPlanRepository _repository;
        private readonly InspectionTestPlanItemRepository _itemRepository;
        private readonly InspectionTestPlanExecutionRepository _executionRepository;
        private readonly ProjectQualityControlPlanRepository _qcpRepository;
        private readonly ProjectQualityControlPlanItemRepository _qcpItemRepository;
        private readonly CustomerQualityRequirementRepository _customerQualityRequirementRepository;
        private readonly ProjectRepository _projectRepository;
        private readonly IWeldRepository _weldRepository;
        private readonly HoldPointRepository _holdPointRepository;
        private readonly WeldNdtRepository _weldNdtRepository;
        private readonly ICurrentUserContext _currentUser;
        private readonly AuditService _auditService;
        private readonly IPermissionAuthorizationService _permissionAuthorization;
        private readonly IProjectAccessAuthorizationService _projectAccessAuthorization;

        public InspectionTestPlanApplicationService(
            InspectionTestPlanRepository repository,
            InspectionTestPlanItemRepository itemRepository,
            InspectionTestPlanExecutionRepository executionRepository,
            ProjectQualityControlPlanRepository qcpRepository,
            ProjectQualityControlPlanItemRepository qcpItemRepository,
            CustomerQualityRequirementRepository customerQualityRequirementRepository,
            ProjectRepository projectRepository,
            IWeldRepository weldRepository,
            HoldPointRepository holdPointRepository,
            WeldNdtRepository weldNdtRepository,
            ICurrentUserContext currentUser,
            AuditService auditService,
            IPermissionAuthorizationService permissionAuthorization,
            IProjectAccessAuthorizationService projectAccessAuthorization)
        {
            _repository = repository;
            _itemRepository = itemRepository;
            _executionRepository = executionRepository;
            _qcpRepository = qcpRepository;
            _qcpItemRepository = qcpItemRepository;
            _customerQualityRequirementRepository = customerQualityRequirementRepository;
            _projectRepository = projectRepository;
            _weldRepository = weldRepository;
            _holdPointRepository = holdPointRepository;
            _weldNdtRepository = weldNdtRepository;
            _currentUser = currentUser;
            _auditService = auditService;
            _permissionAuthorization = permissionAuthorization;
            _projectAccessAuthorization = projectAccessAuthorization;
        }

        public async Task<List<InspectionTestPlan>> GetByProjectAsync(
            Guid projectId)
        {
            await EnsurePermissionAsync(PermissionKeys.Quality.ITP);

            await EnsureProjectAccessAsync(projectId);

            return _repository.GetByProject(projectId);
        }

        public async Task<InspectionTestPlan?> GetByIdAsync(
            Guid planId)
        {
            await EnsurePermissionAsync(PermissionKeys.Quality.ITP);

            if (planId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Inspection Test Plan ID cannot be empty.",
                    nameof(planId));
            }

            var plan = _repository.GetById(planId);

            if (plan == null)
            {
                return null;
            }

            await EnsureProjectAccessAsync(plan.ProjectId);

            return plan;
        }

        public async Task<List<InspectionTestPlanItem>> GetItemsAsync(
            Guid planId)
        {
            await EnsurePermissionAsync(PermissionKeys.Quality.ITP);

            var plan = await EnsurePlanAccessAsync(planId);

            return _itemRepository.GetByPlan(plan.Id);
        }

        public async Task<List<InspectionTestPlanExecution>> GetExecutionsAsync(
            Guid itemId)
        {
            await EnsurePermissionAsync(PermissionKeys.Quality.ITP);

            var item = await EnsureItemAccessAsync(itemId);

            return _executionRepository.GetByItem(item.Id);
        }

        public async Task<(ProjectQualityControlPlan? Qcp, ProjectQualityControlPlanItem? QcpItem)> GetQcpLineageAsync(
            Guid itemId)
        {
            await EnsurePermissionAsync(PermissionKeys.Quality.ITP);

            if (itemId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Inspection Test Plan Item ID cannot be empty.",
                    nameof(itemId));
            }

            var item = _itemRepository.GetById(itemId);

            if (item == null)
            {
                return (null, null);
            }

            await EnsureItemAccessAsync(item.Id);

            var qcp = _qcpRepository.GetById(item.QcpRevisionId);

            if (qcp == null)
            {
                return (null, null);
            }

            var qcpItem = _qcpItemRepository.GetById(item.QcpItemId);

            if (qcpItem == null)
            {
                return (qcp, null);
            }

            if (qcpItem.ProjectQualityControlPlanId != qcp.Id)
            {
                throw new InvalidOperationException(
                    "The ITP item's QCP lineage is inconsistent.");
            }

            return (qcp, qcpItem);
        }
        public async Task<InspectionTestPlanItem?> GetItemByIdAsync(
            Guid itemId)
        {
            await EnsurePermissionAsync(PermissionKeys.Quality.ITP);

            if (itemId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Inspection Test Plan Item ID cannot be empty.",
                    nameof(itemId));
            }

            var item = _itemRepository.GetById(itemId);

            if (item == null)
            {
                return null;
            }

            await EnsureItemAccessAsync(item.Id);

            return item;
        }

        public async Task AddAsync(
            InspectionTestPlan plan)
        {
            await EnsurePermissionAsync(PermissionKeys.Quality.ITP);

            if (plan == null)
            {
                throw new ArgumentNullException(nameof(plan));
            }

            await EnsureProjectAccessAsync(plan.ProjectId);

            if (plan.Id == Guid.Empty)
            {
                plan.Id = Guid.NewGuid();
            }

            plan.Status = InspectionTestPlanStatus.Draft;
            plan.IsActive = true;
            plan.PreparedBy = _currentUser.Username;
            plan.PreparedOn = DateTime.UtcNow;
            plan.ApprovedBy = null;
            plan.ApprovedOn = null;
            plan.EffectiveDate = null;
            plan.SupersededOn = null;
            plan.PreviousRevisionId = null;

            ValidatePlan(plan);

            EnsureNoDuplicateRevision(
                plan.ProjectId,
                plan.ItpNumber,
                plan.Revision,
                null);

            _repository.Add(plan);

            _auditService.Log(
                "ITP Created",
                "Quality.ITP",
                $"ITP {plan.ItpNumber} Rev {plan.Revision} created for project {plan.ProjectId}.");
        }

        public async Task UpdateAsync(
            InspectionTestPlan plan)
        {
            await EnsurePermissionAsync(PermissionKeys.Quality.ITP);

            if (plan == null)
            {
                throw new ArgumentNullException(nameof(plan));
            }

            if (plan.Id == Guid.Empty)
            {
                throw new ArgumentException(
                    "Inspection Test Plan ID cannot be empty.",
                    nameof(plan));
            }

            var existing = _repository.GetById(plan.Id);

            if (existing == null)
            {
                throw new KeyNotFoundException(
                    "Inspection Test Plan was not found.");
            }

            await EnsureProjectAccessAsync(existing.ProjectId);

            EnsureDraftPlan(existing);

            if (plan.ProjectId != existing.ProjectId)
            {
                throw new InvalidOperationException(
                    "An Inspection Test Plan cannot be moved to another project.");
            }

            plan.Status = existing.Status;
            plan.IsActive = existing.IsActive;
            plan.PreparedBy = existing.PreparedBy;
            plan.PreparedOn = existing.PreparedOn;
            plan.ApprovedBy = existing.ApprovedBy;
            plan.ApprovedOn = existing.ApprovedOn;
            plan.EffectiveDate = existing.EffectiveDate;
            plan.SupersededOn = existing.SupersededOn;
            plan.PreviousRevisionId = existing.PreviousRevisionId;

            ValidatePlan(plan);

            EnsureNoDuplicateRevision(
                plan.ProjectId,
                plan.ItpNumber,
                plan.Revision,
                plan.Id);

            _repository.Update(plan);

            _auditService.Log(
                "ITP Updated",
                "Quality.ITP",
                $"ITP {plan.ItpNumber} Rev {plan.Revision} updated.");
        }

        public async Task ApproveAsync(
            Guid planId)
        {
            await EnsurePermissionAsync(PermissionKeys.Quality.ITP);

            var plan = await EnsurePlanAccessAsync(planId);

            EnsureDraftPlan(plan);

            var items = _itemRepository.GetByPlan(plan.Id);

            if (items.Count == 0)
            {
                throw new InvalidOperationException(
                    "An Inspection Test Plan must contain at least one item before approval.");
            }

            if (!_currentUser.IsAuthenticated ||
                string.IsNullOrWhiteSpace(_currentUser.Username))
            {
                throw new UnauthorizedAccessException(
                    "The approving user could not be established.");
            }

            plan.Status = InspectionTestPlanStatus.Active;
            plan.IsActive = true;
            plan.ApprovedBy = _currentUser.Username;
            plan.ApprovedOn = DateTime.UtcNow;
            plan.EffectiveDate = DateTime.UtcNow;
            plan.SupersededOn = null;

            _repository.Update(plan);

            _auditService.Log(
                "ITP Approved",
                "Quality.ITP",
                $"ITP {plan.ItpNumber} Rev {plan.Revision} approved by {plan.ApprovedBy}.");
        }

        public async Task CreateRevisionAsync(
            Guid planId,
            string revision)
        {
            await EnsurePermissionAsync(PermissionKeys.Quality.ITP);

            if (planId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Inspection Test Plan ID cannot be empty.",
                    nameof(planId));
            }

            if (string.IsNullOrWhiteSpace(revision))
            {
                throw new ArgumentException(
                    "New ITP revision is required.",
                    nameof(revision));
            }

            var existing = await EnsurePlanAccessAsync(planId);

            if (existing.Status != InspectionTestPlanStatus.Active ||
                !existing.IsActive)
            {
                throw new InvalidOperationException(
                    "Only the active ITP revision can be revised.");
            }

            EnsureNoDuplicateRevision(
                existing.ProjectId,
                existing.ItpNumber,
                revision.Trim(),
                null);

            var previousItems = _itemRepository.GetByPlan(existing.Id);

            var newRevision = new InspectionTestPlan
            {
                Id = Guid.NewGuid(),
                ProjectId = existing.ProjectId,
                ItpNumber = existing.ItpNumber,
                Revision = revision.Trim(),
                Status = InspectionTestPlanStatus.Draft,
                Title = existing.Title,
                Description = existing.Description,
                PreparedBy = _currentUser.Username,
                PreparedOn = DateTime.UtcNow,
                ApprovedBy = null,
                ApprovedOn = null,
                EffectiveDate = null,
                SupersededOn = null,
                PreviousRevisionId = existing.Id,
                IsActive = true
            };

            var revisionItems =
                new List<InspectionTestPlanItem>();

            foreach (var previousItem in previousItems)
            {
                revisionItems.Add(
                    new InspectionTestPlanItem
                    {
                        Id = Guid.NewGuid(),
                        InspectionTestPlanId = newRevision.Id,
                        SequenceNumber = previousItem.SequenceNumber,
                        QcpRevisionId = previousItem.QcpRevisionId,
                        QcpItemId = previousItem.QcpItemId,
                        CustomerQualityRequirementId =
                            previousItem.CustomerQualityRequirementId,
                        Activity = previousItem.Activity,
                        AcceptanceCriteria = previousItem.AcceptanceCriteria,
                        VerificationMethod = previousItem.VerificationMethod,
                        ResponsibleParty = previousItem.ResponsibleParty,
                        InspectionStage = previousItem.InspectionStage,
                        HoldPointCategory = previousItem.HoldPointCategory,
                        HoldPointType = previousItem.HoldPointType,
                        RequiredNdtMethod = previousItem.RequiredNdtMethod,
                        Mandatory = previousItem.Mandatory,
                        RequiredEvidence = previousItem.RequiredEvidence,
                        ExecutionType = previousItem.ExecutionType,
                        Notes = previousItem.Notes
                    });
            }

            ValidatePlan(newRevision);

            foreach (var item in revisionItems)
            {
                ValidateItem(item);
            }

            _repository.SaveRevision(
                existing,
                newRevision,
                revisionItems);

            _auditService.Log(
                "ITP Revision Created",
                "Quality.ITP",
                $"ITP {newRevision.ItpNumber} Rev {newRevision.Revision} created from Rev {existing.Revision}.");

            _auditService.Log(
                "ITP Revision Execution Associations Reset",
                "Quality.ITP",
                $"Execution associations were not copied to ITP {newRevision.ItpNumber} Rev {newRevision.Revision}.");
        }

        public async Task DeleteAsync(
            Guid planId)
        {
            await EnsurePermissionAsync(PermissionKeys.Quality.ITP);

            var plan = await EnsurePlanAccessAsync(planId);

            EnsureDraftPlan(plan);

            var items = _itemRepository.GetByPlan(plan.Id);

            if (items.Count > 0)
            {
                throw new InvalidOperationException(
                    "An ITP with items cannot be deleted.");
            }

            _repository.Delete(plan.Id);

            _auditService.Log(
                "ITP Deleted",
                "Quality.ITP",
                $"ITP {plan.ItpNumber} Rev {plan.Revision} deleted.");
        }

        public async Task AddItemAsync(
            InspectionTestPlanItem item)
        {
            await EnsurePermissionAsync(PermissionKeys.Quality.ITP);

            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            var plan = await EnsurePlanAccessAsync(
                item.InspectionTestPlanId);

            EnsureDraftPlan(plan);

            if (item.Id == Guid.Empty)
            {
                item.Id = Guid.NewGuid();
            }

            item.InspectionTestPlanId = plan.Id;

            ValidateItem(item);

            await ValidateQcpLineageAsync(item, plan);
            await ValidateCustomerQualityRequirementAsync(item, plan);

            EnsureNoDuplicateSequence(
                plan.Id,
                item.SequenceNumber,
                null);

            _itemRepository.Add(item);

            _auditService.Log(
                "ITP Item Added",
                "Quality.ITP",
                $"ITP item {item.SequenceNumber} ({item.Activity}) added to {plan.ItpNumber} Rev {plan.Revision}.");
        }

        public async Task UpdateItemAsync(
            InspectionTestPlanItem item)
        {
            await EnsurePermissionAsync(PermissionKeys.Quality.ITP);

            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (item.Id == Guid.Empty)
            {
                throw new ArgumentException(
                    "Inspection Test Plan Item ID cannot be empty.",
                    nameof(item));
            }

            var existing = _itemRepository.GetById(item.Id);

            if (existing == null)
            {
                throw new KeyNotFoundException(
                    "Inspection Test Plan Item was not found.");
            }

            var plan = await EnsurePlanAccessAsync(
                existing.InspectionTestPlanId);

            EnsureDraftPlan(plan);

            if (item.InspectionTestPlanId != plan.Id)
            {
                throw new InvalidOperationException(
                    "An Inspection Test Plan item cannot be moved to another plan.");
            }

            ValidateItem(item);

            await ValidateQcpLineageAsync(item, plan);
            await ValidateCustomerQualityRequirementAsync(item, plan);

            EnsureNoDuplicateSequence(
                plan.Id,
                item.SequenceNumber,
                item.Id);

            _itemRepository.Update(item);

            _auditService.Log(
                "ITP Item Updated",
                "Quality.ITP",
                $"ITP item {item.SequenceNumber} ({item.Activity}) updated on {plan.ItpNumber} Rev {plan.Revision}.");
        }

        public async Task DeleteItemAsync(
            Guid itemId)
        {
            await EnsurePermissionAsync(PermissionKeys.Quality.ITP);

            var item = await EnsureItemAccessAsync(itemId);

            var plan = await EnsurePlanAccessAsync(
                item.InspectionTestPlanId);

            EnsureDraftPlan(plan);

            var executions =
                _executionRepository.GetByItem(item.Id);

            if (executions.Count > 0)
            {
                throw new InvalidOperationException(
                    "An ITP item with execution associations cannot be deleted.");
            }

            _itemRepository.Delete(item.Id);

            _auditService.Log(
                "ITP Item Deleted",
                "Quality.ITP",
                $"ITP item {item.SequenceNumber} ({item.Activity}) deleted from {plan.ItpNumber} Rev {plan.Revision}.");
        }

        public async Task LinkExecutionAsync(
            Guid itemId,
            InspectionTestPlanExecutionType executionType,
            string executionRecordId,
            string? notes)
        {
            await EnsurePermissionAsync(PermissionKeys.Quality.ITP);

            var item = await EnsureItemAccessAsync(itemId);

            var plan = await EnsurePlanAccessAsync(
                item.InspectionTestPlanId);

            var normalizedRecordId =
                executionRecordId?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(normalizedRecordId))
            {
                throw new ArgumentException(
                    "Execution record ID is required.",
                    nameof(executionRecordId));
            }

            var executionTypeText =
                executionType.ToString();

            await ValidateExecutionRecordAsync(
                executionType,
                normalizedRecordId,
                plan.ProjectId);

            if (_executionRepository.Exists(
                    item.Id,
                    executionTypeText,
                    normalizedRecordId))
            {
                throw new InvalidOperationException(
                    "This execution record is already linked to the ITP item.");
            }

            var execution = new InspectionTestPlanExecution
            {
                Id = Guid.NewGuid(),
                InspectionTestPlanItemId = item.Id,
                ExecutionType = executionTypeText,
                ExecutionRecordId = normalizedRecordId,
                LinkedOn = DateTime.UtcNow,
                LinkedBy = _currentUser.Username,
                Notes = string.IsNullOrWhiteSpace(notes)
                    ? null
                    : notes.Trim()
            };

            _executionRepository.Add(execution);

            _auditService.Log(
                "ITP Execution Linked",
                "Quality.ITP",
                $"Execution {execution.ExecutionType}:{execution.ExecutionRecordId} linked to ITP item {item.SequenceNumber} on {plan.ItpNumber} Rev {plan.Revision}.");
        }

        public async Task UnlinkExecutionAsync(
            Guid executionId)
        {
            await EnsurePermissionAsync(PermissionKeys.Quality.ITP);

            if (executionId == Guid.Empty)
            {
                throw new ArgumentException(
                    "ITP execution association ID cannot be empty.",
                    nameof(executionId));
            }

            var execution =
                _executionRepository.GetById(executionId);

            if (execution == null)
            {
                throw new KeyNotFoundException(
                    "ITP execution association was not found.");
            }

            var item = await EnsureItemAccessAsync(
                execution.InspectionTestPlanItemId);

            var plan = await EnsurePlanAccessAsync(
                item.InspectionTestPlanId);

            _executionRepository.Delete(execution.Id);

            _auditService.Log(
                "ITP Execution Unlinked",
                "Quality.ITP",
                $"Execution {execution.ExecutionType}:{execution.ExecutionRecordId} unlinked from ITP item {item.SequenceNumber} on {plan.ItpNumber} Rev {plan.Revision}.");
        }

        private async Task ValidateExecutionRecordAsync(
            InspectionTestPlanExecutionType executionType,
            string executionRecordId,
            Guid projectId)
        {
            if (!Guid.TryParse(
                    executionRecordId,
                    out var recordId))
            {
                throw new ArgumentException(
                    "Execution record ID must be a valid GUID.",
                    nameof(executionRecordId));
            }

            switch (executionType)
            {
                case InspectionTestPlanExecutionType.HoldPoint:
                {
                    var holdPoint =
                        _holdPointRepository.GetById(recordId);

                    if (holdPoint == null)
                    {
                        throw new KeyNotFoundException(
                            "The referenced Hold Point was not found.");
                    }

                    var weld =
                        await _weldRepository.GetByIdAsync(
                            holdPoint.WeldId);

                    if (weld == null)
                    {
                        throw new KeyNotFoundException(
                            "The Hold Point's weld was not found.");
                    }

                    if (weld.ProjectId != projectId)
                    {
                        throw new UnauthorizedAccessException(
                            "The referenced Hold Point does not belong to the ITP project.");
                    }

                    return;
                }

                case InspectionTestPlanExecutionType.NdtResult:
                {
                    var ndt =
                        _weldNdtRepository.GetById(recordId);

                    if (ndt == null)
                    {
                        throw new KeyNotFoundException(
                            "The referenced NDT result was not found.");
                    }

                    var weld =
                        await _weldRepository.GetByIdAsync(
                            ndt.WeldId);

                    if (weld == null)
                    {
                        throw new KeyNotFoundException(
                            "The NDT result's weld was not found.");
                    }

                    if (weld.ProjectId != projectId)
                    {
                        throw new UnauthorizedAccessException(
                            "The referenced NDT result does not belong to the ITP project.");
                    }

                    return;
                }

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(executionType),
                        executionType,
                        "Unsupported ITP execution type.");
            }
        }

        private async Task<InspectionTestPlanItem> EnsureItemAccessAsync(
            Guid itemId)
        {
            if (itemId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Inspection Test Plan Item ID cannot be empty.",
                    nameof(itemId));
            }

            var item = _itemRepository.GetById(itemId);

            if (item == null)
            {
                throw new KeyNotFoundException(
                    "Inspection Test Plan Item was not found.");
            }

            await EnsurePlanAccessAsync(
                item.InspectionTestPlanId);

            return item;
        }

        private async Task<InspectionTestPlan> EnsurePlanAccessAsync(
            Guid planId)
        {
            if (planId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Inspection Test Plan ID cannot be empty.",
                    nameof(planId));
            }

            var plan = _repository.GetById(planId);

            if (plan == null)
            {
                throw new KeyNotFoundException(
                    "Inspection Test Plan was not found.");
            }

            await EnsureProjectAccessAsync(plan.ProjectId);

            return plan;
        }

        private async Task<Project> EnsureProjectAccessAsync(
            Guid projectId)
        {
            if (projectId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Project ID cannot be empty.",
                    nameof(projectId));
            }

            if (!_currentUser.IsAuthenticated)
            {
                throw new UnauthorizedAccessException(
                    "User is not authenticated.");
            }

            if (string.IsNullOrWhiteSpace(_currentUser.UserId))
            {
                throw new UnauthorizedAccessException(
                    "User identity could not be established.");
            }

            var project = _projectRepository.GetById(projectId);

            if (project == null)
            {
                throw new KeyNotFoundException(
                    "Project was not found.");
            }

            if (!await _projectAccessAuthorization.CanAccessProjectAsync(
                    _currentUser.UserId,
                    project))
            {
                throw new UnauthorizedAccessException(
                    "You do not have access to this project.");
            }

            return project;
        }

        private async Task EnsurePermissionAsync(
            string permissionKey)
        {
            if (!_currentUser.IsAuthenticated)
            {
                throw new UnauthorizedAccessException(
                    "User is not authenticated.");
            }

            if (string.IsNullOrWhiteSpace(_currentUser.UserId))
            {
                throw new UnauthorizedAccessException(
                    "User identity could not be established.");
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
                    permissionKey);

            if (!hasPermission)
            {
                throw new UnauthorizedAccessException(
                    $"User does not have permission: {permissionKey}");
            }
        }

        private static void EnsureDraftPlan(
            InspectionTestPlan plan)
        {
            if (plan.Status != InspectionTestPlanStatus.Draft)
            {
                throw new InvalidOperationException(
                    "Inspection Test Plan can only be changed while the plan is Draft.");
            }
        }

        private static void ValidatePlan(
            InspectionTestPlan plan)
        {
            if (plan.ProjectId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Project ID is required.",
                    nameof(plan));
            }

            if (string.IsNullOrWhiteSpace(plan.ItpNumber))
            {
                throw new ArgumentException(
                    "ITP number is required.",
                    nameof(plan));
            }

            if (string.IsNullOrWhiteSpace(plan.Revision))
            {
                throw new ArgumentException(
                    "ITP revision is required.",
                    nameof(plan));
            }

            if (string.IsNullOrWhiteSpace(plan.Title))
            {
                throw new ArgumentException(
                    "ITP title is required.",
                    nameof(plan));
            }
        }

        private static void ValidateItem(
            InspectionTestPlanItem item)
        {
            if (item.InspectionTestPlanId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Inspection Test Plan ID is required.",
                    nameof(item));
            }

            if (item.SequenceNumber <= 0)
            {
                throw new ArgumentException(
                    "Sequence number must be greater than zero.",
                    nameof(item));
            }

            if (item.QcpRevisionId == Guid.Empty)
            {
                throw new ArgumentException(
                    "QCP revision ID is required.",
                    nameof(item));
            }

            if (item.QcpItemId == Guid.Empty)
            {
                throw new ArgumentException(
                    "QCP item ID is required.",
                    nameof(item));
            }

            if (string.IsNullOrWhiteSpace(item.Activity))
            {
                throw new ArgumentException(
                    "Activity is required.",
                    nameof(item));
            }

            if (string.IsNullOrWhiteSpace(item.AcceptanceCriteria))
            {
                throw new ArgumentException(
                    "Acceptance criteria is required.",
                    nameof(item));
            }

            if (string.IsNullOrWhiteSpace(item.VerificationMethod))
            {
                throw new ArgumentException(
                    "Verification method is required.",
                    nameof(item));
            }

            if (string.IsNullOrWhiteSpace(item.ResponsibleParty))
            {
                throw new ArgumentException(
                    "Responsible party is required.",
                    nameof(item));
            }

            if (string.IsNullOrWhiteSpace(item.ExecutionType))
            {
                throw new ArgumentException(
                    "Execution type is required.",
                    nameof(item));
            }

            if (!Enum.TryParse<InspectionTestPlanExecutionType>(
                    item.ExecutionType,
                    true,
                    out _))
            {
                throw new ArgumentException(
                    $"Unsupported ITP execution type: {item.ExecutionType}.",
                    nameof(item));
            }
        }

        private async Task ValidateQcpLineageAsync(
            InspectionTestPlanItem item,
            InspectionTestPlan plan)
        {
            var qcp =
                _qcpRepository.GetById(item.QcpRevisionId);

            if (qcp == null)
            {
                throw new KeyNotFoundException(
                    "The referenced QCP revision was not found.");
            }

            if (qcp.ProjectId != plan.ProjectId)
            {
                throw new InvalidOperationException(
                    "The QCP revision must belong to the same project as the ITP.");
            }

            var qcpItem =
                _qcpItemRepository.GetById(item.QcpItemId);

            if (qcpItem == null)
            {
                throw new KeyNotFoundException(
                    "The referenced QCP item was not found.");
            }

            if (qcpItem.ProjectQualityControlPlanId != qcp.Id)
            {
                throw new InvalidOperationException(
                    "The QCP item does not belong to the referenced QCP revision.");
            }

            if (!string.Equals(
                    qcp.Status.ToString(),
                    "Active",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "The ITP must reference an Active QCP revision.");
            }

            if (qcp.IsActive == false)
            {
                throw new InvalidOperationException(
                    "The ITP must reference the active QCP revision.");
            }

            await Task.CompletedTask;
        }

        private async Task ValidateCustomerQualityRequirementAsync(
            InspectionTestPlanItem item,
            InspectionTestPlan plan)
        {
            if (!item.CustomerQualityRequirementId.HasValue)
            {
                return;
            }

            var requirement =
                _customerQualityRequirementRepository.GetById(
                    item.CustomerQualityRequirementId.Value);

            if (requirement == null)
            {
                throw new KeyNotFoundException(
                    "Customer Quality Requirement was not found.");
            }

            if (requirement.ProjectId != plan.ProjectId)
            {
                throw new InvalidOperationException(
                    "The Customer Quality Requirement must belong to the same project as the ITP.");
            }

            await Task.CompletedTask;
        }

        private void EnsureNoDuplicateRevision(
            Guid projectId,
            string itpNumber,
            string revision,
            Guid? excludingId)
        {
            var existingPlans =
                _repository.GetByProject(projectId);

            foreach (var candidate in existingPlans)
            {
                if (excludingId.HasValue &&
                    candidate.Id == excludingId.Value)
                {
                    continue;
                }

                if (string.Equals(
                        candidate.ItpNumber,
                        itpNumber,
                        StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(
                        candidate.Revision,
                        revision,
                        StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        "An ITP with the same number and revision already exists for this project.");
                }
            }
        }

        private void EnsureNoDuplicateSequence(
            Guid planId,
            int sequenceNumber,
            Guid? excludingId)
        {
            var existingItems =
                _itemRepository.GetByPlan(planId);

            foreach (var candidate in existingItems)
            {
                if (excludingId.HasValue &&
                    candidate.Id == excludingId.Value)
                {
                    continue;
                }

                if (candidate.SequenceNumber == sequenceNumber)
                {
                    throw new InvalidOperationException(
                        "An ITP item with the same sequence number already exists.");
                }
            }
        }
    }
}


