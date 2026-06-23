using System.Collections.Generic;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Core.Reporting.Models
{
    public class TurnoverPackage
    {
        public string ProjectNumber { get; set; }
        = string.Empty;

        public string ProjectName { get; set; }
        = string.Empty;

        public List<Weld> Welds { get; set; }
            = new();

        public List<RepairRecord> Repairs { get; set; }
            = new();

        public List<DocumentVaultFile> Documents
        { get; set; }
            = new();

        public List<string> Warnings
        { get; set; }
            = new();

        public List<Quality.Models.WeldNdtResult>
        NdtResults
        { get; set; }
        = new();

    }
}
