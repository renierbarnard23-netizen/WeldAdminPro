using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Core.Quality.Services;
using WeldAdminPro.Data;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.ViewModels
{
    public partial class WeldTraceabilityViewModel
        : ObservableObject
    {
        public ObservableCollection<WeldTraceabilityRow>
            Welds
        { get; }
            = new();

        public WeldTraceabilityViewModel()
        {
            Load();
        }

        private void Load()
        {
            var databasePath =
                $"Data Source={DatabasePath.Get()}";

            var weldRepository =
                new WeldRepository(
                    databasePath);

            var ndtRepository =
                new WeldNdtRepository(
                    databasePath);

            List<Weld> welds =
                weldRepository.GetAll();

            List<WeldNdtResult> ndtResults =
                ndtRepository.GetAll();

            var service =
                new WeldTraceabilityService();

            var rows =
                service.Build(
                    welds,
                    ndtResults);

            Welds.Clear();

            foreach (var row in rows)
            {
                Welds.Add(row);
            }
        }
    }
}
