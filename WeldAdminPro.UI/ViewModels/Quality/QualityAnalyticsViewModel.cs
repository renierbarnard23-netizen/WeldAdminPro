using CommunityToolkit.Mvvm.ComponentModel;
using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Core.Quality.Services;
using WeldAdminPro.Data;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.ViewModels.Quality
{
    public partial class QualityAnalyticsViewModel
        : ObservableObject
    {
        private readonly WeldRepository
            _weldRepository;

        private readonly NcrRepository
            _ncrRepository;

        private readonly CapaRepository
            _capaRepository;

        private readonly QualityAnalyticsService
            _analyticsService = new();

        [ObservableProperty]
        private QualityKpiModel kpis
            = new();

        public QualityAnalyticsViewModel()
        {
            _weldRepository =
                new WeldRepository(
                    DatabasePath.GetConnectionString());

            _ncrRepository =
                new NcrRepository(
                    DatabasePath.GetConnectionString());

            _capaRepository =
                new CapaRepository(
                    DatabasePath.GetConnectionString());

            Load();
        }

        private void Load()
        {
            var welds =
                _weldRepository.GetAll();

            var ncrs =
                _ncrRepository.GetAll();

            var capas =
                _capaRepository.GetAll();

            Kpis =
                _analyticsService.Generate(
                    welds,
                    ncrs,
                    capas);
        }
    }
}
