using CommunityToolkit.Mvvm.ComponentModel;
using DocumentFormat.OpenXml.ExtendedProperties;
using System.Collections.ObjectModel;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Services;
using WeldAdminPro.Data;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.ViewModels
{
    public partial class WeldMapViewModel
        : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<WeldMapNode>
            welds = new();

        public WeldMapViewModel()
        {
            Load();
        }

        private void Load()
        {
            var repository =
                new WeldRepository(
                    DatabasePath.GetConnectionString());

            var mapService =
                new WeldMapService();

            var all =
                repository.GetAll();

            double x = 20;
            double y = 20;

            foreach (var weld in all)
            {
                Welds.Add(
                    new WeldMapNode
                    {
                        WeldId =
                            weld.Id,

                        WeldNumber =
                            weld.WeldNumber,

                        X = x,
                        Y = y,

                        StatusColor =
                            mapService.GetStatusColor(
                                weld)
                    });

                x += 120;

                if (x > 900)
                {
                    x = 20;
                    y += 80;
                }
            }
        }
    }
}