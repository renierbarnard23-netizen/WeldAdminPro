using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WeldAdminPro.Core.Analytics.Executive;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality;
using WeldAdminPro.Core.Security.Abstractions;
using WeldAdminPro.Data.Services;
using WeldAdminPro.Data.Services.Projects;

namespace WeldAdminPro.Web.Services
{
    public class ProjectComplianceApplicationService
    {
        private readonly ProjectApplicationService _projectService;
        private readonly ICurrentUserContext _currentUser;
        private readonly IProjectAccessAuthorizationService _projectAccessAuthorization;
        private readonly ProjectComplianceService _compliance;
        private readonly UnifiedRiskService _risk;

        public ProjectComplianceApplicationService(
            ProjectApplicationService projectService,
            ICurrentUserContext currentUser,
            IProjectAccessAuthorizationService projectAccessAuthorization,
            ProjectComplianceService compliance,
            UnifiedRiskService risk)
        {
            _projectService =
                projectService ??
                throw new ArgumentNullException(nameof(projectService));

            _currentUser =
                currentUser ??
                throw new ArgumentNullException(nameof(currentUser));

            _projectAccessAuthorization =
                projectAccessAuthorization ??
                throw new ArgumentNullException(nameof(projectAccessAuthorization));

            _compliance =
                compliance ??
                throw new ArgumentNullException(nameof(compliance));

            _risk =
                risk ??
                throw new ArgumentNullException(nameof(risk));
        }

        public async Task<List<ProjectComplianceDisplay>> GetAccessibleProjectsAsync()
        {
            if (!_currentUser.IsAuthenticated)
                throw new UnauthorizedAccessException(
                    "Authentication is required.");

            if (string.IsNullOrWhiteSpace(_currentUser.UserId))
                throw new UnauthorizedAccessException(
                    "A valid user identity is required.");

            var projects = await _projectService.GetProjectsAsync();

            var results = new List<ProjectComplianceDisplay>();

            foreach (var project in projects)
            {
                if (project == null ||
                    project.Id == Guid.Empty ||
                    !project.CompanyId.HasValue ||
                    project.CompanyId.Value == Guid.Empty)
                {
                    continue;
                }

                var hasProjectAccess =
                    await _projectAccessAuthorization.CanAccessProjectAsync(
                        _currentUser.UserId,
                        project);

                if (!hasProjectAccess)
                    continue;

                var compliance =
                    _compliance.Evaluate(
                        project.Id,
                        project.CompanyId.Value);

                var unified =
                    _risk.Evaluate(compliance, project);

                results.Add(new ProjectComplianceDisplay
                {
                    ProjectId = project.Id,
                    JobNumber = project.JobNumber,
                    ProjectName = project.ProjectName,
                    Client = project.Client,

                    ComplianceScore = unified.ComplianceScore,
                    FinancialScore = unified.FinancialScore,

                    WpsPercent = compliance.WpsCompliancePercentage,
                    DocPercent = compliance.DocumentCompliancePercent,

                    IsCompliant = compliance.IsCompliant,
                    IssueCount = compliance.Issues.Count,

                    RiskScore = unified.Score,
                    RiskLevel = unified.Level,
                    RiskColor = unified.Color
                });
            }

            return results;
        }
    }
}
