using System;
using System.Linq;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Data
{
    public static class DataSeeder
    {
        public static void Seed(WeldAdminDbContext db)
        {
            if (db.Projects.Any()) return; // already seeded

            var proj = new Project
            {
                ProjectNumber = "DOE-2025-001",
                Title = "No.11 Plant Turbine Sprinkler Installation",
                Client = "Dynamic Options Engineering",
                StartDate = DateTime.UtcNow.Date,
                DatabookVersion = "v1"
            };

            db.Projects.Add(proj);

            var doc1 = new Document
            {
                ProjectId = proj.Id,
                DocType = "WPS",
                Title = "WPS - SA 304L TIG",
                DocumentNumber = "WPS-SA304L-001",
                Revision = "A",
                Status = "Approved",
                UploadedAt = DateTime.UtcNow,
                UploadedBy = "admin",
                FilePath = "storage/DOE-2025-001/WPS-SA304L-001_A.pdf"
            };

            var doc2 = new Document
            {
                ProjectId = proj.Id,
                DocType = "QCP",
                Title = "QCP - Turbine Sprinkler Installation",
                DocumentNumber = "QCP-TSI-001",
                Revision = "0",
                Status = "Draft",
                UploadedAt = DateTime.UtcNow,
                UploadedBy = "engineer",
                FilePath = "storage/DOE-2025-001/QCP-TSI-001_0.docx"
            };

            db.Documents.AddRange(doc1, doc2);

            var wps = new Wps
            {
                ProjectId = proj.Id,
                WpsNumber = "WPS-SA304L-001",
                Revision = "A",
                BaseMaterial = "SA-304L",
                FillerMaterial = "ER308L",
                Process = "GTAW",
                Position = "PA",
                Preheat = "No",
                PostHeat = "No",
                Author = "Welding Engineer",
                ApprovedBy = "QA Manager",
                ApprovedAt = DateTime.UtcNow
            };

            db.Wpss.Add(wps);

            db.SaveChanges();
        }
    }
}
