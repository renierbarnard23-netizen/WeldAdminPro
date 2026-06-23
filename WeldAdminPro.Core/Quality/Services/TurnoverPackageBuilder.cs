using System;
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Core.Reporting.Models;

namespace WeldAdminPro.Core.Reporting.Services
{
    public class TurnoverPackageBuilder
    {
        public TurnoverPackage Build(
            Project project,
            List<Weld> welds,
            List<RepairRecord> repairs,
            List<DocumentVaultFile> documents,
            List<WeldNdtResult> ndtResults)
        {
            if (project == null)
            {
                throw new ArgumentNullException(
                    nameof(project));
            }

            welds ??= new();
            repairs ??= new();
            documents ??= new();
            ndtResults ??= new();

            var package =
                new TurnoverPackage
                {
                    ProjectNumber =
                        string.IsNullOrWhiteSpace(
                            project.JobNumber.ToString())
                                ? "N/A"
                                : project.JobNumber.ToString(),

                    ProjectName =
                        string.IsNullOrWhiteSpace(
                            project.ProjectName)
                                ? "Unnamed Project"
                                : project.ProjectName,

                    Welds = welds,
                    Repairs = repairs,
                    Documents = documents,
                    NdtResults = ndtResults
                };

            if (!welds.Any())
            {
                package.Warnings.Add(
                    "No welds exist.");
            }

            if (!documents.Any())
            {
                package.Warnings.Add(
                    "No turnover documents attached.");
            }

            if (!ndtResults.Any())
            {
                package.Warnings.Add(
                    "No NDT results attached.");
            }

            if (repairs.Any(x =>
                    x.Status != RepairStatus.Closed))
            {
                package.Warnings.Add(
                    "Open repairs exist.");
            }

            // We'll fix this section next
            if (welds.Any(x =>
                x.WorkflowStatus !=
                WeldWorkflowStatus.Closed))
            {
                package.Warnings.Add(
                    "Incomplete welds exist.");
            }

            if (!documents.Any(x =>
                    x.IsApproved))
            {
                package.Warnings.Add(
                    "No approved turnover documents exist.");
            }

            package.Warnings =
                package.Warnings
                    .Distinct()
                    .ToList();

            return package;
        }
    }
}