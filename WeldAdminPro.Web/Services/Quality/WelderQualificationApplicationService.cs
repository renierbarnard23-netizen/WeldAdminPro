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

    public void Create(WelderQualification qualification)
    {
        Validate(qualification);

        if (qualification.InitialQualificationDate == DateTime.MinValue)
        {
            qualification.InitialQualificationDate =
                qualification.QualificationDate;
        }

        _repository.Add(qualification);
    }

    public void Update(WelderQualification qualification)
    {
        Validate(qualification);

        _repository.Update(qualification);
    }

    public void Delete(int id)
    {
        _repository.Delete(id);
    }

    public void Renew(
        WelderQualification qualification,
        int months = 6)
    {
        qualification.RenewalDate = DateTime.Today;
        qualification.ExpiryDate =
            DateTime.Today.AddMonths(months);
        qualification.IsActive = true;

        _repository.Update(qualification);
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

    private static void Validate(
        WelderQualification qualification)
    {
        if (string.IsNullOrWhiteSpace(
            qualification.WelderNumber))
        {
            throw new InvalidOperationException(
                "Welder number is required.");
        }

        if (string.IsNullOrWhiteSpace(
            qualification.Process))
        {
            throw new InvalidOperationException(
                "Welding process is required.");
        }

        if (string.IsNullOrWhiteSpace(
            qualification.Position))
        {
            throw new InvalidOperationException(
                "Welding position is required.");
        }

        if (qualification.ExpiryDate <=
            qualification.QualificationDate)
        {
            throw new InvalidOperationException(
                "Expiry date must be after qualification date.");
        }

        if (qualification.ThicknessMin < 0)
        {
            throw new InvalidOperationException(
                "Minimum thickness cannot be negative.");
        }

        if (qualification.ThicknessMax <
            qualification.ThicknessMin)
        {
            throw new InvalidOperationException(
                "Maximum thickness cannot be less than minimum thickness.");
        }
    }
}
