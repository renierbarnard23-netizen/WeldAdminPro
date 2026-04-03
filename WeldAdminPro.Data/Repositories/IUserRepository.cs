using System.Threading.Tasks;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Data.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);
    }
}
