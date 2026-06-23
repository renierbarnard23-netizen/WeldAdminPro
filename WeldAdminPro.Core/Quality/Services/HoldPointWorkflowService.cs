using System;
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Core.Services;

namespace WeldAdminPro.Core.Quality.Services
{
    public class HoldPointWorkflowService
    {
        public bool CanAdvanceWorkflow(
            IEnumerable<WeldHoldPoint> holdPoints,
            out List<string> blockingReasons)
        {
            blockingReasons =
                holdPoints
                .Where(x =>
                    x.IsMandatory &&
                    x.Category == HoldPointCategory.Hold &&
                    x.Status != HoldPointStatus.Approved)
                .Select(x =>
                    $"{x.HoldPointType} not approved")
                .ToList();

            return !blockingReasons.Any();
        }

        public void Approve(
            WeldHoldPoint holdPoint,
            string approvedBy,
            string comments)
        {

            if (CurrentUserContext.Role
    == Core.Enums.SystemRole.Viewer)
            {
                throw new InvalidOperationException(
                    "User does not have approval permissions.");
            }

            holdPoint.Status =
                HoldPointStatus.Approved;

            holdPoint.ApprovedBy =
                approvedBy;

            holdPoint.ApprovedDate =
                DateTime.UtcNow;

            holdPoint.Comments =
                comments;
        }

        public void Reject(
            WeldHoldPoint holdPoint,
            string comments)
        {
            holdPoint.Status =
                HoldPointStatus.Rejected;

            holdPoint.Comments =
                comments;
        }
    }
}