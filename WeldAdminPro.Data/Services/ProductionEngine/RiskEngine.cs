using System.Collections.ObjectModel;
using System.Linq;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Services.Risk;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services.ProductionEngine
{
    public class RiskEngine
    {
        private readonly WorkOrderRepository _workOrderRepository;
        private readonly DeadlineRiskDetectionService _deadlineService;

        public RiskEngine()
        {
            _workOrderRepository = new WorkOrderRepository();
            _deadlineService = new DeadlineRiskDetectionService();
        }

        public void Evaluate(
            ProductionSnapshot snapshot)
        {
            var workOrders =
                _workOrderRepository
                    .GetAll()
                    .ToList();

            snapshot.DeadlineRisks =
                new ObservableCollection<DeadlineRisk>(
                    _deadlineService.GetDeadlineRisks(workOrders));
        }
    }
}