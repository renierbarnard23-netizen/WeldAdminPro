using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Core.Quality.Services;
using WeldAdminPro.Data;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.ViewModels.Quality
{
    public partial class TurnoverGovernanceViewModel
        : ObservableObject
    {
        private readonly WeldRepository
            _weldRepository;

        private readonly TurnoverGovernanceService
            _service = new();

        [ObservableProperty]
        private ObservableCollection<TurnoverReadinessRecord>
            records = new();

        [ObservableProperty]
        private int totalWelds;

        [ObservableProperty]
        private int readyForTurnover;

        [ObservableProperty]
        private int blockedWelds;

        public TurnoverGovernanceViewModel()
        {
            _weldRepository =
                new WeldRepository(
                    DatabasePath.GetConnectionString());

            Load();
        }

        public void Load()
        {
            Records.Clear();

            var welds =
                _weldRepository.GetAll();

            foreach (var weld in welds)
            {
                var result =
                    _service.Analyze(
                        weld,
                        ndtAccepted: weld.NdtStatus == "Accepted",
                        noOpenRepairs: weld.RepairCount == 0,
                        noOpenNcrs: true,
                        holdPointsApproved: true,
                        documentsAttached: true);

                Records.Add(result);
            }

            TotalWelds =
                Records.Count;

            ReadyForTurnover =
                Records.Count(x =>
                    x.TurnoverReady);

            BlockedWelds =
                Records.Count(x =>
                    !x.TurnoverReady);
        }
    }
}