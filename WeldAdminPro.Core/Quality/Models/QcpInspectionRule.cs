using System;
using System.Data;
using WeldAdminPro.Core.Quality.Enums;

namespace WeldAdminPro.Core.Quality.Models
{
    public class QcpInspectionRule
    {
        public Guid Id
        {
            get;
            set;
        }

        public string WeldType
        {
            get;
            set;
        } = string.Empty;

        public NdtType RequiredNdtType
        {
            get;
            set;
        }

        public double InspectionPercentage
        {
            get;
            set;
        }

        public bool RequiresClientWitness
        {
            get;
            set;
        }

        public bool RequiresHoldPoint
        {
            get;
            set;
        }
    }
}