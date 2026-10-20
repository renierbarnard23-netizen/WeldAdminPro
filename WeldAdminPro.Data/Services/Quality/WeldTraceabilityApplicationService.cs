using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Core.Quality.Services;
using WeldAdminPro.Core.Security.Abstractions;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services.Quality
{
    public class WeldTraceabilityApplicationService
    {
        private readonly ICurrentUserContext _currentUser;
        private readonly IProjectAccessAuthorizationService _projectAccessAuthorization;
        private readonly ProjectRepository _projectRepository;

        private readonly WeldRepository _weldRepository;
        private readonly WeldNdtRepository _ndtRepository;
        private readonly WeldTraceabilityRepository _traceabilityRepository;

        public WeldTraceabilityApplicationService(
            ICurrentUserContext currentUser,
            IProjectAccessAuthorizationService projectAccessAuthorization,
            ProjectRepository projectRepository)
        {
            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));
            _projectAccessAuthorization = projectAccessAuthorization
                ?? throw new ArgumentNullException(nameof(projectAccessAuthorization));
            _projectRepository = projectRepository
                ?? throw new ArgumentNullException(nameof(projectRepository));

            _weldRepository = new WeldRepository();

            _ndtRepository =
                new WeldNdtRepository();

            _traceabilityRepository =
                new WeldTraceabilityRepository();
        }

        public async Task<List<WeldTraceabilityRow>> GetProjectRows(Guid projectId)
        {
            if (!_currentUser.IsAuthenticated)
                throw new UnauthorizedAccessException(
                    "Authentication is required.");

            var userId = _currentUser.UserId;

            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedAccessException(
                    "A valid user identity is required.");

            if (projectId == Guid.Empty)
                throw new ArgumentException(
                    "A valid project ID is required.", nameof(projectId));

            var project = _projectRepository.GetById(projectId);

            if (project == null)
                throw new KeyNotFoundException("Project was not found.");

            var hasAccess =
                await _projectAccessAuthorization
                    .CanAccessProjectAsync(userId, project);

            if (!hasAccess)
                throw new UnauthorizedAccessException(
                    "You do not have access to this project.");

            List<Weld> welds =
                await _weldRepository.GetByProjectAsync(projectId);

            var ndtResults = new List<WeldNdtResult>();

            foreach (var weld in welds)
            {
                ndtResults.AddRange(
                    _ndtRepository.GetByWeld(weld.Id));
            }

            var service =
                new WeldTraceabilityService();

            return service.Build(
                welds,
                ndtResults);
        }

        private async Task EnsureWeldProjectAccessAsync(Guid weldId)
        {
            if (!_currentUser.IsAuthenticated)
                throw new UnauthorizedAccessException(
                    "Authentication is required.");

            var userId = _currentUser.UserId;

            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedAccessException(
                    "A valid user identity is required.");

            if (weldId == Guid.Empty)
                throw new ArgumentException(
                    "A valid weld ID is required.",
                    nameof(weldId));

            var weld = await _weldRepository.GetByIdAsync(weldId);

            if (weld == null)
                throw new KeyNotFoundException(
                    "Weld was not found.");

            if (weld.ProjectId == Guid.Empty)
                throw new UnauthorizedAccessException(
                    "The weld is not associated with a valid project.");

            var project = _projectRepository.GetById(weld.ProjectId);

            if (project == null)
                throw new KeyNotFoundException(
                    "The weld project was not found.");

            var hasAccess =
                await _projectAccessAuthorization
                    .CanAccessProjectAsync(userId, project);

            if (!hasAccess)
                throw new UnauthorizedAccessException(
                    "You do not have access to this weld project.");
        }
        public async Task<List<WeldTraceabilityRecord>> GetByWeld(Guid weldId)
        {
            await EnsureWeldProjectAccessAsync(weldId);

            return _traceabilityRepository.GetByWeld(weldId);
        }

        public async Task AddTraceability(WeldTraceabilityRecord record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            await EnsureWeldProjectAccessAsync(record.WeldId);

            _traceabilityRepository.Add(record);
        }

        public async Task UpdateTraceability(WeldTraceabilityRecord record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            await EnsureWeldProjectAccessAsync(record.WeldId);

            _traceabilityRepository.Update(record);
        }
    }
}

