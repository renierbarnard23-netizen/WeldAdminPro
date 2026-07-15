using System;

namespace WeldAdminPro.Core.Models
{
	public class MaterialRequirement
	{
		public Guid Id { get; set; }

		public Guid WorkOrderId { get; set; }

		public string ItemCode { get; set; } = "";

        public string Description { get; set; } = "";

        public string Unit { get; set; } = "";

        public double RequiredQuantity { get; set; }

        public double ReservedQuantity { get; set; }

        public double AvailableQuantity { get; set; }

        public double RequiredAmount { get; set; }

        public double IssuedQuantity { get; set; }

        public bool IsAllocated =>
            ReservedQuantity >= RequiredQuantity;

        public double RequiredFee { get; set; }

        public double RequiredFeeAmount
        {
            get; set;
        }

        public double RequiredFeeFeeRate
        {
            get;
        }

}
}

