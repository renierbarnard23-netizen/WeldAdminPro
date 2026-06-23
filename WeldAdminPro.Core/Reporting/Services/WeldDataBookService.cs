using PdfSharpCore.Pdf.IO;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Core.Quality.Services;
using WeldAdminPro.Core.Reporting.Models;

namespace WeldAdminPro.Core.Reporting.Services
{
    public class WeldDataBookService
    {

        private readonly WelderAnalyticsService _welderAnalyticsService = new();
        private readonly WelderPerformanceService _welderPerformanceService = new();
        public WeldDataBook Build(
            CompanyProfile company,
            string projectName,
            string clientName,
            List<Weld> welds,
            List<WeldNdtResult> ndtResults,
            List<WeldHistoryEntry> history,
            DataBookRevision revision)
        {
            var book = new WeldDataBook
            {
                Company = company,
                ProjectName = projectName,
                ClientName = clientName,
                GeneratedDate = DateTime.Now,
                RevisionInfo = revision,
            };

            book.WelderPerformance = _welderPerformanceService.Build(welds);

            book.Sections.Add(
                new DataBookSection
        {
                Number = 1,
                Title = "Executive Summary",
                PageNumber = 2
            });

            book.Sections.Add(
                new DataBookSection
                {
                    Number = 2,
                    Title = "Weld Summary",
                    PageNumber = 3
                });

            book.Sections.Add(
                new DataBookSection
                {
                    Number = 3,
                    Title = "Detailed Weld Records",
                    PageNumber = 4
                });

            book.Sections.Add(
                new DataBookSection
                {
                    Number = 4,
                    Title = "Welder Performance",
                    PageNumber = 8
                });

            book.Sections.Add(
                new DataBookSection
                {
                    Number = 5,
                    Title = "Attachments",
                    PageNumber = 9
                });            

            book.RevisionHistory.Add(
                new RevisionHistoryEntry
                {
                    Revision =
                    revision.Revision,

                    RevisionDate =
                    revision.RevisionDate,

                    Description =
                    revision.RevisionNotes,

                    PreparedBy =
                    revision.PreparedBy,

                    ApprovedBy =
                    revision.ApprovedBy,

                    Status =
                    revision.Status
                });


            // =====================================
            // BUILD WELD ENTRIES
            // =====================================

            foreach (var weld in welds)
            {
                var entry = new WeldDataBookEntry
                {
                    Weld = weld,

                    NdtResults = ndtResults
                        .Where(x => x.WeldId == weld.Id)
                        .ToList(),

                    History = history
                        .Where(x => x.WeldId == weld.Id)
                        .ToList()
                };

                book.Welds.Add(entry);
            }

            // =====================================
            // ANALYTICS
            // =====================================

            var analyticsService =
                new WeldAnalyticsService();

            book.Analytics =
                analyticsService.Generate(welds);

            // =====================================
            // CHARTS
            // =====================================

            var chartService =
                new WeldRepairChartService();

            book.RepairStatusChart =
                chartService.GenerateRepairStatusChart(
                    welds);

            book.WelderMetrics = 
                _welderAnalyticsService 
                .BuildWelderMetrics(welds);

            return book;
        }
    }
}
