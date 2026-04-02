using System.Linq;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Core.Execution
{
	public class BlockReasonEngine
	{
		public BlockResult Evaluate(WorkOrder order)
		{
			// ✅ NULL CHECK FIRST
			if (order == null)
				return BlockResult.Create(BlockReason.Unknown, "Work order is null");

			// ✅ Completed orders are NOT blocked
			if (order.Status == WorkOrderStatus.Completed)
				return BlockResult.None();

			// RULE 1 — Material Check
			var materialResult = CheckMaterials(order);
			if (materialResult.IsBlocked)
				return materialResult;

			// RULE 2 — Dependency Check
			var dependencyResult = CheckDependencies(order);
			if (dependencyResult.IsBlocked)
				return dependencyResult;

			// RULE 3 — Scheduling Check
			var scheduleResult = CheckSchedule(order);
			if (scheduleResult.IsBlocked)
				return scheduleResult;

			// RULE 4 — Capacity Check (placeholder)
			var capacityResult = CheckCapacity(order);
			if (capacityResult.IsBlocked)
				return capacityResult;

			return BlockResult.None();
		}

		private BlockResult CheckMaterials(WorkOrder order)
		{
			if (order.MaterialRequirements == null || !order.MaterialRequirements.Any())
				return BlockResult.None();

			foreach (var req in order.MaterialRequirements)
			{
				// 🔥 TEMP FIX: assume 0 stock until service is wired
				double availableQty = 0;

				if (availableQty <= 0)
				{
					return BlockResult.Create(
						BlockReason.NoStock,
						$"No stock for {req.ItemCode}"
					);
				}

				if (availableQty < req.RequiredQuantity)
				{
					return BlockResult.Create(
						BlockReason.InsufficientStock,
						$"Insufficient stock for {req.ItemCode}"
					);
				}
			}

			return BlockResult.None();
		}

		private BlockResult CheckDependencies(WorkOrder order)
		{
			if (order.Dependencies == null || !order.Dependencies.Any())
				return BlockResult.None();

			var incomplete = order.Dependencies
				.FirstOrDefault(d => d.Status != WorkOrderStatus.Completed);

			if (incomplete != null)
			{
				return BlockResult.Create(
					BlockReason.DependencyNotMet,
					$"Waiting for WO-{incomplete.Id}"
				);
			}

			return BlockResult.None();
		}

		private BlockResult CheckSchedule(WorkOrder order)
		{
			// 🔓 Allow manual execution even if not scheduled
			if (!order.ScheduledStart.HasValue)
			{
				return BlockResult.None();
			}

			return BlockResult.None();
		}

		private BlockResult CheckCapacity(WorkOrder order)
		{
			// Future: integrate with scheduler capacity logic
			return BlockResult.None();
		}
	}
}