namespace WeldAdminPro.Core.Quality.Models
{
    public class WeldReadinessIndicator
    {
        public bool ReleaseReady
        {
            get;
            set;
        }

        public bool TurnoverReady
        {
            get;
            set;
        }

        public int BlockingCount
        {
            get;
            set;
        }

        public string ReadinessSummary
        {
            get;
            set;
        } = string.Empty;
    }
}