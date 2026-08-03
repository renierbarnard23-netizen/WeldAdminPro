using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Core.Security;
using WeldAdminPro.Core.Security.Abstractions;

namespace WeldAdminPro.Core.Quality.Services
{
    public class HoldPointWorkflowService
    {
        private readonly ICurrentUserContext _currentUser;
        private readonly IPermissionAuthorizationService _permissionAuthorization;

        public HoldPointWorkflowService(
            ICurrentUserContext currentUser,
            IPermissionAuthorizationService permissionAuthorization)
        {
            _currentUser = currentUser;
            _permissionAuthorization = permissionAuthorization;
        }

        public bool CanAdvanceWorkflow(
            IEnumerable<WeldHoldPoint> holdPoints,
            out List<string> blockingReasons)
        {
            blockingReasons =
                holdPoints
                    .Where(x =>
                        x.IsMandatory &&
                        x.Category == HoldPointCategory.Hold &&
                        x.Status != HoldPointStatus.Approved)
                    .Select(x =>
                        $"{x.HoldPointType} not approved")
                    .ToList();

            return !blockingReasons.Any();
        }

        public async Task ApproveAsync(
            WeldHoldPoint holdPoint,
            string approvedBy,
            string comments)
        {
            await EnsureApprovalPermissionAsync();

            holdPoint.Status =
                HoldPointStatus.Approved;

            holdPoint.ApprovedBy =
                approvedBy;

            holdPoint.ApprovedDate =
                DateTime.UtcNow;

            holdPoint.Comments =
                comments;
        }

        public async Task RejectAsync(
            WeldHoldPoint holdPoint,
            string comments)
        {
            await EnsureApprovalPermissionAsync();

            holdPoint.Status =
                HoldPointStatus.Rejected;

            holdPoint.Comments =
                comments;
        }

        private async Task EnsureApprovalPermissionAsync()
        {
            if (!_currentUser.IsAuthenticated)
            {
                throw new UnauthorizedAccessException(
                    "User is not authenticated.");
            }

            if (string.IsNullOrWhiteSpace(
                _currentUser.Role))
            {
                throw new UnauthorizedAccessException(
                    "User does not have an assigned role.");
            }

            var hasPermission =
                await _permissionAuthorization
                    .HasPermissionAsync(
                        _currentUser.UserId,
                        _currentUser.Role,
                        PermissionKeys.Quality.HoldPointApproval);

            if (!hasPermission)
            {
                throw new UnauthorizedAccessException(
                    "User does not have permission to approve or reject quality hold points.");
            }
        }
    }
}