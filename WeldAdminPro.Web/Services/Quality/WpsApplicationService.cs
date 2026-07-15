using WeldAdminPro.Core.Quality;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Web.Services.Quality;

public class WpsApplicationService
{
    private readonly WpsRepository _repository;

    public WpsApplicationService(WpsRepository repository)
    {
        _repository = repository;
    }

    // =====================================================
    // Queries
    // =====================================================

    public List<Wps> GetAll()
    {
        return _repository.GetAll();
    }

    public List<Wps> GetActive()
    {
        return _repository.GetActive();
    }

    public Wps? Get(string wpsNumber)
    {
        return _repository.GetByWpsNumber(wpsNumber);
    }

    // =====================================================
    // Commands
    // =====================================================

    public void Save(Wps wps)
    {
        var existing = _repository.GetByWpsNumber(wps.WpsNumber);

        if (existing == null)
        {
            _repository.Add(wps);
        }
        else
        {
            wps.Id = existing.Id;
            _repository.Update(wps);
        }
    }

    public void Delete(Guid id)
    {
        _repository.Delete(id);
    }
}