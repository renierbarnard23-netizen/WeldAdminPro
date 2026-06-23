using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace WeldAdminPro.Core.Quality
{
    public partial class ProjectDocument
        : ObservableObject
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ProjectId { get; set; }

        public string DocumentName { get; set; } = "";
        public string DocumentType { get; set; } = "";

        [ObservableProperty]
        private bool isRequired;

        [ObservableProperty]
        private bool isUploaded;
        public string? FilePath { get; set; }

        // 🔥 FIX — ADD THIS
        public DateTime? UploadedDate { get; set; }
        public DateTime? LastModifiedOn { get; set; }
        public string ApprovedBy { get; set; } = "";

        public int Revision { get; set; } = 0;
        public string Version { get; set; } = "";   

        [ObservableProperty]
        private bool isApproved;
        public bool IsLocked { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime? ApprovedOn { get; set; }
        public List<ProjectDocumentFile> Files { get; set; } = new();

        [ObservableProperty]
        private bool allowMultiple;   // 🔥 KEY FEATURE
        public string Category { get; set; } = ""; // e.g. "Inspection", "Quality", "Drawing"
    }
}