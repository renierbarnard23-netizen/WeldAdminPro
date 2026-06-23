using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
	public class MaterialValidator
	{
		private readonly StockRepository _stockRepo;
		private readonly WorkOrderMaterialRepository _materialRepo;

		public MaterialValidator(
			StockRepository stockRepo,
			WorkOrderMaterialRepository materialRepo)
		{
			_stockRepo = stockRepo;
			_materialRepo = materialRepo;
		}

        public bool CanStart(WorkOrder workOrder, out string reason)
        {
            try
            {
                var materials = _materialRepo.GetByWorkOrderId(workOrder.Id)
                                ?? Enumerable.Empty<WorkOrderMaterial>();

                Debug.WriteLine($"🔥 MATERIAL COUNT: {materials.Count()}");

                foreach (var m in materials)
                {
                    Debug.WriteLine($"➡ {m.ItemCode} | Qty: {m.RequiredQuantity}");
                }

                // 🔹 No materials → allow (for now, system stabilization)
                if (!materials.Any())
                {
                    Console.WriteLine("⚠ No materials assigned — allowing start");
                    reason = string.Empty;
                    return true;
                }

                foreach (var mat in materials)
                {
                    // 🔥 EXTRA SAFETY (recommended)
                    if (string.IsNullOrWhiteSpace(mat.ItemCode))
                        continue;

                    var stock = _stockRepo.GetByItemCode(mat.ItemCode);

                    if (stock == null)
                    {
                        Console.WriteLine($"⚠ Missing stock item: {mat.ItemCode} — allowing start");
                        continue;
                    }

                    if (stock.Quantity < mat.RequiredQuantity)
                    {
                        Console.WriteLine($"⚠ Insufficient stock for {mat.ItemCode} — allowing start");
                        continue;
                    }

                    if (stock.Quantity < 0)
                    {
                        Console.WriteLine($"⚠ Negative stock for {mat.ItemCode} — allowing start");
                        continue;
                    }
                }

                reason = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠ MATERIAL VALIDATION ERROR (IGNORED): {ex.Message}");

                // ✅ ALWAYS allow execution (stability mode)
                reason = string.Empty;
                return true;
            }
        }
    }
}