using System.Collections.ObjectModel;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Core.Analytics.Production
{
    public class ProductionDashboardModel
    {
        // Factory Summary

        public string FactoryStatus { get; set; } = "Healthy";

        public int RunningWorkOrders { get; set; }

        public int ReadyWorkOrders { get; set; }

        public int BlockedWorkOrders { get; set; }

        public int CompletedWorkOrders { get; set; }

        // AI

        public ProductionRecommendationModel? TopRecommendation { get; set; }

        // Work Centres

        public ObservableCollection<WorkCenterStatus> WorkCenters { get; set; }
            = new();

        // Timeline

        public ObservableCollection<ProductionGanttItem> Timeline { get; set; }
            = new();
    }
}