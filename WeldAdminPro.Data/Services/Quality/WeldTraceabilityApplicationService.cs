using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Core.Quality.Services;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services.Quality
{
    public class WeldTraceabilityApplicationService
    {
        private readonly WeldRepository _weldRepository;
        private readonly WeldNdtRepository _ndtRepository;

        public WeldTraceabilityApplicationService()
        {
            _weldRepository = new WeldRepository();

            _ndtRepository = 
                new WeldNdtRepository();
        }

        public async Task<List<WeldTraceabilityRow>> GetProjectRows(Guid projectId)
        {
            List<Weld> welds =
                await _weldRepository.GetByProjectAsync(projectId);

            List<WeldNdtResult> ndtResults =
                _ndtRepository.GetAll();

            var service =
                new WeldTraceabilityService();

            return service.Build(
                welds,
                ndtResults);
        }
    }
}