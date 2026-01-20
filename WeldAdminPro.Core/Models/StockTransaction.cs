using System;

namespace WeldAdminPro.Core.Models
{
    public class StockTransaction
    {
        public Guid Id { get; set; }
        public Guid StockItemId { get; set; }

        public DateTime TransactionDate { get; set; }

        /// <summary>
        /// Quantity stored as POSITIVE for both IN and OUT
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// "IN" or "OUT"
        /// </summary>
        public string Type { get; set; } = string.Empty;

        public string Reference { get; set; } = string.Empty;

        // =====================================================
        // Business helpers (Core-safe, NO UI dependencies)
        // =====================================================

        /// <summary>
        /// Quantity with sign applied
        /// IN  => +Quantity
        /// OUT => -Quantity
        /// </summary>
        public int SignedQuantity
        {
            get
            {
                return Type == "IN" ? Quantity : -Quantity;
            }
        }

        public bool IsIn => Type == "IN";
        public bool IsOut => Type == "OUT";

        public string QuantityDisplay =>
            (Type == "IN" ? "+" : "-") + Quantity;

        // =====================================================
        // Calculated by ViewModel
        // =====================================================

        /// <summary>
        /// Running balance AFTER this transaction
        /// </summary>
        public int RunningBalance { get; set; }

        // =====================================================
        // Joined values (from SQL JOIN)
        // =====================================================

        public string ItemCode { get; set; } = string.Empty;
        public string ItemDescription { get; set; } = string.Empty;
    }
}
