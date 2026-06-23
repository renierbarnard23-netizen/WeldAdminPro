using System;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Core.Quality.Services
{
    public class RepairWorkflowService
    {
        public bool CanTransition(
        RepairStatus current,
        RepairStatus next)
        {
            return (current, next) switch
            {
            (RepairStatus.Requested,
            RepairStatus.Authorized) => true,

            (RepairStatus.Authorized,
                RepairStatus.ExcavationInProgress) => true,

            (RepairStatus.ExcavationInProgress,
                RepairStatus.RepairWeldingInProgress) => true,

            (RepairStatus.RepairWeldingInProgress,
                RepairStatus.PendingReinspection) => true,

            (RepairStatus.PendingReinspection,
                RepairStatus.Accepted) => true,

            (RepairStatus.PendingReinspection,
                RepairStatus.Rejected) => true,

            (RepairStatus.Accepted,
                RepairStatus.Closed) => true,

            _ => false
                };
    }

    public bool MoveToStatus(
        RepairRecord repair,
        RepairStatus newStatus,
        out string error)
        {
            error = string.Empty;

            if (!CanTransition(
                    repair.Status,
                    newStatus))
            {
                error =
                    $"Invalid repair transition from " +
                    $"{repair.Status} to {newStatus}.";

                return false;
            }

            repair.Status = newStatus;

            return true;
        }

        public bool AuthorizeRepair(
            RepairRecord repair,
            string authorizedBy,
            out string error)
        {
            if (!MoveToStatus(
                    repair,
                    RepairStatus.Authorized,
                    out error))
            {
                return false;
            }

            repair.AuthorizedBy =
                authorizedBy;

            repair.AuthorizedDate =
                DateTime.UtcNow;

            return true;
        }

        public bool StartExcavation(
            RepairRecord repair,
            out string error)
        {
            return MoveToStatus(
                repair,
                RepairStatus.ExcavationInProgress,
                out error);
        }

        public bool StartRepairWelding(
            RepairRecord repair,
            out string error)
        {
            return MoveToStatus(
                repair,
                RepairStatus.RepairWeldingInProgress,
                out error);
        }

        public bool SendForReinspection(
            RepairRecord repair,
            out string error)
        {
            return MoveToStatus(
                repair,
                RepairStatus.PendingReinspection,
                out error);
        }

        public bool AcceptRepair(
            RepairRecord repair,
            out string error)
        {
            return MoveToStatus(
                repair,
                RepairStatus.Accepted,
                out error);
        }

        public bool RejectRepair(
            RepairRecord repair,
            out string error)
        {
            return MoveToStatus(
                repair,
                RepairStatus.Rejected,
                out error);
        }

        public bool CloseRepair(
            RepairRecord repair,
            out string error)
        {
            return MoveToStatus(
                repair,
                RepairStatus.Closed,
                out error);
        }
    }

}
