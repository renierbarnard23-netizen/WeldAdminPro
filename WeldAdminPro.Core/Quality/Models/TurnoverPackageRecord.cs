using System;

namespace WeldAdminPro.Core.Quality.Models
{
    public class TurnoverPackageRecord
    {
        public Guid Id
        {
            get;
            set;
        }

        public Guid ProjectId
        {
            get;
            set;
        }

        public string PackageNumber
        {
            get;
            set;
        } = string.Empty;

        public DateTime CreatedDate
        {
            get;
            set;
        }

        public string CreatedBy
        {
            get;
            set;
        } = string.Empty;

        public bool IsApproved
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
    }
}