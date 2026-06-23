using WeldAdminPro.Core.Quality;

namespace WeldAdminPro.Data.Services
{
    public class QualityDocumentService
    {
        public List<QualityDocumentRequirement> GetDefaultChecklist()
        {
            return new List<QualityDocumentRequirement>
            {
                new() { DocumentName = "Index", IsRequired = true },
                new() { DocumentName = "List of Involved Parties", IsRequired = true },
                new() { DocumentName = "Method Statement", IsRequired = true },
                new() { DocumentName = "QCP", IsRequired = true },
                new() { DocumentName = "Drawings", IsRequired = true },

                new() { DocumentName = "WPS", IsRequired = true },
                new() { DocumentName = "PQR", IsRequired = true },
                new() { DocumentName = "WPQR", IsRequired = true },

                new() { DocumentName = "Material Certificates", IsRequired = true },
                new() { DocumentName = "Consumable Certificates", IsRequired = true },

                new() { DocumentName = "NDT Procedures", IsRequired = true },
                new() { DocumentName = "NDT Reports", IsRequired = true },

                new() { DocumentName = "NCRs", IsRequired = false },

                new() { DocumentName = "Pressure Test Certificates", IsRequired = true },
                new() { DocumentName = "Certificate of Conformance", IsRequired = true },
                new() { DocumentName = "ISO 3834 Certificate", IsRequired = true }
            };
        }

        public (bool IsCompliant, List<string> MissingDocs) CheckCompliance(List<QualityDocumentRequirement> docs)
        {
            var missing = docs
                .Where(d => d.IsRequired && !d.IsUploaded)
                .Select(d => d.DocumentName)
                .ToList();

            return (!missing.Any(), missing);
        }
    }
}