using System.Collections.Generic;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Data.Repositories
{
    public interface IProjectRepository
    {
        List<Project> GetAll();
        void Create(Project project);
        void Update(Project project);
        void Delete(string projectNumber);
    }
}
