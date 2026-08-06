using WeldAdminPro.Core.Interfaces;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Core.Services
{
    public class WeldService : IWeldService
    {
        private readonly IWeldRepository _repository;

        public WeldService(
            IWeldRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Weld>> GetByProjectAsync(
            Guid projectId)
        {
            return await _repository
                .GetByProjectAsync(projectId);
        }

        public async Task AddAsync(Weld weld)
        {
            if (weld.Id == Guid.Empty)
            {
                weld.Id = Guid.NewGuid();
            }

            await _repository.AddAsync(weld);
        }

        public async Task<string> GetNextWeldNumberAsync(
            Guid projectId)
        {
            return await _repository
                .GetNextWeldNumberAsync(projectId);
        }

        public async Task UpdateAsync(Weld weld)
        {
            await _repository.UpdateAsync(weld);
        }
    }
}