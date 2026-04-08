using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
    public class ProjectValidationService
    {
        private readonly IProjectRepository _projectRepo;

        public ProjectValidationService()
        {
            _projectRepo = new ProjectRepository();
        }

        public (bool IsValid, string Message) Validate(Guid projectId)
        {
            var project = _projectRepo.GetById(projectId);

            if (project == null)
                return (false, "Project not found");

            if (project.RequiresWps && project.SelectedWpsId == null)
            {
                return (false, "Project requires a WPS but none is assigned.");
            }

            return (true, "OK");
        }
    }
}