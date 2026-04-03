using WeldAdminPro.Core.Models;

public class PlanningContext
{
	public IEnumerable<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();

	public required Func<Guid, IEnumerable<MaterialRequirement>> GetMaterials { get; set; }

	public required Func<string, double> GetStock { get; set; }

	public required Func<string, double> GetCapacity { get; set; }
}