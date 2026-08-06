using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Core.Quality.Services;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Web.Services.Quality;

public class NcrApplicationService
{
    private readonly NcrRepository _repository;

    public NcrApplicationService(
        NcrRepository repository)
    {
        _repository = repository;
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

        ncr.Status = target;

        _repository.Update(ncr);

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

        ncr.Status = NcrStatus.Closed;
        ncr.IsClosed = true;
        ncr.ClosedBy = closedBy;
        ncr.ClosedDate = DateTime.Now;

        _repository.Update(ncr);

        return true;
    }
}
