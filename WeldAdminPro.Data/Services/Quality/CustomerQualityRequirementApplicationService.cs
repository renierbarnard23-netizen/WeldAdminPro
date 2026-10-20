using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Core.Security;
using WeldAdminPro.Core.Security.Abstractions;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services.Quality
{
    public class CustomerQualityRequirementApplicationService
    {
        private readonly CustomerQualityRequirementRepository _repository;
        private readonly ProjectRepository _projectRepository;
        private readonly ICurrentUserContext _currentUser;
        private readonly IPermissionAuthorizationService _permissionAuthorization;
        private readonly IProjectAccessAuthorizationService _projectAccessAuthorization;

        public CustomerQualityRequirementApplicationService(
            CustomerQualityRequirementRepository repository,
            ProjectRepository projectRepository,
            ICurrentUserContext currentUser,
            IPermissionAuthorizationService permissionAuthorization,
            IProjectAccessAuthorizationService projectAccessAuthorization)
        {
            _repository = repository;
            _projectRepository = projectRepository;
            _currentUser = currentUser;
            _permissionAuthorization = permissionAuthorization;
            _projectAccessAuthorization = projectAccessAuthorization;
        }

        public async Task<List<CustomerQualityRequirement>> GetByProjectAsync(
            Guid projectId)
        {
            await EnsurePermissionAsync(
                PermissionKeys.Quality.CustomerRequirements);

            var project = await EnsureProjectAccessAsync(projectId);

            return _repository.GetByProject(project.Id);
        }

        public async Task<CustomerQualityRequirement?> GetByIdAsync(
            Guid id)
        {
            await EnsurePermissionAsync(
                PermissionKeys.Quality.CustomerRequirements);

            if (id == Guid.Empty)
            {
                throw new ArgumentException(
                    "Customer quality requirement ID cannot be empty.",
                    nameof(id));
            }

            var requirement = _repository.GetById(id);

            if (requirement == null)
            {
                return null;
            }

            await EnsureProjectAccessAsync(requirement.ProjectId);

            return requirement;
        }

        public async Task AddAsync(
            CustomerQualityRequirement requirement)
        {
            if (requirement == null)
            {
                throw new ArgumentNullException(nameof(requirement));
            }

            await EnsurePermissionAsync(
                PermissionKeys.Quality.CustomerRequirements);

            if (requirement.Id == Guid.Empty)
            {
                requirement.Id = Guid.NewGuid();
            }

            await EnsureProjectAccessAsync(requirement.ProjectId);

            ValidateRequiredFields(requirement);


            requirement.CreatedOn = requirement.CreatedOn == default
                ? DateTime.Now
                : requirement.CreatedOn;

            if (string.IsNullOrWhiteSpace(requirement.CreatedBy))
            {
                requirement.CreatedBy = _currentUser.Username ?? string.Empty;
            }

            _repository.Add(requirement);
        }

        public async Task UpdateAsync(
            CustomerQualityRequirement requirement)
        {
            if (requirement == null)
            {
                throw new ArgumentNullException(nameof(requirement));
            }

            await EnsurePermissionAsync(
                PermissionKeys.Quality.CustomerRequirements);

            if (requirement.Id == Guid.Empty)
            {
                throw new ArgumentException(
                    "Customer quality requirement ID cannot be empty.",
                    nameof(requirement));
            }

            var existing = _repository.GetById(requirement.Id);

            if (existing == null)
            {
                throw new KeyNotFoundException(
                    "Customer quality requirement was not found.");
            }

            await EnsureProjectAccessAsync(existing.ProjectId);

            if (requirement.ProjectId != existing.ProjectId)
            {
                throw new InvalidOperationException(
                    "A customer quality requirement cannot be moved to another project.");
            }

            if (existing.Status != CustomerQualityRequirementStatus.Draft)
            {
                throw new InvalidOperationException(
                    "Only draft customer quality requirements can be edited in place. Create a new revision for a controlled requirement change.");
            }

            ValidateRequiredFields(requirement);


            _repository.Update(requirement);
        }

        public async Task<CustomerQualityRequirement> CreateRevisionAsync(
            Guid id,
            string revision)
        {
            await EnsurePermissionAsync(
                PermissionKeys.Quality.CustomerRequirements);

            if (id == Guid.Empty)
            {
                throw new ArgumentException(
                    "Customer quality requirement ID cannot be empty.",
                    nameof(id));
            }

            if (string.IsNullOrWhiteSpace(revision))
            {
                throw new ArgumentException(
                    "New customer quality requirement revision is required.",
                    nameof(revision));
            }

            var existing = _repository.GetById(id);

            if (existing == null)
            {
                throw new KeyNotFoundException(
                    "Customer quality requirement was not found.");
            }

            await EnsureProjectAccessAsync(existing.ProjectId);

            if (existing.Status != CustomerQualityRequirementStatus.Active)
            {
                throw new InvalidOperationException(
                    "Only active customer quality requirements can be revised.");
            }

            if (!existing.IsActive)
            {
                throw new InvalidOperationException(
                    "The customer quality requirement is not the active revision.");
            }

            if (string.Equals(
                    existing.Revision?.Trim(),
                    revision.Trim(),
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "The new customer quality requirement revision must differ from the current revision.");
            }

            var newRevision = new CustomerQualityRequirement
            {
                Id = Guid.NewGuid(),
                ProjectId = existing.ProjectId,
                RequirementNumber = existing.RequirementNumber,
                Category = existing.Category,
                Description = existing.Description,
                SourceDocument = existing.SourceDocument,
                SourceClause = existing.SourceClause,
                Revision = revision.Trim(),
                Mandatory = existing.Mandatory,
                AcceptanceCriteria = existing.AcceptanceCriteria,
                VerificationMethod = existing.VerificationMethod,
                ResponsibleParty = existing.ResponsibleParty,
                RequiredEvidence = existing.RequiredEvidence,
                Status = CustomerQualityRequirementStatus.Draft,
                CreatedOn = DateTime.Now,
                CreatedBy = _currentUser.Username ?? string.Empty,
                ApprovedOn = null,
                ApprovedBy = string.Empty,
                EffectiveDate = null,
                SupersededOn = null,
                PreviousRevisionId = existing.Id,
                IsActive = true
            };

            _repository.SaveRevision(
                existing,
                newRevision);

            return newRevision;
        }
        public async Task ApproveAsync(Guid id)
        {
            await EnsurePermissionAsync(
                PermissionKeys.Quality.CustomerRequirements);

            if (id == Guid.Empty)
            {
                throw new ArgumentException(
                    "Customer quality requirement ID cannot be empty.",
                    nameof(id));
            }

            var existing = _repository.GetById(id);

            if (existing == null)
            {
                throw new KeyNotFoundException(
                    "Customer quality requirement was not found.");
            }

            await EnsureProjectAccessAsync(existing.ProjectId);

            if (existing.Status != CustomerQualityRequirementStatus.Draft)
            {
                throw new InvalidOperationException(
                    "Only draft customer quality requirements can be approved.");
            }

            if (!existing.IsActive)
            {
                throw new InvalidOperationException(
                    "Only active customer quality requirement revisions can be approved.");
            }

            ValidateRequiredFields(existing);

            if (!_currentUser.IsAuthenticated ||
                string.IsNullOrWhiteSpace(_currentUser.Username))
            {
                throw new UnauthorizedAccessException(
                    "Authenticated user identity could not be established.");
            }

            var now = DateTime.UtcNow;

            existing.Status = CustomerQualityRequirementStatus.Active;
            existing.ApprovedBy = _currentUser.Username;
            existing.ApprovedOn = now;
            existing.EffectiveDate = now;

            _repository.Update(existing);
        }
        public async Task DeleteAsync(Guid id)
        {
            await EnsurePermissionAsync(
                PermissionKeys.Quality.CustomerRequirements);

            if (id == Guid.Empty)
            {
                throw new ArgumentException(
                    "Customer quality requirement ID cannot be empty.",
                    nameof(id));
            }

            var existing = _repository.GetById(id);

            if (existing == null)
            {
                throw new KeyNotFoundException(
                    "Customer quality requirement was not found.");
            }

            await EnsureProjectAccessAsync(existing.ProjectId);

            if (existing.Status != CustomerQualityRequirementStatus.Draft)
            {
                throw new InvalidOperationException(
                    "Only draft customer quality requirements can be deleted.");
            }

            _repository.Delete(id);
        }

        private static void ValidateRequiredFields(
            CustomerQualityRequirement requirement)
        {
            if (string.IsNullOrWhiteSpace(requirement.RequirementNumber))
            {
                throw new ArgumentException(
                    "Requirement number is required.",
                    nameof(requirement));
            }

            if (string.IsNullOrWhiteSpace(requirement.Description))
            {
                throw new ArgumentException(
                    "Requirement description is required.",
                    nameof(requirement));
            }

            if (string.IsNullOrWhiteSpace(requirement.Category))
            {
                throw new ArgumentException(
                    "Requirement category is required.",
                    nameof(requirement));
            }

            if (string.IsNullOrWhiteSpace(requirement.AcceptanceCriteria))
            {
                throw new ArgumentException(
                    "Acceptance criteria is required.",
                    nameof(requirement));
            }

            if (string.IsNullOrWhiteSpace(requirement.VerificationMethod))
            {
                throw new ArgumentException(
                    "Verification method is required.",
                    nameof(requirement));
            }

            if (string.IsNullOrWhiteSpace(requirement.ResponsibleParty))
            {
                throw new ArgumentException(
                    "Responsible party is required.",
                    nameof(requirement));
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





