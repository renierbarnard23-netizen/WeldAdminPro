using System;

namespace WeldAdminPro.Core.Models
{
	public class MaterialRequirement
	{
		public Guid Id { get; set; }

		public Guid WorkOrderId { get; set; }

		public string ItemCode { get; set; } = "";

		public double RequiredQuantity { get; set; }

		public double RequiredAmount { get; set; }

		public double RequiredFee { get; set; }

        public double RequiredFeeAmount
        {
            get; set;
        }

        public double RequiredFeeFeeRate
        {
            get;
        }

        public double AvailableQuantity { get; set; }
}
}

