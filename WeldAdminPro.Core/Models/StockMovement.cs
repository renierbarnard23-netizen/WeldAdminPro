using System;

namespace WeldAdminPro.Core.Models
{
    public class StockMovement
    {
        public Guid Id { get; set; }

        public Guid StockItemId { get; set; }

        /// <summary>
        /// IN, OUT, ADJUST
        /// </summary>
        public string MovementType { get; set; } = string.Empty;

        public double Quantity { get; set; }

        public DateTime MovementDate { get; set; }

        public string? Reference { get; set; }
        public string? Notes { get; set; }
    }
}
