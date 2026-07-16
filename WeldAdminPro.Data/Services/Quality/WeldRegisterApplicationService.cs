using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services.Quality
{
    public class WeldRegisterApplicationService
    {
        private readonly WeldRepository _weldRepository;
        private readonly WeldHistoryRepository _historyRepository;
        private readonly WeldNdtRepository _ndtRepository;

        public WeldRegisterApplicationService()
        {
            _weldRepository = new WeldRepository();

            var connectionString =
                $"Data Source={DatabasePath.Get()}";

            _historyRepository =
                new WeldHistoryRepository(connectionString);

            _ndtRepository =
                new WeldNdtRepository(connectionString);
        }

        public async Task<List<Weld>> GetProjectWelds(Guid projectId)
        {
            return await _weldRepository.GetByProjectAsync(projectId);
        }

        public List<WeldHistoryEntry> GetHistory(Guid weldId)
        {
            return _historyRepository.GetByWeld(weldId);
        }

        public List<WeldNdtResult> GetNdt(Guid weldId)
        {
            return _ndtRepository.GetByWeld(weldId);
        }

        public List<Weld> GetAllWelds()
        {
            return _weldRepository.GetAll();
        }
    }
}