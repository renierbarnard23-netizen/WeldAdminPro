using System.Collections.ObjectModel;
using WeldAdminPro.Core.Reporting.Models;

namespace WeldAdminPro.Core.Quality.Models
{
    public class QualityKpiModel
    {
        // =====================================
        // WELDING KPIs
        // =====================================

        public int TotalWelds { get; set; }

        public int RejectedWelds { get; set; }

        public double RejectRate { get; set; }

        public double RepairRate { get; set; }

        // =====================================
        // NCR KPIs
        // =====================================

        public int TotalNcrs { get; set; }

        public int OpenNcrs { get; set; }

        // =====================================
        // CAPA KPIs
        // =====================================

        public int TotalCapas { get; set; }

        public int OpenCapas { get; set; }

        public int OverdueCapas { get; set; }

        // =====================================
        // TREND ANALYTICS
        // =====================================

        public string WorstWelder { get; set; }
            = "N/A";

        public string TopDefectType { get; set; }
            = "N/A";

        // =====================================
        // EXECUTIVE SCORING
        // =====================================

        public double QualityScore { get; set; }

        public string RiskLevel { get; set; }
            = "LOW";

        // =====================================
        // WELDER ANALYTICS
        // =====================================

        public ObservableCollection<WelderPerformanceModel>
            WelderPerformance
        { get; set; }
                    = new();

        // =====================================
        // DEFECT ANALYTICS
        // =====================================

        public ObservableCollection<DefectParetoModel>
            ParetoDefects
        { get; set; }
                    = new();

        // =====================================
        // CAPA AGING
        // =====================================

        public ObservableCollection<CapaAgingModel>
            CapaAging
        { get; set; }
                    = new();

        // =====================================
        // NCR LIFECYCLE
        // =====================================

        public ObservableCollection<NcrLifecycleModel>
            NcrLifecycle
        { get; set; }
                    = new();

        // =====================================
        // REPAIR COST ANALYTICS
        // =====================================

        public ObservableCollection<RepairCostModel>
            RepairCosts
        { get; set; }
                    = new();

        // =====================================
        // WELDER QUALIFICATION RISK
        // =====================================

        public ObservableCollection<WelderQualificationRiskModel>
            QualificationRisks
        { get; set; }
                    = new();

        // =====================================
        // PRODUCTION BOTTLENECKS
        // =====================================

        public ObservableCollection<ProductionBottleneckModel>
            Bottlenecks
        { get; set; }
                    = new();

        // =====================================
        // AI REPAIR PREDICTIONS
        // =====================================

        public ObservableCollection<RepairPredictionModel>
            RepairPredictions
        { get; set; }
                    = new();

        // =====================================
        // ALERTS
        // =====================================

        public ObservableCollection<string>
            Alerts
        { get; set; }
                    = new();
    }
}