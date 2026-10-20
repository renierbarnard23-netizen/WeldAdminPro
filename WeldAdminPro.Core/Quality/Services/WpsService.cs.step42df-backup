using WeldAdminPro.Core.Interfaces;

namespace WeldAdminPro.Core.Quality.Services
{
    public class WpsService
    {
        private readonly IWpsRepository _repo;

        public WpsService(IWpsRepository repo)
        {
            _repo = repo;
        }

        public void SaveWps(Wps wps)
        {
            #if DEBUG
            Console.WriteLine($"SAVING WPS: {wps.WpsNumber}");
            #endif

            var existing = _repo.GetByWpsNumber(wps.WpsNumber);

            if (existing == null)
            {
                _repo.Add(wps);   // INSERT
            }
            else
            {
                wps.Id = existing.Id; // preserve ID
                _repo.Update(wps);    // UPDATE
            }
        }
        public void SaveRevision(Wps wps)
        {
            var repo = _repo;

            // 🔒 lock old revision
            repo.DeactivatePrevious(wps.WpsNumber);

            // 🆕 assign new revision
            wps.Revision = repo.GetNextRevision(wps.WpsNumber);
            wps.IsActive = true;

            repo.Add(wps);
        }
    }
    }