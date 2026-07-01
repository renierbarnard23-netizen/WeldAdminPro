using System;

namespace WeldAdminPro.Core.Analytics.Production
{
    public class ProductionAlert
    {
        public Guid WorkOrderId { get; set; }

        public string Severity { get; set; } = "";

        public string Title { get; set; } = "";

        public string Message { get; set; } = "";

        public DateTime CreatedOn { get; set; }

        public string RecommendedAction { get; set; } = "";
    }
}