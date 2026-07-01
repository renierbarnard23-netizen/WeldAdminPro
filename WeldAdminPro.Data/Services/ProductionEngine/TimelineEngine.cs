using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Analytics.Production;
using WeldAdminPro.Data.Services;

namespace WeldAdminPro.Data.Services.ProductionEngine
{
    /*
    ==========================================================
    WeldAdmin Pro
    Timeline Engine
    ----------------------------------------------------------
    Converts the production schedule into a timeline that
    can be displayed by the dashboard.
    ==========================================================
    */

    public class TimelineEngine
    {
        private readonly ProductionScheduleService _scheduleService;

        public TimelineEngine()
        {
            _scheduleService =
                new ProductionScheduleService();
        }

        public void Evaluate(
            ProductionSnapshot snapshot)
        {
            var schedule =
                _scheduleService.GetSchedule();

            if (!schedule.Any())
            {
                snapshot.Timeline =
                    new List<ProductionGanttItem>();

                return;
            }

            var minDate =
                schedule.Min(s => s.StartDate);

            snapshot.Timeline =
                schedule.Select(s => new ProductionGanttItem
                {
                    WorkOrderNumber = s.WorkOrderNumber,

                    StartDate = s.StartDate,

                    DueDate = s.EndDate,

                    Status = "Scheduled",

                    StartOffset =
                        (s.StartDate - minDate).TotalDays * 40,

                    Duration =
                        (s.EndDate - s.StartDate).TotalDays * 40
                })
                .ToList();
        }
    }
}