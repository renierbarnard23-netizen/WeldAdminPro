using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Analytics.Production;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
    public class WorkCenterCapacityService
    {
        private readonly WorkOrderRepository _repo;

        public WorkCenterCapacityService()
        {
            _repo =
                new WorkOrderRepository();
        }

        public List<WorkCenterCapacity>
            GetCapacity()
        {
            var workOrders =
                _repo.GetAll()
                     .Where(x =>
                        x.Status !=
                        WorkOrderStatus.Completed)
                     .ToList();

            const double availableHours =
                40;

            return workOrders
                .GroupBy(x =>
                    string.IsNullOrWhiteSpace(
                        x.WorkCenter)
                        ? "Unassigned"
                        : x.WorkCenter)
                .Select(g =>
                {
                    var scheduledHours =
                        g.Sum(x =>
                            x.EstimatedHours);

                    var utilization =
                        availableHours <= 0
                            ? 0
                            : (scheduledHours /
                               availableHours)
                              * 100;

                    return new WorkCenterCapacity
                    {
                        WorkCenter =
                            g.Key,

                        ActiveWorkOrders =
                            g.Count(),

                        ScheduledHours =
                            scheduledHours,

                        AvailableHours =
                            availableHours,

                        UtilizationPercent =
                            System.Math.Round(
                                utilization,
                                1)
                    };
                })
                .OrderByDescending(
                    x =>
                        x.UtilizationPercent)
                .ToList();
        }
    }
}