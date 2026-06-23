using System;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Core.Services
{
    public class ProjectContextService : IProjectContextService
    {
        private Project? _currentProject;

    public Project? CurrentProject => _currentProject;

        public event Action<Project?>? ProjectChanged;

        public void SetCurrentProject(Project? project)
        {
            _currentProject = project;

            ProjectChanged?.Invoke(_currentProject);
        }

        public void ClearProject()
        {
            _currentProject = null;

            ProjectChanged?.Invoke(null);
        }
    }

}
