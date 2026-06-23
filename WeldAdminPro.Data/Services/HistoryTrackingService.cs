using System;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Core.Quality.Services;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
    public class HistoryTrackingService
    : IHistoryTrackingService
    {
        private readonly WeldHistoryRepository
        _historyRepository;

    public HistoryTrackingService(
        string connectionString)
        {
            _historyRepository =
                new WeldHistoryRepository(
                    connectionString);
        }

        public void Track(
            Weld weld,
            string eventType,
            string description)
        {
            var entry =
                new WeldHistoryEntry
                {
                    Id = Guid.NewGuid(),

                    WeldId = weld.Id,

                    EventDate =
                        DateTime.UtcNow,

                    EventType =
                        eventType,

                    Description =
                        description,

                    UserName =
                        Environment.UserName
                };

            _historyRepository.Add(entry);
        }
    }

}
