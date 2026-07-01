using System.Linq;
using WeldAdminPro.Core.Analytics.Production;

namespace WeldAdminPro.Data.Services.ProductionEngine
{
    public class SchedulingEngine
    {
        private readonly ProductionSchedulingService _schedulingService;

        public SchedulingEngine()
        {
            _schedulingService =
                new ProductionSchedulingService();
        }

        public void Build(
            ProductionSnapshot snapshot)
        {
            snapshot.Queue =
                _schedulingService
                    .BuildQueue()
                    .Where(q => q.Status != "Completed")
                    .ToList();
        }
    }
}