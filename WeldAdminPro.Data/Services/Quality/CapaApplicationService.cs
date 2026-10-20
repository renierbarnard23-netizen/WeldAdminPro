using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Core.Security;
using WeldAdminPro.Core.Security.Abstractions;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services.Quality
{
    public class CapaApplicationService
    {
        private readonly CapaRepository _repository;
        private readonly NcrRepository _ncrRepository;
        private readonly WeldRepository _weldRepository;
        private readonly ProjectRepository _projectRepository;
        private readonly ICurrentUserContext _currentUser;
        private readonly IPermissionAuthorizationService _permissionAuthorization;
        private readonly IProjectAccessAuthorizationService _projectAccessAuthorization;

        public CapaApplicationService(
            ICurrentUserContext currentUser,
            IPermissionAuthorizationService permissionAuthorization,
            IProjectAccessAuthorizationService projectAccessAuthorization)
        {
            _repository =
                new CapaRepository(
                    DatabasePath.GetConnectionString());

            _ncrRepository =
                new NcrRepository(
                    DatabasePath.GetConnectionString());

            _weldRepository =
                new WeldRepository(
                    DatabasePath.GetConnectionString());

            _projectRepository =
                new ProjectRepository();

            _currentUser = currentUser;
            _permissionAuthorization = permissionAuthorization;
            _projectAccessAuthorization = projectAccessAuthorization;
        }

        public async Task<List<CapaRecord>> GetAll()
        {
            await EnsurePermissionAsync();

            var capas = _repository.GetAll();
            var accessible = new List<CapaRecord>();

            foreach (var capa in capas)
            {
                if (await CanAccessCapaAsync(capa))
                {
                    accessible.Add(capa);
                }
            }

            return accessible;
        }

        public void Add(CapaRecord record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            _repository.Add(record);
        }

        public void Update(CapaRecord record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            _repository.Update(record);
        }

        public async Task Verify(
            CapaRecord record,
            string verifiedBy)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            await EnsurePermissionAsync();

            if (!await CanAccessCapaAsync(record))
                throw new UnauthorizedAccessException(
                    "You do not have access to this CAPA project.");

            record.Status =
                CapaStatus.Verified;

            record.IsEffective = true;

            record.VerifiedBy =
                verifiedBy;

            record.VerifiedDate =
                DateTime.UtcNow;

            _repository.Update(record);
        }

        private async Task EnsurePermissionAsync()
        {
            if (!_currentUser.IsAuthenticated)
                throw new UnauthorizedAccessException(
                    "Authentication is required.");

            if (string.IsNullOrWhiteSpace(_currentUser.UserId))
                throw new UnauthorizedAccessException(
                    "Authenticated user identity is required.");

            var hasPermission =
                await _permissionAuthorization.HasPermissionAsync(
                    _currentUser.UserId,
                    _currentUser.Role,
                    PermissionKeys.Quality.NCR);

            if (!hasPermission)
                throw new UnauthorizedAccessException(
                    "You do not have permission to access CAPA records.");
        }

        private async Task<bool> CanAccessCapaAsync(
            CapaRecord capa)
        {
            if (capa == null ||
                capa.NcrId == Guid.Empty)
            {
                return false;
            }

            var ncr =
                _ncrRepository.GetById(capa.NcrId);

            if (ncr == null ||
                !ncr.WeldId.HasValue ||
                ncr.WeldId.Value == Guid.Empty)
            {
                return false;
            }

            var weld =
                await _weldRepository.GetByIdAsync(
                    ncr.WeldId.Value);

            if (weld == null ||
                weld.ProjectId == Guid.Empty)
            {
                return false;
            }

            var project =
                _projectRepository.GetById(
                    weld.ProjectId);

            if (project == null)
                return false;

            return await _projectAccessAuthorization
                .CanAccessProjectAsync(
                    _currentUser.UserId,
                    project);
        }
    }
}