using WeldAdminPro.Core.Quality;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Web.Services.Quality;

public class WelderQualificationApplicationService
{
    private readonly WelderQualificationRepository _repository;

    public WelderQualificationApplicationService(
        WelderQualificationRepository repository)
    {
        _repository = repository;
    }

    public List<WelderQualification> GetAll()
    {
        return _repository.GetAll();
    }

    public int GetValidQualifiedWelderCount()
    {
        return _repository
            .GetAll()
            .Where(x => x.ExpiryDate >= DateTime.Today)
            .Select(x => x.WelderNumber.Trim().ToUpper())
            .Distinct()
            .Count();
    }

    public int GetExpiringQualificationCount(int days = 30)
    {
        var today = DateTime.Today;
        var expiryLimit = today.AddDays(days);

        return _repository
            .GetAll()
            .Count(x =>
                x.ExpiryDate >= today &&
                x.ExpiryDate <= expiryLimit);
    }
}