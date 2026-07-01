using System;

namespace WeldAdminPro.Core.Models
{
    public class ReservedMaterial
    {
        public Guid Id { get; set; }

        public Guid WorkOrderId { get; set; }

        public string ItemCode { get; set; } =
            string.Empty;

        public decimal Quantity { get; set; }

        public DateTime ReservedOn { get; set; }
    }
}