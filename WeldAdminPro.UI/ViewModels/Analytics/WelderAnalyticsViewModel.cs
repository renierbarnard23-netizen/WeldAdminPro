using CommunityToolkit.Mvvm.ComponentModel;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Collections.ObjectModel;
using WeldAdminPro.Core.Analytics.Models;
using WeldAdminPro.Core.Analytics.Services;
using WeldAdminPro.Data;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.ViewModels.Analytics
{
    public partial class WelderAnalyticsViewModel
        : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<WelderPerformanceRecord>
            welders = new();

        public WelderAnalyticsViewModel()
        {
            Load();
        }

        private void Load()
        {
            var weldRepository =
                new WeldRepository(
                    DatabasePath.GetConnectionString());

            var ncrRepository =
                new NcrRepository(
                    DatabasePath.GetConnectionString());

            var service =
                new WelderAnalyticsService();

            var data =
                service.Generate(
                    weldRepository.GetAll(),
                    ncrRepository.GetAll());

            Welders =
                new ObservableCollection
                    <WelderPerformanceRecord>(
                        data);
        }
    }
}