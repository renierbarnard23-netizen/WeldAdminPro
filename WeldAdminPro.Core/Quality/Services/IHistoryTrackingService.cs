using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Core.Quality.Services
{
    public interface IHistoryTrackingService
    {
        void Track(
        Weld weld,
        string eventType,
        string description);
    }
}
