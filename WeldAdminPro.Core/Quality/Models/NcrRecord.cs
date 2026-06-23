using System;
using WeldAdminPro.Core.Quality.Enums;

namespace WeldAdminPro.Core.Quality.Models
{
    public class NcrRecord
    {
        public Guid Id
        {
            get;
            set;
        }

        public Guid WeldId
        {
            get;
            set;
        }

        public string WeldNumber
        {
            get;
            set;
        } = string.Empty;

        public string NcrNumber
        {
            get;
            set;
        } = "";

        public string Description
        {
            get;
            set;
        } = string.Empty;

        public string RootCause
        {
            get;
            set;
        } = string.Empty;

        public string CorrectiveAction
        {
            get;
            set;
        } = string.Empty;

        public string PreventiveAction
        {
            get;
            set;
        } = string.Empty;

        public string RaisedBy
        {
            get;
            set;
        } = string.Empty;

        public DateTime RaisedDate
        {
            get;
            set;
        }

        public string AssignedTo
        {
            get;
            set;
        } = string.Empty;

        public DateTime? DueDate
        {
            get;
            set;
        }

        public NcrStatus Status
        {
            get;
            set;
        }

        public bool IsClosed
        {
            get;
            set;
        }

        public string ClosedBy
        {
            get;
            set;
        } = string.Empty;

        public DateTime? ClosedDate
        {
            get;
            set;
        }

        public NcrDispositionType? DispositionType { get; set; }

        public string? DispositionApprovedBy { get; set; }

        public DateTime? DispositionApprovedDate { get; set; }

        public string? VerificationBy { get; set; }

        public DateTime? VerificationDate { get; set; }

        public bool RequiresCustomerApproval { get; set; }

        public bool CustomerApproved { get; set; }

        public string? CustomerApprovalReference { get; set; }

    }
}