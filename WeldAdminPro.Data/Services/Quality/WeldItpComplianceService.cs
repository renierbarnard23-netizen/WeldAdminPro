using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services.Quality
{
    /// <summary>
    /// Evaluates whether a weld satisfies the mandatory requirements
    /// of the active Inspection Test Plan for its project.
    ///
    /// No active ITP means there are no ITP requirements to enforce.
    /// </summary>
    public class WeldItpComplianceService
    {
        private readonly InspectionTestPlanRepository
            _itpRepository;

        private readonly InspectionTestPlanItemRepository
            _itemRepository;

        private readonly InspectionTestPlanExecutionRepository
            _executionRepository;

        private readonly WeldNdtRepository
            _weldNdtRepository;

        private readonly HoldPointRepository
            _holdPointRepository;

        private readonly InspectionTestPlanInspectionExecutionRepository
            _inspectionExecutionRepository;

        public WeldItpComplianceService(
            InspectionTestPlanRepository itpRepository,
            InspectionTestPlanItemRepository itemRepository,
            InspectionTestPlanExecutionRepository executionRepository,
            WeldNdtRepository weldNdtRepository,
            HoldPointRepository holdPointRepository,
            InspectionTestPlanInspectionExecutionRepository inspectionExecutionRepository)
        {
            _itpRepository = itpRepository;
            _itemRepository = itemRepository;
            _executionRepository = executionRepository;
            _weldNdtRepository = weldNdtRepository;
            _holdPointRepository = holdPointRepository;
            _inspectionExecutionRepository = inspectionExecutionRepository;
        }

        public WeldItpComplianceResult Evaluate(
            Weld weld)
        {
            var result =
                new WeldItpComplianceResult
                {
                    IsCompliant = true
                };

            if (weld == null)
            {
                result.IsCompliant = false;

                result.BlockingIssues.Add(
                    "Weld is required for ITP compliance evaluation.");

                return result;
            }

            var activeItp =
                _itpRepository
                    .GetByProject(weld.ProjectId)
                    .FirstOrDefault(x =>
                        x.Status ==
                            InspectionTestPlanStatus.Active &&
                        x.IsActive);

            // No active ITP means there are no ITP requirements
            // to enforce for release.
            if (activeItp == null)
            {
                return result;
            }

            result.ActiveItpId = activeItp.Id;
            result.ActiveItpNumber = activeItp.ItpNumber;
            result.ActiveItpRevision = activeItp.Revision;

            var items =
                _itemRepository
                    .GetByPlan(activeItp.Id)
                    .Where(x => x.Mandatory)
                    .ToList();

            foreach (var item in items)
            {
                if (string.Equals(
                        item.ExecutionType,
                        nameof(InspectionTestPlanExecutionType.NdtResult),
                        StringComparison.OrdinalIgnoreCase))
                {
                    if (!item.RequiredNdtMethod.HasValue)
                    {
                        result.IsCompliant = false;

                        result.BlockingIssues.Add(
                            $"Mandatory ITP NDT requirement is invalid: " +
                            $"{item.Activity} has no required NDT method.");

                        continue;
                    }

                    var executions =
                        _executionRepository
                            .GetByItem(item.Id)
                            .Where(x =>
                                string.Equals(
                                    x.ExecutionType,
                                    nameof(InspectionTestPlanExecutionType.NdtResult),
                                    StringComparison.OrdinalIgnoreCase))
                            .ToList();

                    var satisfied =
                        executions.Any(execution =>
                        {
                            if (!Guid.TryParse(
                                    execution.ExecutionRecordId,
                                    out var recordId))
                            {
                                return false;
                            }

                            var ndt =
                                _weldNdtRepository
                                    .GetById(recordId);

                            if (ndt == null)
                            {
                                return false;
                            }

                            if (ndt.WeldId != weld.Id)
                            {
                                return false;
                            }

                            if (ndt.NdtMethod !=
                                item.RequiredNdtMethod.Value)
                            {
                                return false;
                            }

                            return ndt.Result ==
                                       NdtResultType.Accept ||
                                   ndt.Result ==
                                       NdtResultType.ConditionalAccept;
                        });

                    if (!satisfied)
                    {
                        result.IsCompliant = false;

                        result.BlockingIssues.Add(
                            $"Mandatory ITP NDT requirement not satisfied: " +
                            $"{item.Activity} requires " +
                            $"{item.RequiredNdtMethod.Value}.");
                    }

                    continue;
                }

                if (string.Equals(
                        item.ExecutionType,
                        nameof(InspectionTestPlanExecutionType.Inspection),
                        StringComparison.OrdinalIgnoreCase))
                {
                    var executions =
                        _inspectionExecutionRepository
                            .GetByItem(item.Id)
                            .Where(x => x.WeldId == weld.Id)
                            .ToList();

                    var satisfied =
                        executions.Any(execution =>
                            execution.Status ==
                                InspectionTestPlanInspectionExecutionStatus.Completed &&
                            (execution.Result ==
                                InspectionTestPlanInspectionResult.Accept ||
                             execution.Result ==
                                InspectionTestPlanInspectionResult.ConditionalAccept));

                    if (!satisfied)
                    {
                        result.IsCompliant = false;

                        result.BlockingIssues.Add(
                            $"Mandatory ITP Inspection requirement not satisfied: " +
                            $"{item.Activity} requires a completed inspection with an " +
                            $"Accept or ConditionalAccept result.");
                    }

                    continue;
                }

                if (string.Equals(
                        item.ExecutionType,
                        nameof(InspectionTestPlanExecutionType.HoldPoint),
                        StringComparison.OrdinalIgnoreCase))
                {
                    var executions =
                        _executionRepository
                            .GetByItem(item.Id)
                            .Where(x =>
                                string.Equals(
                                    x.ExecutionType,
                                    nameof(InspectionTestPlanExecutionType.HoldPoint),
                                    StringComparison.OrdinalIgnoreCase))
                            .ToList();

                    var satisfied =
                        executions.Any(execution =>
                        {
                            if (!Guid.TryParse(
                                    execution.ExecutionRecordId,
                                    out var recordId))
                            {
                                return false;
                            }

                            var holdPoint =
                                _holdPointRepository
                                    .GetById(recordId);

                            if (holdPoint == null)
                            {
                                return false;
                            }

                            if (holdPoint.WeldId != weld.Id)
                            {
                                return false;
                            }

                            return holdPoint.Status ==
                                HoldPointStatus.Approved;
                        });

                    if (!satisfied)
                    {
                        result.IsCompliant = false;

                        result.BlockingIssues.Add(
                            $"Mandatory ITP Hold Point not satisfied: " +
                            $"{item.Activity} requires an approved Hold Point.");
                    }

                    continue;
                }

                result.IsCompliant = false;

                result.BlockingIssues.Add(
                    $"Mandatory ITP requirement is unsupported: " +
                    $"{item.Activity} uses execution type " +
                    $"{item.ExecutionType}.");
            }

            return result;
        }
    }

    public class WeldItpComplianceResult
    {
        public bool IsCompliant { get; set; }

        public Guid? ActiveItpId { get; set; }

        public string? ActiveItpNumber { get; set; }

        public string? ActiveItpRevision { get; set; }

        public List<string> BlockingIssues { get; } =
            new();
    }
}






