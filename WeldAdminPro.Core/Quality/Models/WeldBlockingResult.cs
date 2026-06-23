using System.Collections.Generic;

namespace WeldAdminPro.Core.Quality.Models
{
    public class WeldBlockingResult
    {
        public bool CanRelease
        {
            get;
            set;
        }

        public bool CanTurnover
        {
            get;
            set;
        }

        public bool CanClose
        {
            get;
            set;
        }

        public List<string> BlockingReasons
        {
            get;
            set;
        } = new();
    }
}