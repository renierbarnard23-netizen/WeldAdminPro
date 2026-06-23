using System;
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Enums;

namespace WeldAdminPro.Core.Quality.Services
{
    public class TurnoverPackageService
    {
        public bool CanGeneratePackage(
            IEnumerable<Weld> welds,
            out List<string> blockingReasons)
        {
            blockingReasons =
                welds
                    .Where(x =>
                        x.WorkflowStatus
                        != WeldWorkflowStatus.Released)
                    .Select(x =>
                        $"{x.WeldNumber} not released")
                    .ToList();

            return !blockingReasons.Any();
        }
    }
}