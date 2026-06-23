namespace WeldAdminPro.Core.Analytics.Models
{
    public class WelderPerformanceRecord
    {
        public string WelderNumber
        {
            get;
            set;
        } = string.Empty;

        public int TotalWelds
        {
            get;
            set;
        }

        public int RejectedWelds
        {
            get;
            set;
        }

        public int RepairCount
        {
            get;
            set;
        }

        public double RejectRate
        {
            get;
            set;
        }

        public int NcrCount
        {
            get;
            set;
        }
    }
}