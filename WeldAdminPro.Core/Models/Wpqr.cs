using System;

namespace WeldAdminPro.Core.Models
{
    /// <summary>
    /// Weld Procedure Qualification Record.
    ///
    /// Represents the qualification record linking one welder qualification
    /// to one approved Welding Procedure Specification (WPS).
    ///
    /// Multiple WPQR records may reference the same WPS.
    /// A WPQR does not link welders to each other.
    /// </summary>
    public class Wpqr
    {
        public Guid Id { get; set; }

/// <summary>
/// Company that owns this WPQR.
/// Ownership is enforced by the application service.
/// </summary>
public Guid? CompanyId { get; set; }

        /// <summary>
        /// Controlled WPQR document/record number.
        /// </summary>
        public string WpqrNumber { get; set; } = "";

        /// <summary>
        /// The WPS to which this welder qualification applies.
        /// </summary>
        public Guid? WpsId { get; set; }

        /// <summary>
        /// The existing welder qualification represented by this WPQR.
        /// </summary>
        public int? WelderQualificationId { get; set; }

        /// <summary>
        /// Date on which this WPQR qualification/update was recorded.
        /// </summary>
        public DateTime QualificationDate { get; set; }

        /// <summary>
        /// Date of the latest qualification/update.
        /// </summary>
        public DateTime? RenewalDate { get; set; }

        /// <summary>
        /// Qualification expiry date.
        /// </summary>
        public DateTime ExpiryDate { get; set; }

        /// <summary>
        /// Whether this WPQR is currently active.
        /// </summary>
        public bool IsActive { get; set; } = true;

        // ============================================================
        // AIA APPROVAL
        // ============================================================

        /// <summary>
        /// Whether the WPQR has been approved.
        /// </summary>
        public bool IsApproved { get; set; }

        /// <summary>
        /// Name/user identifier of the approving AIA.
        /// </summary>
        public string? ApprovedBy { get; set; }

        /// <summary>
        /// Date/time on which the AIA approved the WPQR.
        /// </summary>
        public DateTime? ApprovedOn { get; set; }

        /// <summary>
        /// Role of the approving person.
        /// </summary>
        public string? ApprovalRole { get; set; }

        /// <summary>
        /// Stored signature reference.
        /// </summary>
        public string? SignaturePath { get; set; }

        /// <summary>
        /// Stored professional stamp reference.
        /// </summary>
        public string? StampPath { get; set; }

        // ============================================================
        // ORIGINAL DOCUMENT
        // ============================================================

        /// <summary>
        /// Path/reference to the original imported WPQR PDF.
        /// </summary>
        public string? OriginalDocumentPath { get; set; }

        // ============================================================
        // AUDIT
        // ============================================================

        // ============================================================
        // REVISION CONTROL
        // ============================================================

        /// <summary>
        /// ID of the preceding WPQR revision, if this is a revision.
        /// </summary>
        public Guid? PreviousRevisionId { get; set; }

        /// <summary>
        /// Revision number. Existing records default to revision zero
        /// until a controlled data backfill is implemented.
        /// </summary>
        public int Revision { get; set; }

        /// <summary>
        /// Prevents ordinary edits to a finalized WPQR.
        /// Enforcement is handled by the application service.
        /// </summary>
        public bool IsLocked { get; set; }

        // ============================================================
        // AUDIT
        // ============================================================

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public string CreatedBy { get; set; } = "";

        /// <summary>
        /// Creates a new WPQR revision while preserving qualification
        /// data and clearing approval and locking state.
        /// </summary>
        public Wpqr CreateRevision(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException(
                    "A username is required to create a WPQR revision.",
                    nameof(username));

            return new Wpqr
            {
                Id = Guid.NewGuid(),
                CompanyId = CompanyId,
                WpqrNumber = WpqrNumber,
                PreviousRevisionId = Id,
                Revision = Revision + 1,
                IsLocked = false,

                WpsId = WpsId,
                WelderQualificationId = WelderQualificationId,
                QualificationDate = QualificationDate,
                RenewalDate = RenewalDate,
                ExpiryDate = ExpiryDate,
                IsActive = true,

                IsApproved = false,
                ApprovedBy = null,
                ApprovedOn = null,
                ApprovalRole = null,

                SignaturePath = null,
                StampPath = null,
                OriginalDocumentPath = OriginalDocumentPath,

                CreatedOn = DateTime.UtcNow,
                CreatedBy = username.Trim()
            };
        }
    }
}


