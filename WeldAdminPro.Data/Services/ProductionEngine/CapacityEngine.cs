using System.Collections.ObjectModel;
using WeldAdminPro.Core.Analytics.Production;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Data.Services;

namespace WeldAdminPro.Data.Services.ProductionEngine
{
    /*
    ==========================================================
    WeldAdmin Pro
    Capacity Engine
    ----------------------------------------------------------
    Uses the existing ProductionCapacityService to populate
    the ProductionSnapshot.
    ==========================================================
    */

    public class CapacityEngine
    {
        private readonly ProductionCapacityService _capacityService;

        public CapacityEngine()
        {
            _capacityService =
                new ProductionCapacityService(
                    new WorkOrderRepository());
        }

        public void Evaluate(
            ProductionSnapshot snapshot)
        {
            snapshot.CapacityForecast =
                new ObservableCollection<ProductionCapacityForecast>(
                    _capacityService.GetCapacityForecast());
        }
    }
}