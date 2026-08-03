using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Web.Services.Quality;

public class NdtApplicationService
{
    private readonly WeldNdtRepository _repository;

    public NdtApplicationService(WeldNdtRepository repository)
    {
        _repository = repository;
    }

    public int GetPendingCount()
    {
        return _repository
            .GetAll()
            .Count(x => x.Result == NdtResultType.Pending);
    }

    public int GetAcceptedCount()
    {
        return _repository
            .GetAll()
            .Count(x => x.Result == NdtResultType.Accept);
    }

    public int GetRejectedCount()
    {
        return _repository
            .GetAll()
            .Count(x => x.Result == NdtResultType.Reject);
    }

    public int GetRepairCount()
    {
        return _repository
            .GetAll()
            .Count(x => x.Result == NdtResultType.Repair);
    }

    public int GetConditionalAcceptCount()
    {
        return _repository
            .GetAll()
            .Count(x => x.Result == NdtResultType.ConditionalAccept);
    }
}