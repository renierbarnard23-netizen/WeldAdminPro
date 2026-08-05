using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Core.Quality.Services;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Web.Services.Quality;

public class NcrApplicationService
{
    private readonly NcrRepository _repository;

    private readonly NcrWorkflowHistoryRepository
        _historyRepository;

    public NcrApplicationService(
        NcrRepository repository,
        NcrWorkflowHistoryRepository historyRepository)
    {
        _repository = repository;
        _historyRepository = historyRepository;
    }

    // =====================================================
    // Queries
    // =====================================================

    public List<NcrRecord> GetAll()
    {
        return _repository.GetAll();
    }

    public List<NcrRecord> GetByWeld(
        Guid weldId)
    {
        return _repository.GetByWeld(weldId);
    }

    public NcrRecord? GetById(
        Guid id)
    {
        return _repository
            .GetAll()
            .FirstOrDefault(x => x.Id == id);
    }

    public List<NcrWorkflowHistoryEntry> GetHistory(
        Guid ncrId)
    {
        return _historyRepository.GetByNcr(ncrId);
    }

    // =====================================================
    // Commands
    // =====================================================

    public void Create(
        NcrRecord ncr)
    {
        if (ncr.Id == Guid.Empty)
        {
            ncr.Id = Guid.NewGuid();
        }

        if (ncr.RaisedDate == default)
        {
            ncr.RaisedDate = DateTime.Now;
        }

        ncr.Status = NcrStatus.Open;
        ncr.IsClosed = false;
        ncr.ClosedBy = string.Empty;
        ncr.ClosedDate = null;

        _repository.Add(ncr);

        AddHistory(
            ncr.Id,
            null,
            ncr.Status,
            "Created",
            ncr.RaisedBy,
            BuildCreatedDetails(ncr));
    }

    public void Update(
        NcrRecord ncr)
    {
        _repository.Update(ncr);
    }

    public bool CanMoveTo(
        NcrRecord ncr,
        NcrStatus target)
    {
        return NcrWorkflowService.CanMoveTo(
            ncr.Status,
            target);
    }

    public bool MoveTo(
        Guid id,
        NcrStatus target)
    {
        var ncr = GetById(id);

        if (ncr == null)
        {
            return false;
        }

        if (!NcrWorkflowService.CanMoveTo(
                ncr.Status,
                target))
        {
            return false;
        }

        var previousStatus =
            ncr.Status;

        ncr.Status = target;

        _repository.Update(ncr);

        AddHistory(
            ncr.Id,
            previousStatus,
            target,
            "Status Changed",
            ResolveActor(ncr),
            $"Status changed from {previousStatus} to {target}.");

        return true;
    }

    public bool SetDisposition(
        Guid id,
        NcrDispositionType disposition,
        string approvedBy,
        bool requiresCustomerApproval = false,
        bool customerApproved = false,
        string? customerApprovalReference = null)
    {
        var ncr = GetById(id);

        if (ncr == null)
        {
            return false;
        }

        ncr.DispositionType = disposition;
        ncr.DispositionApprovedBy = approvedBy;
        ncr.DispositionApprovedDate = DateTime.Now;
        ncr.RequiresCustomerApproval =
            requiresCustomerApproval;
        ncr.CustomerApproved =
            customerApproved;
        ncr.CustomerApprovalReference =
            customerApprovalReference;

        _repository.Update(ncr);

        var details =
            $"Disposition: {disposition}. " +
            $"Customer approval required: " +
            $"{(requiresCustomerApproval ? "Yes" : "No")}.";

        if (requiresCustomerApproval)
        {
            details +=
                $" Customer approved: " +
                $"{(customerApproved ? "Yes" : "No")}.";

            if (!string.IsNullOrWhiteSpace(
                    customerApprovalReference))
            {
                details +=
                    $" Approval reference: " +
                    $"{customerApprovalReference}.";
            }
        }

        AddHistory(
            ncr.Id,
            ncr.Status,
            ncr.Status,
            "Disposition Recorded",
            approvedBy,
            details);

        return true;
    }

    public bool RecordVerification(
        Guid id,
        string verifiedBy)
    {
        var ncr = GetById(id);

        if (ncr == null)
        {
            return false;
        }

        ncr.VerificationBy = verifiedBy;
        ncr.VerificationDate = DateTime.Now;

        _repository.Update(ncr);

        AddHistory(
            ncr.Id,
            ncr.Status,
            ncr.Status,
            "Verification Recorded",
            verifiedBy,
            $"Verification completed by {verifiedBy}.");

        return true;
    }

    public bool Close(
        Guid id,
        string closedBy)
    {
        var ncr = GetById(id);

        if (ncr == null)
        {
            return false;
        }

        if (!NcrWorkflowService.CanMoveTo(
                ncr.Status,
                NcrStatus.Closed))
        {
            return false;
        }

        if (ncr.RequiresCustomerApproval &&
            !ncr.CustomerApproved)
        {
            return false;
        }

        var previousStatus =
            ncr.Status;

        ncr.Status = NcrStatus.Closed;
        ncr.IsClosed = true;
        ncr.ClosedBy = closedBy;
        ncr.ClosedDate = DateTime.Now;

        _repository.Update(ncr);

        AddHistory(
            ncr.Id,
            previousStatus,
            NcrStatus.Closed,
            "Closed",
            closedBy,
            $"NCR closed by {closedBy}.");

        return true;
    }

    public string GetNextNcrNumber()
    {
        return _repository.GetNextNcrNumber();
    }

    // =====================================================
    // History
    // =====================================================

    private void AddHistory(
        Guid ncrId,
        NcrStatus? fromStatus,
        NcrStatus toStatus,
        string action,
        string? performedBy,
        string? details)
    {
        _historyRepository.Add(
            new NcrWorkflowHistoryEntry
            {
                Id = Guid.NewGuid(),
                NcrId = ncrId,
                FromStatus = fromStatus,
                ToStatus = toStatus,
                Action = action,
                PerformedBy =
                    performedBy ?? string.Empty,
                PerformedDate = DateTime.Now,
                Details =
                    details ?? string.Empty
            });
    }

    private static string ResolveActor(
        NcrRecord ncr)
    {
        if (!string.IsNullOrWhiteSpace(
                ncr.AssignedTo))
        {
            return ncr.AssignedTo;
        }

        if (!string.IsNullOrWhiteSpace(
                ncr.RaisedBy))
        {
            return ncr.RaisedBy;
        }

        return "System";
    }

    private static string BuildCreatedDetails(
        NcrRecord ncr)
    {
        var category =
            string.IsNullOrWhiteSpace(ncr.Category)
                ? "Not specified"
                : ncr.Category;

        var details =
            $"NCR {ncr.NcrNumber} created. " +
            $"Category: {category}. " +
            $"Welding related: " +
            $"{(ncr.IsWeldingRelated ? "Yes" : "No")}.";

        if (!string.IsNullOrWhiteSpace(
                ncr.CustomReason))
        {
            details +=
                $" Custom reason: {ncr.CustomReason}.";
        }

        if (ncr.WeldId.HasValue)
        {
            details +=
                $" Associated weld: {ncr.WeldNumber}.";
        }

        return details;
    }
}