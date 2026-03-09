using System.Collections.Generic;
using WeldAdminPro.Core.Analytics.Production;

namespace WeldAdminPro.Data.Repositories
{
	public class BillOfMaterialRepository
	{
		public List<BillOfMaterial> GetAll()
		{
			return new List<BillOfMaterial>
			{
				new BillOfMaterial
				{
					Id = 1,
					ProductCode = "WO-PRODUCT-001",
					Description = "Example Fabrication Assembly"
				}
			};
		}

		public List<BillOfMaterialItem> GetItems(int bomId)
		{
			return new List<BillOfMaterialItem>
			{
				new BillOfMaterialItem
				{
					BillOfMaterialId = bomId,
					ItemCode = "7018",
					Description = "Welding Rod 7018",
					QuantityRequired = 200
				},
				new BillOfMaterialItem
				{
					BillOfMaterialId = bomId,
					ItemCode = "ARGON",
					Description = "Argon Gas Bottle",
					QuantityRequired = 2
				}
			};
		}
	}
}