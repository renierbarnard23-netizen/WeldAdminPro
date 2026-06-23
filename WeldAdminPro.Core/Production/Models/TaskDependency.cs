namespace WeldAdminPro.Core.Production.Models
{
	public class TaskDependency
	{
		public Guid Id { get; set; }

		public Guid TaskId { get; set; }          // The task that depends on something
		public Guid DependsOnTaskId { get; set; } // The prerequisite task

		public List<Guid> DependencyIds { get; set; } = new();
	}
}