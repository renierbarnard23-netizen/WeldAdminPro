using System;
using WeldAdminPro.Core.Quality.Enums;

namespace WeldAdminPro.Core.Quality.Models
{
    public class WeldHoldPoint
    {
        public Guid Id
        {
            get;
            set;
        }

        public Guid WeldId
        {
            get;
            set;
        }

        public HoldPointType HoldPointType
        {
            get;
            set;
        }

        public HoldPointCategory Category
        {
            get;
            set;
        }

        public HoldPointApproverRole RequiredApproverRole
        {
            get;
            set;
        }

        public HoldPointStatus Status
        {
            get;
            set;
        }

        public bool IsMandatory
        {
            get;
            set;
        }

        public string ApprovedBy
        {
            get;
            set;
        } = string.Empty;

        public DateTime? ApprovedDate
        {
            get;
            set;
        }

        public string Comments
        {
            get;
            set;
        } = string.Empty;
    }
}