using WeldAdminPro.Core.Quality;

namespace WeldAdminPro.Core.Interfaces
{
    public interface IWpsRepository
    {
        List<Wps> GetAll();

        Wps? GetByWpsNumber(string wpsNumber);

        void Add(Wps wps);

        void Update(Wps wps);

        void Delete(Guid id);

        void DeactivatePrevious(string wpsNumber);

        int GetNextRevision(string wpsNumber);
    }
}