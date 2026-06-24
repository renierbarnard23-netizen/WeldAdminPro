using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Core.Quality.Services
{
    public interface IWeldReadinessEngine
    {
        WeldReadinessResult Evaluate(Weld weld);
    }
}