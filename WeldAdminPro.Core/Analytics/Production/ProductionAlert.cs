namespace WeldAdminPro.Core.Analytics.Production
{
    public class ProductionAlert
    {
        public string Severity { get; set; } = "";

        public string Message { get; set; } = "";

        public DateTime CreatedDate
        {
            get;
            set;
        }
        = DateTime.Now;
    }
}