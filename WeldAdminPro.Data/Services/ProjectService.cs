using Microsoft.EntityFrameworkCore;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Data.Services
{
    public class ProjectService
    {
        private readonly WeldAdminDbContext _db;
        public ProjectService(WeldAdminDbContext db) { _db = db; }

        public async Task<Project> CreateProjectAsync(Project p)
        {
            _db.Projects.Add(p);
            await _db.SaveChangesAsync();
            return p;
        }

        public Task<List<Project>> ListProjectsAsync() =>
            _db.Projects.Include(x => x.Documents).ToListAsync();
    }
}
