using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Core.Interfaces
{
    public interface IWeldRepository
    {
        Task<List<Weld>> GetByProjectAsync(
            Guid projectId);

        Task AddAsync(Weld weld);

        Task<string> GetNextWeldNumberAsync(
            Guid projectId);

        Task UpdateAsync(Weld weld);
        Task DeleteAsync(Guid weldId);
    }
}