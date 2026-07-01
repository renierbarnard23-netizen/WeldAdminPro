using System;

namespace WeldAdminPro.Core.Analytics.Production
{
    public class WorkOrderReadiness
    {
        public Guid WorkOrderId { get; set; }

        public string WorkOrderNumber { get; set; } = "";

        public bool MaterialsReady { get; set; }

        public bool DependenciesReady { get; set; }

        public bool IsReady =>
            MaterialsReady &&
            DependenciesReady;

        public string Reason { get; set; } = "";
    }
}