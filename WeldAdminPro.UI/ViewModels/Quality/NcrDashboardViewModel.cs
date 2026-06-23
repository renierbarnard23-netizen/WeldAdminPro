using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Data;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.ViewModels.Quality
{
    public partial class NcrDashboardViewModel
        : ObservableObject
    {
        private readonly NcrRepository
            _repository;

        [ObservableProperty]
        private ObservableCollection<NcrRecord>
            ncrs = new();

        [ObservableProperty]
        private NcrRecord? selectedNcr;

        [ObservableProperty]
        private int openNcrs;

        [ObservableProperty]
        private int closedNcrs;

        [ObservableProperty]
        private int overdueNcrs;

        public NcrDashboardViewModel()
        {
            _repository =
                new NcrRepository(
                    DatabasePath.GetConnectionString());

            Load();
        }

        public void Load()
        {
            Ncrs.Clear();

            var all =
                _repository.GetAll();

            foreach (var item in all)
            {
                Ncrs.Add(item);
            }

            OpenNcrs =
                Ncrs.Count(x =>
                    !x.IsClosed);

            ClosedNcrs =
                Ncrs.Count(x =>
                    x.IsClosed);

            OverdueNcrs =
                Ncrs.Count(x =>
                    !x.IsClosed
                    && x.DueDate != null
                    && x.DueDate < DateTime.UtcNow);
        }

        [RelayCommand]
        private void Refresh()
        {
            Load();
        }

        [RelayCommand]
        private void CloseNcr()
        {
            if (SelectedNcr == null)
                return;

            SelectedNcr.IsClosed = true;

            SelectedNcr.Status =
                NcrStatus.Closed;

            SelectedNcr.ClosedDate =
                DateTime.UtcNow;

            SelectedNcr.ClosedBy =
                Environment.UserName;

            _repository.Update(
                SelectedNcr);

            Load();

            MessageBox.Show(
                "NCR closed.");
        }
    }
}