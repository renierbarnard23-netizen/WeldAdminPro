using System.Collections.Generic;
using WeldAdminPro.Core.Analytics.Production;

namespace WeldAdminPro.Data.Repositories
{
	public class BillOfMaterialRepository
	{
		public List<BillOfMaterial> GetAll()
		{
			return new List<BillOfMaterial>();
		}

		public List<BillOfMaterialItem> GetItems(int bomId)
		{
			return new List<BillOfMaterialItem>();
		}
	}
}