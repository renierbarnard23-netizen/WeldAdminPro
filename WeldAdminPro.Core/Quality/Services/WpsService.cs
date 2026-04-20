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
            Console.WriteLine($"SAVING WPS: {wps.WpsNumber}");

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
    }
    }