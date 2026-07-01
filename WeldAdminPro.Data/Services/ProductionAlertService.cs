using System;
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Analytics.Production;
using WeldAdminPro.Core.Execution;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
    public class ProductionAlertService
    {
        private readonly WorkOrderRepository _repository;

        public ProductionAlertService()
        {
            _repository =
                new WorkOrderRepository();
        }

        public List<ProductionAlert> GetAlerts()
        {
            var alerts =
                new List<ProductionAlert>();

            var orders =
                _repository
                    .GetAll()
                    .ToList();

            foreach (var wo in orders)
            {
                //--------------------------------------------------
                // Ignore completed work
                //--------------------------------------------------

                if (wo.Status ==
                    WorkOrderStatus.Completed)
                    continue;

                //--------------------------------------------------
                // Blocked
                //--------------------------------------------------

                if (wo.BlockReason !=
                    BlockReason.None)
                {
                    alerts.Add(
                        new ProductionAlert
                        {
                            WorkOrderId =
                                wo.Id,

                            Severity =
                                "Critical",

                            Title =
                                wo.WorkOrderNumber,

                            Message =
                                $"Blocked: {wo.BlockReason}",

                            CreatedOn =
                                DateTime.Now,

                            RecommendedAction =
                                "Resolve blockage"
                        });
                }

                //--------------------------------------------------
                // Overdue
                //--------------------------------------------------

                if (wo.DueDate.HasValue &&
                    wo.DueDate.Value.Date <
                    DateTime.Today)
                {
                    alerts.Add(
                        new ProductionAlert
                        {
                            WorkOrderId =
                                wo.Id,

                            Severity =
                                "Warning",

                            Title =
                                wo.WorkOrderNumber,

                            Message =
                                "Work order overdue",

                            CreatedOn =
                                DateTime.Now,

                            RecommendedAction =
                                "Reschedule immediately"
                        });
                }

                //--------------------------------------------------
                // Due Today
                //--------------------------------------------------

                if (wo.DueDate.HasValue &&
                    wo.DueDate.Value.Date ==
                    DateTime.Today)
                {
                    alerts.Add(
                        new ProductionAlert
                        {
                            WorkOrderId =
                                wo.Id,

                            Severity =
                                "Info",

                            Title =
                                wo.WorkOrderNumber,

                            Message =
                                "Due today",

                            CreatedOn =
                                DateTime.Now,

                            RecommendedAction =
                                "Prioritise completion"
                        });
                }
            }

            return alerts
                .OrderByDescending(x => x.CreatedOn)
                .ToList();
        }
    }
}