using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Core.Security;
using WeldAdminPro.Core.Security.Abstractions;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services.Quality
{
    public class ProjectQualityControlPlanApplicationService
    {
        private readonly ProjectQualityControlPlanRepository _repository;
        private readonly ProjectQualityControlPlanItemRepository _itemRepository;
        private readonly CustomerQualityRequirementRepository _customerQualityRequirementRepository;
        private readonly ProjectRepository _projectRepository;
        private readonly ICurrentUserContext _currentUser;
        private readonly AuditService _auditService;
        private readonly ProjectQualityControlPlanItemApprovalHistoryRepository _itemApprovalHistoryRepository;
        private readonly QcpMasterActivityRepository _qcpMasterActivityRepository;
        private readonly IPermissionAuthorizationService _permissionAuthorization;
        private readonly IProjectAccessAuthorizationService _projectAccessAuthorization;

        public ProjectQualityControlPlanApplicationService(
            ProjectQualityControlPlanRepository repository,
            ProjectQualityControlPlanItemRepository itemRepository,
            CustomerQualityRequirementRepository customerQualityRequirementRepository,
            ProjectRepository projectRepository,
            ICurrentUserContext currentUser,
            IPermissionAuthorizationService permissionAuthorization,
            IProjectAccessAuthorizationService projectAccessAuthorization,
            ProjectQualityControlPlanItemApprovalHistoryRepository itemApprovalHistoryRepository,
            AuditService auditService)
        {
            _repository = repository;
            _itemRepository = itemRepository;
            _customerQualityRequirementRepository = customerQualityRequirementRepository;
            _projectRepository = projectRepository;
            _currentUser = currentUser;
            _permissionAuthorization = permissionAuthorization;
            _projectAccessAuthorization = projectAccessAuthorization;
            _auditService = auditService;
            _itemApprovalHistoryRepository = itemApprovalHistoryRepository;
        }

        public async Task<List<ProjectQualityControlPlan>> GetByProjectAsync(
            Guid projectId)
        {
            await EnsurePermissionAsync(PermissionKeys.Quality.QCP);

            var project = await EnsureProjectAccessAsync(projectId);

            return _repository.GetByProject(project.Id);
        }

        public async Task<ProjectQualityControlPlan?> GetByIdAsync(
            Guid id)
        {
            await EnsurePermissionAsync(PermissionKeys.Quality.QCP);

            if (id == Guid.Empty)
            {
                throw new ArgumentException(
                    "Quality Control Plan ID cannot be empty.",
                    nameof(id));
            }

            var plan = _repository.GetById(id);

            if (plan == null)
            {
                return null;
            }

            await EnsureProjectAccessAsync(plan.ProjectId);

            return plan;
        }

        public async Task AddAsync(
            ProjectQualityControlPlan plan)
        {
            if (plan == null)
            {
                throw new ArgumentNullException(nameof(plan));
            }

            await EnsurePermissionAsync(PermissionKeys.Quality.QCP);

            var project = await EnsureProjectAccessAsync(plan.ProjectId);

            if (plan.Id == Guid.Empty)
            {
                plan.Id = Guid.NewGuid();
            }

            plan.ProjectId = project.Id;
            plan.Status = ProjectQualityControlPlanStatus.Draft;
            plan.IsActive = false;
            plan.PreparedBy = _currentUser.Username;
            plan.PreparedOn = DateTime.UtcNow;
            plan.ApprovedBy = null;
            plan.ApprovedOn = null;
            plan.EffectiveDate = null;
            plan.SupersededOn = null;
            plan.PreviousRevisionId = null;

            ValidatePlan(plan);

            var existingPlans = _repository.GetByProject(project.Id);

            foreach (var existing in existingPlans)
            {
                if (string.Equals(
                        existing.QcpNumber,
                        plan.QcpNumber,
                        StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(
                        existing.Revision,
                        plan.Revision,
                        StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        "A Quality Control Plan with the same number and revision already exists for this project.");
                }
            }

            _repository.Add(plan);

            _auditService.Log(
                "QCP Created",
                "Quality.QCP",
                $"QCP {plan.QcpNumber} Rev {plan.Revision} created for project {plan.ProjectId}.");
        }

        public async Task UpdateAsync(
            ProjectQualityControlPlan plan)
        {
            if (plan == null)
            {
                throw new ArgumentNullException(nameof(plan));
            }

            await EnsurePermissionAsync(PermissionKeys.Quality.QCP);

            if (plan.Id == Guid.Empty)
            {
                throw new ArgumentException(
                    "Quality Control Plan ID cannot be empty.",
                    nameof(plan));
            }

            var existing = _repository.GetById(plan.Id);

            if (existing == null)
            {
                throw new KeyNotFoundException(
                    "Quality Control Plan was not found.");
            }

            var project = await EnsureProjectAccessAsync(existing.ProjectId);

            if (plan.ProjectId != existing.ProjectId)
            {
                throw new InvalidOperationException(
                    "A Quality Control Plan cannot be moved to another project.");
            }

            if (existing.Status != ProjectQualityControlPlanStatus.Draft)
            {
                throw new InvalidOperationException(
                    "Only Draft Quality Control Plans can be edited.");
            }

            plan.Status = existing.Status;
            plan.IsActive = existing.IsActive;
            plan.PreviousRevisionId = existing.PreviousRevisionId;
            plan.ApprovedBy = existing.ApprovedBy;
            plan.ApprovedOn = existing.ApprovedOn;
            plan.EffectiveDate = existing.EffectiveDate;
            plan.SupersededOn = existing.SupersededOn;

            if (string.IsNullOrWhiteSpace(plan.PreparedBy))
            {
                plan.PreparedBy = existing.PreparedBy;
            }

            if (!plan.PreparedOn.HasValue)
            {
                plan.PreparedOn = existing.PreparedOn;
            }

            ValidatePlan(plan);

            var existingPlans = _repository.GetByProject(project.Id);

            foreach (var candidate in existingPlans)
            {
                if (candidate.Id == plan.Id)
                {
                    continue;
                }

                if (string.Equals(
                        candidate.QcpNumber,
                        plan.QcpNumber,
                        StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(
                        candidate.Revision,
                        plan.Revision,
                        StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        "A Quality Control Plan with the same number and revision already exists for this project.");
                }
            }

            _repository.Update(plan);

            _auditService.Log(
                "QCP Updated",
                "Quality.QCP",
                $"QCP {plan.QcpNumber} Rev {plan.Revision} updated for project {plan.ProjectId}.");
        }

        public async Task<ProjectQualityControlPlan> CreateRevisionAsync(
            Guid id,
            string revision)
        {
            await EnsurePermissionAsync(PermissionKeys.Quality.QCP);

            if (id == Guid.Empty)
            {
                throw new ArgumentException(
                    "Quality Control Plan ID cannot be empty.",
                    nameof(id));
            }

            if (string.IsNullOrWhiteSpace(revision))
            {
                throw new ArgumentException(
                    "Revision is required.",
                    nameof(revision));
            }

            var existing = _repository.GetById(id);

            if (existing == null)
            {
                throw new KeyNotFoundException(
                    "Quality Control Plan was not found.");
            }

            await EnsureProjectAccessAsync(existing.ProjectId);

            if (existing.Status != ProjectQualityControlPlanStatus.Active ||
                !existing.IsActive)
            {
                throw new InvalidOperationException(
                    "A new revision can only be created from an Active Quality Control Plan.");
            }

            var plans = _repository.GetByProject(existing.ProjectId);

            foreach (var candidate in plans)
            {
                if (string.Equals(
                        candidate.QcpNumber,
                        existing.QcpNumber,
                        StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(
                        candidate.Revision,
                        revision,
                        StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        "A Quality Control Plan with the same number and revision already exists for this project.");
                }
            }

            var newRevision = new ProjectQualityControlPlan
            {
                Id = Guid.NewGuid(),
                ProjectId = existing.ProjectId,
                QcpNumber = existing.QcpNumber,
                Revision = revision.Trim(),
                Status = ProjectQualityControlPlanStatus.Draft,
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

            ValidatePlan(newRevision);

            var previousItems = _itemRepository.GetByPlan(existing.Id);
            var revisionItems = new List<ProjectQualityControlPlanItem>();

            foreach (var item in previousItems)
            {
                revisionItems.Add(new ProjectQualityControlPlanItem
                {
                    Id = Guid.NewGuid(),
                    ProjectQualityControlPlanId = newRevision.Id,
                    SequenceNumber = item.SequenceNumber,
                    Activity = item.Activity,
                    CustomerQualityRequirementId = item.CustomerQualityRequirementId,
                    AcceptanceCriteria = item.AcceptanceCriteria,
                    VerificationMethod = item.VerificationMethod,
                    ResponsibleParty = item.ResponsibleParty,
                    InspectionStage = item.InspectionStage,
                    HoldPointCategory = item.HoldPointCategory,
                    HoldPointType = item.HoldPointType,
                    RequiredNdtMethod = item.RequiredNdtMethod,
                    Mandatory = item.Mandatory,
                    RequiredEvidence = item.RequiredEvidence,
                    Notes = item.Notes
                });
            }

            _repository.SaveRevision(
                existing,
                newRevision,
                revisionItems);

            _auditService.Log(
                "QCP Revision Created",
                "Quality.QCP",
                $"QCP {newRevision.QcpNumber} Rev {newRevision.Revision} created from Rev {existing.Revision} for project {newRevision.ProjectId}.");

            return newRevision;
        }
        public async Task ApproveAsync(Guid id)
        {
            await EnsurePermissionAsync(PermissionKeys.Quality.QCP);

            if (id == Guid.Empty)
            {
                throw new ArgumentException(
                    "Quality Control Plan ID cannot be empty.",
                    nameof(id));
            }

            var plan = _repository.GetById(id);

            if (plan == null)
            {
                throw new KeyNotFoundException(
                    "Quality Control Plan was not found.");
            }

            await EnsureProjectAccessAsync(plan.ProjectId);

            if (plan.Status != ProjectQualityControlPlanStatus.Draft)
            {
                throw new InvalidOperationException(
                    "Only a Draft Quality Control Plan can be approved.");
            }

            ValidatePlan(plan);

            if (string.IsNullOrWhiteSpace(_currentUser.Username))
            {
                throw new UnauthorizedAccessException(
                    "Current user identity could not be established.");
            }

            plan.Status = ProjectQualityControlPlanStatus.Active;
            plan.IsActive = true;
            plan.ApprovedBy = _currentUser.Username;
            plan.ApprovedOn = DateTime.UtcNow;
            plan.EffectiveDate = DateTime.UtcNow;
            plan.SupersededOn = null;

            _repository.Update(plan);

            _auditService.Log(
                "QCP Approved",
                "Quality.QCP",
                $"QCP {plan.QcpNumber} Rev {plan.Revision} approved by {plan.ApprovedBy} for project {plan.ProjectId}.");
        }

        public async Task DeleteAsync(Guid id)
        {
            await EnsurePermissionAsync(PermissionKeys.Quality.QCP);

            if (id == Guid.Empty)
            {
                throw new ArgumentException(
                    "Quality Control Plan ID cannot be empty.",
                    nameof(id));
            }

            var plan = _repository.GetById(id);

            if (plan == null)
            {
                throw new KeyNotFoundException(
                    "Quality Control Plan was not found.");
            }

            await EnsureProjectAccessAsync(plan.ProjectId);

            if (plan.Status != ProjectQualityControlPlanStatus.Draft)
            {
                throw new InvalidOperationException(
                    "Only Draft Quality Control Plans can be deleted.");
            }

            _repository.Delete(id);

            _auditService.Log(
                "QCP Deleted",
                "Quality.QCP",
                $"QCP {plan.QcpNumber} Rev {plan.Revision} deleted from project {plan.ProjectId}.");
        }

        public async Task<List<ProjectQualityControlPlanItem>> GetItemsAsync(
            Guid planId)
        {
            await EnsurePermissionAsync(PermissionKeys.Quality.QCP);

            var plan = await EnsurePlanAccessAsync(planId);

            return _itemRepository.GetByPlan(plan.Id);
        }

        public async Task<ProjectQualityControlPlanItem?> GetItemByIdAsync(
            Guid itemId)
        {
            await EnsurePermissionAsync(PermissionKeys.Quality.QCP);

            if (itemId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Quality Control Plan Item ID cannot be empty.",
                    nameof(itemId));
            }

            var item = _itemRepository.GetById(itemId);

            if (item == null)
            {
                return null;
            }

            await EnsurePlanAccessAsync(item.ProjectQualityControlPlanId);

            return item;
        }

        public async Task AddItemAsync(
            ProjectQualityControlPlanItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            await EnsurePermissionAsync(PermissionKeys.Quality.QCP);

            var plan = await EnsurePlanAccessAsync(
                item.ProjectQualityControlPlanId);

            EnsureDraftPlan(plan);

            if (item.Id == Guid.Empty)
            {
                item.Id = Guid.NewGuid();
            }

            item.ProjectQualityControlPlanId = plan.Id;

            ValidateItem(item);
            await ValidateCustomerQualityRequirementAsync(item, plan);

            var existingItems = _itemRepository.GetByPlan(plan.Id);

            foreach (var existing in existingItems)
            {
                if (existing.SequenceNumber == item.SequenceNumber)
                {
                    throw new InvalidOperationException(
                        "A Quality Control Plan item with the same sequence number already exists.");
                }
            }

            _itemRepository.Add(item);

            _auditService.Log(
                "QCP Item Added",
                "Quality.QCP",
                $"QCP item {item.SequenceNumber} ({item.Activity}) added to QCP {plan.QcpNumber} Rev {plan.Revision}.");
        }

        public async Task UpdateItemAsync(
            ProjectQualityControlPlanItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            await EnsurePermissionAsync(PermissionKeys.Quality.QCP);

            if (item.Id == Guid.Empty)
            {
                throw new ArgumentException(
                    "Quality Control Plan Item ID cannot be empty.",
                    nameof(item));
            }

            var existing = _itemRepository.GetById(item.Id);

            if (existing == null)
            {
                throw new KeyNotFoundException(
                    "Quality Control Plan Item was not found.");
            }

            var plan = await EnsurePlanAccessAsync(
                existing.ProjectQualityControlPlanId);

            EnsureDraftPlan(plan);

            if (item.ProjectQualityControlPlanId != existing.ProjectQualityControlPlanId)
            {
                throw new InvalidOperationException(
                    "A Quality Control Plan item cannot be moved to another plan.");
            }

            ValidateItem(item);
            await ValidateCustomerQualityRequirementAsync(item, plan);

            var existingItems = _itemRepository.GetByPlan(plan.Id);

            foreach (var candidate in existingItems)
            {
                if (candidate.Id == item.Id)
                {
                    continue;
                }

                if (candidate.SequenceNumber == item.SequenceNumber)
                {
                    throw new InvalidOperationException(
                        "A Quality Control Plan item with the same sequence number already exists.");
                }
            }

            _itemRepository.Update(item);

            _auditService.Log(
                "QCP Item Updated",
                "Quality.QCP",
                $"QCP item {item.SequenceNumber} ({item.Activity}) updated on QCP {plan.QcpNumber} Rev {plan.Revision}.");
        }

        public async Task DeleteItemAsync(Guid itemId)
        {
            await EnsurePermissionAsync(PermissionKeys.Quality.QCP);

            if (itemId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Quality Control Plan Item ID cannot be empty.",
                    nameof(itemId));
            }

            var item = _itemRepository.GetById(itemId);

            if (item == null)
            {
                throw new KeyNotFoundException(
                    "Quality Control Plan Item was not found.");
            }

            var plan = await EnsurePlanAccessAsync(
                item.ProjectQualityControlPlanId);

            EnsureDraftPlan(plan);

            _itemRepository.Delete(itemId);

            _auditService.Log(
                "QCP Item Deleted",
                "Quality.QCP",
                $"QCP item {item.SequenceNumber} ({item.Activity}) deleted from QCP {plan.QcpNumber} Rev {plan.Revision}.");
        }

        public async Task ApproveItemAsync(
            Guid itemId,
            QcpApprovalStatus status,
            string? remarks)
        {
            await EnsurePermissionAsync(PermissionKeys.Quality.QCP);

            if (itemId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Quality Control Plan Item ID cannot be empty.",
                    nameof(itemId));
            }

            var item = _itemRepository.GetById(itemId);

            if (item == null)
            {
                throw new KeyNotFoundException(
                    "Quality Control Plan Item was not found.");
            }

            var plan = await EnsurePlanAccessAsync(
                item.ProjectQualityControlPlanId);

            EnsureDraftPlan(plan);

            var cleanRemarks = remarks?.Trim() ?? string.Empty;

            if (status == QcpApprovalStatus.ApprovedWithComments &&
                string.IsNullOrWhiteSpace(cleanRemarks))
            {
                throw new InvalidOperationException(
                    "Remarks are mandatory for Approved with Comments.");
            }

            if (status == QcpApprovalStatus.ReviseAndResubmit &&
                string.IsNullOrWhiteSpace(cleanRemarks))
            {
                throw new InvalidOperationException(
                    "Remarks are mandatory for Revise & Resubmit.");
            }

            if (!_currentUser.IsAuthenticated ||
                string.IsNullOrWhiteSpace(_currentUser.Username))
            {
                throw new UnauthorizedAccessException(
                    "The approving user could not be established.");
            }

            var history = new ProjectQualityControlPlanItemApprovalHistory
            {
                Id = Guid.NewGuid(),
                ProjectQualityControlPlanItemId = item.Id,
                Status = status,
                Action = status switch
                {
                    QcpApprovalStatus.Approved => "Approved",
                    QcpApprovalStatus.ApprovedWithComments => "Approved with Comments",
                    QcpApprovalStatus.ReviseAndResubmit => "Revise & Resubmit",
                    _ => throw new ArgumentOutOfRangeException(nameof(status))
                },
                ApprovedBy = _currentUser.Username,
                ApprovedOn = DateTime.UtcNow,
                Remarks = cleanRemarks
            };

            _itemApprovalHistoryRepository.Add(history);

            _auditService.Log(
                "QCP Item Approval",
                "Quality.QCP",
                $"QCP item {item.SequenceNumber} ({item.Activity}) on QCP {plan.QcpNumber} Rev {plan.Revision} was marked {history.Action} by {history.ApprovedBy}. Remarks: {history.Remarks}");
        }

        public async Task<List<ProjectQualityControlPlanItemApprovalHistory>> GetItemApprovalHistoryAsync(
            Guid itemId)
        {
            await EnsurePermissionAsync(PermissionKeys.Quality.QCP);

            if (itemId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Quality Control Plan Item ID cannot be empty.",
                    nameof(itemId));
            }

            var item = _itemRepository.GetById(itemId);

            if (item == null)
            {
                throw new KeyNotFoundException(
                    "Quality Control Plan Item was not found.");
            }

            await EnsurePlanAccessAsync(item.ProjectQualityControlPlanId);

            return _itemApprovalHistoryRepository.GetByItem(item.Id);
        }


        private async Task<ProjectQualityControlPlan> EnsurePlanAccessAsync(
            Guid planId)
        {
            if (planId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Quality Control Plan ID cannot be empty.",
                    nameof(planId));
            }

            var plan = _repository.GetById(planId);

            if (plan == null)
            {
                throw new KeyNotFoundException(
                    "Quality Control Plan was not found.");
            }

            await EnsureProjectAccessAsync(plan.ProjectId);

            return plan;
        }

        private static void EnsureDraftPlan(
            ProjectQualityControlPlan plan)
        {
            if (plan.Status != ProjectQualityControlPlanStatus.Draft)
            {
                throw new InvalidOperationException(
                    "Quality Control Plan items can only be changed while the plan is Draft.");
            }
        }

        private static void ValidatePlan(
            ProjectQualityControlPlan plan)
        {
            if (plan.ProjectId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Project ID is required.",
                    nameof(plan));
            }

            if (string.IsNullOrWhiteSpace(plan.QcpNumber))
            {
                throw new ArgumentException(
                    "QCP number is required.",
                    nameof(plan));
            }

            if (string.IsNullOrWhiteSpace(plan.Revision))
            {
                throw new ArgumentException(
                    "Revision is required.",
                    nameof(plan));
            }

            if (string.IsNullOrWhiteSpace(plan.Title))
            {
                throw new ArgumentException(
                    "QCP title is required.",
                    nameof(plan));
            }
        }

        private async Task ValidateCustomerQualityRequirementAsync(
            ProjectQualityControlPlanItem item,
            ProjectQualityControlPlan plan)
        {
            if (!item.CustomerQualityRequirementId.HasValue)
            {
                return;
            }

            var requirement = _customerQualityRequirementRepository.GetById(
                item.CustomerQualityRequirementId.Value);

            if (requirement == null)
            {
                throw new KeyNotFoundException(
                    "Customer Quality Requirement was not found.");
            }

            if (requirement.ProjectId != plan.ProjectId)
            {
                throw new InvalidOperationException(
                    "The Customer Quality Requirement must belong to the same project as the Quality Control Plan.");
            }

            await Task.CompletedTask;
        }
        private static void ValidateItem(
            ProjectQualityControlPlanItem item)
        {
            if (item.ProjectQualityControlPlanId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Quality Control Plan ID is required.",
                    nameof(item));
            }

            if (item.SequenceNumber <= 0)
            {
                throw new ArgumentException(
                    "Sequence number must be greater than zero.",
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

            if (string.IsNullOrWhiteSpace(item.InspectionStage))
            {
                throw new ArgumentException(
                    "Inspection stage is required.",
                    nameof(item));
            }
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
    }
}










