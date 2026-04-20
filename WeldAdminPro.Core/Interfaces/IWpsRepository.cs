using WeldAdminPro.Core.Quality;

namespace WeldAdminPro.Core.Interfaces
{
    public interface IWpsRepository
    {
        void Add(Wps wps);
        void Update(Wps wps);
        Wps? GetByWpsNumber(string wpsNumber);
    }
}