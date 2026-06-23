using System.Collections.Generic;

namespace WeldAdminPro.Core.Quality.Models
{
    public class TurnoverReadinessResult
    {
        public bool IsReady { get; set; }

        public List<string> BlockingIssues { get; set; }
        = new();

        public int OpenWelds { get; set; }

        public int OpenRepairs { get; set; }

        public int PendingReinspections { get; set; }

        public int MissingDocuments { get; set; }
    }
}
