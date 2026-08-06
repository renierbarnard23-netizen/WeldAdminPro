using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Core.Interfaces
{
    public interface IWeldService
    {
        Task<List<Weld>> GetByProjectAsync(Guid projectId);

        Task AddAsync(Weld weld);

        Task<string> GetNextWeldNumberAsync(Guid projectId);
        Task UpdateAsync(Weld weld);
    }
}