using System;
using System.Threading.Tasks;
using WeldAdminPro.Core.Interfaces;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Security;
using WeldAdminPro.Core.Security.Abstractions;

namespace WeldAdminPro.Web.Services.Quality
{
    /// <summary>
    /// Controls WPQR approval independently of ordinary record editing.
    /// </summary>
    public sealed class WpqrWorkflowService
    {
        private readonly IWpqrRepository _repository;
        private readonly ICurrentUserContext _currentUser;
        private readonly IPermissionAuthorizationService _permissionAuthorization;
        private readonly IProjectAccessAuthorizationService _projectAccessAuthorization;
        private readonly WpsApplicationService _wpsService;
        private readonly WelderQualificationApplicationService _welderService;

        public WpqrWorkflowService(
            IWpqrRepository repository,
            ICurrentUserContext currentUser,
            IPermissionAuthorizationService permissionAuthorization,
            IProjectAccessAuthorizationService projectAccessAuthorization,
            WpsApplicationService wpsService,
            WelderQualificationApplicationService welderService)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _permissionAuthorization = permissionAuthorization
                ?? throw new ArgumentNullException(nameof(permissionAuthorization));
            _projectAccessAuthorization = projectAccessAuthorization
                ?? throw new ArgumentNullException(nameof(projectAccessAuthorization));
            _wpsService = wpsService
                ?? throw new ArgumentNullException(nameof(wpsService));
            _welderService = welderService
                ?? throw new ArgumentNullException(nameof(welderService));
        }

        public async Task ApproveAsync(Guid wpqrId)
        {
            if (wpqrId == Guid.Empty)
                throw new ArgumentException("A valid WPQR ID is required.", nameof(wpqrId));

            if (!_currentUser.IsAuthenticated ||
                string.IsNullOrWhiteSpace(_currentUser.UserId))
            {
                throw new UnauthorizedAccessException(
                    "You must be authenticated to approve a WPQR.");
            }

            if (string.IsNullOrWhiteSpace(_currentUser.Role))
            {
                throw new UnauthorizedAccessException(
                    "Your account does not have an assigned role.");
            }

            if (string.IsNullOrWhiteSpace(_currentUser.Username))
            {
                throw new UnauthorizedAccessException(
                    "Your account does not have a valid username.");
            }

            var hasPermission = await _permissionAuthorization.HasPermissionAsync(
                _currentUser.UserId,
                _currentUser.Role,
                PermissionKeys.Quality.WpqrApproval);

            if (!hasPermission)
            {
                throw new UnauthorizedAccessException(
                    "You do not have permission to approve WPQR records.");
            }

            // Always reload authoritative state; never approve client-supplied fields.
            var wpqr = _repository.GetById(wpqrId)
                ?? throw new InvalidOperationException("The WPQR record was not found.");

            if (!wpqr.CompanyId.HasValue || wpqr.CompanyId.Value == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "The WPQR does not have a valid owning company.");
            }

            var canAccessCompany =
                await _projectAccessAuthorization.CanAccessCompanyAsync(
                    _currentUser.UserId,
                    wpqr.CompanyId.Value);

            if (!canAccessCompany)
            {
                throw new UnauthorizedAccessException(
                    "You do not have access to the company that owns this WPQR.");
            }

            if (!wpqr.IsActive)
                throw new InvalidOperationException("An inactive WPQR cannot be approved.");

            if (wpqr.IsApproved || wpqr.IsLocked)
            {
                throw new InvalidOperationException(
                    "The WPQR is already approved or locked. Create a revision to change it.");
            }

            // A WPQR cannot be approved without both required qualification links.
            if (!wpqr.WpsId.HasValue || wpqr.WpsId.Value == Guid.Empty)
                throw new InvalidOperationException(
                    "A WPS must be linked before a WPQR can be approved.");

            if (!wpqr.WelderQualificationId.HasValue || wpqr.WelderQualificationId.Value <= 0)
                throw new InvalidOperationException(
                    "A welder qualification must be linked before a WPQR can be approved.");

            // Revalidate linked qualification records at approval time.
            WeldAdminPro.Core.Quality.Wps? linkedWps = null;

            if (wpqr.WpsId.HasValue)
            {
                var accessibleWpsRecords = await _wpsService.GetAllAsync();
                linkedWps = accessibleWpsRecords.FirstOrDefault(
                    x => x.Id == wpqr.WpsId.Value);

                if (linkedWps == null)
                    throw new InvalidOperationException(
                        "The linked WPS could not be found or is not accessible.");

                if (!linkedWps.CompanyId.HasValue ||
                    linkedWps.CompanyId.Value != wpqr.CompanyId.Value)
                    throw new InvalidOperationException(
                        "The linked WPS must belong to the same company as the WPQR.");

                if (!linkedWps.IsApproved || !linkedWps.IsActive)
                    throw new InvalidOperationException(
                        "The linked WPS must be approved and active before WPQR approval.");
            }

            if (wpqr.WelderQualificationId.HasValue)
            {
                var accessibleQualifications =
                    await _welderService.GetAccessibleAsync();

                var linkedQualification = accessibleQualifications.FirstOrDefault(
                    x => x.Id == wpqr.WelderQualificationId.Value);

                if (linkedQualification == null)
                    throw new InvalidOperationException(
                        "The linked welder qualification could not be found or is not accessible.");

                if (!linkedQualification.CompanyId.HasValue ||
                    linkedQualification.CompanyId.Value != wpqr.CompanyId.Value)
                    throw new InvalidOperationException(
                        "The linked welder qualification must belong to the same company as the WPQR.");

                if (!linkedQualification.IsActive)
                    throw new InvalidOperationException(
                        "The linked welder qualification must be active before WPQR approval.");

                if (linkedWps == null)
                    throw new InvalidOperationException(
                        "The linked WPS must be loaded before material compatibility can be checked.");

                var wpsMaterialGroup =
                    WeldAdminPro.Core.Quality.Normalization.ComplianceNormalizer
                        .NormalizeMaterialGroup(linkedWps.MaterialGroup);

                var welderMaterialGroup =
                    WeldAdminPro.Core.Quality.Normalization.ComplianceNormalizer
                        .NormalizeMaterialGroup(linkedQualification.MaterialGroup);

                if (string.IsNullOrWhiteSpace(wpsMaterialGroup) ||
                    string.IsNullOrWhiteSpace(welderMaterialGroup) ||
                    !string.Equals(
                        wpsMaterialGroup,
                        welderMaterialGroup,
                        StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "The WPQR cannot be approved because the welder qualification's material group is missing or does not match the linked WPS.");
                }
            }

            var approvedOn = DateTime.UtcNow;

            var approved = _repository.TryApprove(
                wpqr.Id,
                _currentUser.Username,
                approvedOn,
                _currentUser.Role);

            if (!approved)
            {
                throw new InvalidOperationException(
                    "The WPQR could not be approved. It may have been changed, approved, locked, or deactivated by another operation. Reload the record and try again.");
            }
        }
    }
}


