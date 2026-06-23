using System;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Core.Services
{
    public interface IProjectContextService
    {
        Project? CurrentProject { get; }

    event Action<Project?>? ProjectChanged;

        void SetCurrentProject(Project? project);

        void ClearProject();
    }

}
