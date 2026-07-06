using WeldAdminPro.Data.Services.ProductionEngine;

namespace WeldAdminPro.Web.Services.Dashboard
{
    public class DashboardService
    {
        private readonly ProductionEngineService _productionEngine;

        public DashboardService(
            ProductionEngineService productionEngine)
        {
            _productionEngine = productionEngine;
        }

        public ProductionSnapshot? GetDashboard()
        {
            var result = _productionEngine.Refresh();

            if (!result.Success)
                return null;

            return result.Snapshot;
        }
    }
}