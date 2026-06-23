using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Core.Reporting.Models;

namespace WeldAdminPro.Core.Quality.Services
{
    public class DocumentComplianceEngine
    {
        public DocumentComplianceResult Evaluate(
        List<DocumentVaultFile> documents,
        List<DocumentRequirement> requirements)
        {
            var result =
            new DocumentComplianceResult();

        foreach (var requirement in requirements)
            {
                var matching =
                    documents.Count(x =>
                        x.Category ==
                        requirement.Category
                        &&
                        x.IsApproved);

                if (requirement.IsRequired
                    &&
                    matching < requirement.MinimumRequired)
                {
                    result.MissingRequiredDocuments++;

                    result.Issues.Add(
                        $"Missing required document category: " +
                        $"{requirement.Category}");
                }
            }

            result.IsCompliant =
                result.MissingRequiredDocuments == 0;

            return result;
        }
    }
}
