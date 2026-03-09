using WeldAdminPro.Core.Enums;

namespace WeldAdminPro.Core.Analytics.Production
{
	public class WorkOrderExecutionStatusModel
	{
		public string WorkOrderNumber { get; set; }

		public WorkOrderExecutionStatus Status { get; set; }

		public string Reason { get; set; }

		public string StatusText => Status.ToString();
	}
}