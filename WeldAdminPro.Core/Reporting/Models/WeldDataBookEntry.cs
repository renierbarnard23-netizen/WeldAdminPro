using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Core.Reporting.Models
{
    public class WeldDataBookEntry
    {
        public Weld Weld { get; set; } = new();

        public List<WeldNdtResult> NdtResults { get; set; }
            = new();

        public List<WeldHistoryEntry> History { get; set; }
            = new();
    }
}