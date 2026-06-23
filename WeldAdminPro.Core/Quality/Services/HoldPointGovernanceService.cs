using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Core.Quality.Services
{
    public class HoldPointGovernanceService
    {
        public bool HasBlockingHoldPoints(
            IEnumerable<WeldHoldPoint> holdPoints)
        {
            return holdPoints.Any(x =>
                x.IsMandatory &&
                x.Status != HoldPointStatus.Approved);
        }

        public string GetBlockingReason(
            IEnumerable<WeldHoldPoint> holdPoints)
        {
            var blocked =
                holdPoints
                    .Where(x =>
                        x.IsMandatory &&
                        x.Category == HoldPointCategory.Hold &&
                        x.Status != HoldPointStatus.Approved)
                    .ToList();

            if (!blocked.Any())
            {
                return string.Empty;
            }

            return string.Join(
                ", ",
                blocked.Select(x =>
                    x.HoldPointType.ToString()));
        }
    }
}