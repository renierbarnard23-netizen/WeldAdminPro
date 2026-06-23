using System;

namespace WeldAdminPro.Core.Quality.Models
{
    public class WeldTraceabilityRecord
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

        public string WpsNumber
        {
            get;
            set;
        } = string.Empty;

        public string PqrNumber
        {
            get;
            set;
        } = string.Empty;

        public string WelderQualification
        {
            get;
            set;
        } = string.Empty;

        public string MaterialHeatNumber
        {
            get;
            set;
        } = string.Empty;

        public string ConsumableBatch
        {
            get;
            set;
        } = string.Empty;

        public string NdtReportNumber
        {
            get;
            set;
        } = string.Empty;

        public string ReleaseCertificate
        {
            get;
            set;
        } = string.Empty;
    }
}