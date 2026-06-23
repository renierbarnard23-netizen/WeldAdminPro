namespace WeldAdminPro.Core.Quality.Models
{
    public class WeldBlockingDisplayItem
    {
        public string WeldNumber
        {
            get;
            set;
        } = string.Empty;

        public string WorkflowStatus
        {
            get;
            set;
        } = string.Empty;

        public bool IsBlocked
        {
            get;
            set;
        }

        public string BlockingReasons
        {
            get;
            set;
        } = string.Empty;
    }
}