using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using WeldAdminPro.Core.Quality;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Data.Services;

namespace WeldAdminPro.UI.ViewModels
{
    public partial class ProjectComplianceDashboardViewModel : ObservableObject
    {
        private readonly ProjectRepository _projectRepo = new();
        private readonly ProjectComplianceService _complianceService = new();
        private readonly ProjectRiskService _riskService = new();
        private readonly UnifiedRiskService _unifiedRisk = new();

        public ObservableCollection<ProjectComplianceDisplay> Projects { get; } = new();

        public ProjectComplianceDashboardViewModel()
        {
            Load();
        }

        private void Load()
        {
            Projects.Clear();

            var allProjects = _projectRepo.GetAll();

            foreach (var project in allProjects)
            {
                var compliance = _complianceService.Evaluate(project.Id);
                var unified = _unifiedRisk.Evaluate(compliance, project.Id);

                Projects.Add(new ProjectComplianceDisplay
                {
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
        }
    }

    public class ProjectComplianceDisplay
    {
        public int JobNumber { get; set; }
        public string ProjectName { get; set; } = "";
        public string Client { get; set; } = "";

        public double WpsPercent { get; set; }
        public double DocPercent { get; set; }

        public bool IsCompliant { get; set; }
        public int IssueCount { get; set; }

        public int RiskScore { get; set; }
        public string RiskLevel { get; set; } = "";
        public string RiskColor { get; set; } = "";

        // 🔥 ADD THEM HERE
        public int ComplianceScore { get; set; }
        public int FinancialScore { get; set; }

        public string StatusText =>
            IsCompliant ? "✔ COMPLIANT" : "❌ NON-COMPLIANT";

        public string RiskDisplay =>
            $"{RiskLevel} ({RiskScore})";
    }
}