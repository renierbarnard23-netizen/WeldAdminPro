using System.Collections.Generic;

namespace WeldAdminPro.Core.Quality.Models
{
    public class ProjectMrbExportModel
    {
        public string ProjectNumber { get; set; }
            = "";

        public string ProjectName { get; set; }
            = "";

        public List<WeldDossierExportModel> Welds
        {
            get;
            set;
        }
            = new();
    }
}