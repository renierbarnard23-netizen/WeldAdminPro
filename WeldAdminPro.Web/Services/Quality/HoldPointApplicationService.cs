using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Core.Quality.Services;
using WeldAdminPro.Core.Security;
using WeldAdminPro.Core.Security.Abstractions;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Web.Services.Quality
{
    public class HoldPointApplicationService
    {
        private readonly HoldPointRepository _repository;
        private readonly WeldRepository _weldRepository;
        private readonly ProjectRepository _projectRepository;
        private readonly ICurrentUserContext _currentUser;
        private readonly IPermissionAuthorizationService _permissionAuthorization;
        private readonly IProjectAccessAuthorizationService _projectAccessAuthorization;
        private readonly HoldPointWorkflowService _workflowService;

        public HoldPointApplicationService(
            HoldPointRepository repository,
            WeldRepository weldRepository,
            ProjectRepository projectRepository,
            ICurrentUserContext currentUser,
            IPermissionAuthorizationService permissionAuthorization,
            IProjectAccessAuthorizationService projectAccessAuthorization,
            HoldPointWorkflowService workflowService)
        {
            _repository = repository;
            _weldRepository = weldRepository;
            _projectRepository = projectRepository;
            _currentUser = currentUser;
            _permissionAuthorization = permissionAuthorization;
            _projectAccessAuthorization = projectAccessAuthorization;
            _workflowService = workflowService;
        }

        public async Task<List<WeldHoldPoint>> GetByWeldAsync(Guid weldId)
        {
            await EnsureWeldProjectAccessAsync(
                weldId,
                PermissionKeys.Quality.View);

            return _repository.GetByWeld(weldId);
        }

        public async Task<WeldHoldPoint?> GetByIdAsync(Guid holdPointId)
        {
            var holdPoint = _repository.GetById(holdPointId);

            if (holdPoint == null)
                return null;

            await EnsureWeldProjectAccessAsync(
                holdPoint.WeldId,
                PermissionKeys.Quality.View);

            return holdPoint;
        }

        public async Task<WeldHoldPoint> CreateAsync(
            Guid weldId,
            HoldPointType holdPointType,
            HoldPointCategory category,
            HoldPointApproverRole requiredApproverRole,
            bool isMandatory,
            string comments)
        {
            await EnsureWeldProjectAccessAsync(
                weldId,
                PermissionKeys.Quality.ITP);

            var holdPoint = new WeldHoldPoint
            {
                Id = Guid.NewGuid(),
                WeldId = weldId,
                HoldPointType = holdPointType,
                Category = category,
                RequiredApproverRole = requiredApproverRole,
                Status = HoldPointStatus.Pending,
                IsMandatory = isMandatory,
                ApprovedBy = string.Empty,
                ApprovedDate = null,
                Comments = comments ?? string.Empty
            };

            _repository.Add(holdPoint);

            return holdPoint;
        }

        public async Task ApproveAsync(
            Guid holdPointId,
            string approvedBy,
            string comments)
        {
            var holdPoint =
                _repository.GetById(holdPointId);

            if (holdPoint == null)
            {
                throw new KeyNotFoundException(
                    "The requested hold point could not be found.");
            }

            await EnsureWeldProjectAccessAsync(
                holdPoint.WeldId,
                PermissionKeys.Quality.HoldPointApproval);

            await _workflowService.ApproveAsync(
                holdPoint,
                approvedBy,
                comments);

            _repository.Update(holdPoint);
        }

        public async Task RejectAsync(
            Guid holdPointId,
            string comments)
        {
            var holdPoint =
                _repository.GetById(holdPointId);

            if (holdPoint == null)
            {
                throw new KeyNotFoundException(
                    "The requested hold point could not be found.");
            }

            await EnsureWeldProjectAccessAsync(
                holdPoint.WeldId,
                PermissionKeys.Quality.HoldPointApproval);

            await _workflowService.RejectAsync(
                holdPoint,
                comments);

            _repository.Update(holdPoint);
        }

        private async Task EnsureWeldProjectAccessAsync(
            Guid weldId,
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

            var weld =
                await _weldRepository.GetByIdAsync(weldId);

            if (weld == null)
            {
                throw new KeyNotFoundException(
                    "The requested weld could not be found.");
            }

            var project =
                _projectRepository.GetById(weld.ProjectId);

            if (project == null)
            {
                throw new KeyNotFoundException(
                    "The weld's project could not be found.");
            }

            var hasProjectAccess =
                await _projectAccessAuthorization
                    .CanAccessProjectAsync(
                        _currentUser.UserId,
                        project);

            if (!hasProjectAccess)
            {
                throw new UnauthorizedAccessException(
                    "The requested weld is not accessible.");
            }
        }
    }
}
