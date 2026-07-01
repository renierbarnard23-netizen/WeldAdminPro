using System;
using System.Diagnostics;
using WeldAdminPro.Core.Execution;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Core.Analytics.Production
{
	public class ProductionQueueItem
	{
		public Guid Id { get; set; }

		public string WorkOrderNumber { get; set; } = "";

        private int _priority;

        public int Priority
        {
            get => _priority;
            set
            {
                _priority = value;
                Debug.WriteLine(
                    $"UI PRIORITY SET: {WorkOrderNumber} -> {_priority}");
            }
        }

        public string Status { get; set; } = "";

		public string StartDate { get; set; } = "";

		public string DueDate { get; set; } = "";
		public bool IsTopPriority { get; set; }
		public double EstimatedHours { get; set; }
		public DateTime Deadline { get; set; }
		public double PriorityScore { get; set; }
		public bool IsLate { get; set; }
		public string RequiredResource { get; set; } = "";

		public BlockReason BlockReason { get; set; }

		public string BlockMessage { get; set; } = "";
		public WorkOrderType Type { get; set; }

		public bool CanStart => BlockReason == BlockReason.None;
	}
}