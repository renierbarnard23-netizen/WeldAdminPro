using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Data;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Core.Quality.Enums;

namespace WeldAdminPro.UI.ViewModels.Quality
{
    public partial class CapaManagementViewModel
        : ObservableObject
    {
        private readonly CapaRepository
            _repository;

        [ObservableProperty]
        private ObservableCollection<CapaRecord>
            capas = new();

        [ObservableProperty]
        private CapaRecord? selectedCapa;

        public CapaManagementViewModel()
        {
            _repository =
                new CapaRepository(
                    DatabasePath.GetConnectionString());

            Load();
        }

        public void Load()
        {
            Capas.Clear();

            var all =
                _repository.GetAll();

            foreach (var item in all)
            {
                Capas.Add(item);
            }
        }

        [RelayCommand]
        private void VerifyCapa()
        {
            if (SelectedCapa == null)
                return;

            SelectedCapa.Status =
                Core.Quality.Enums.CapaStatus.Verified;

            SelectedCapa.IsEffective = true;

            SelectedCapa.VerifiedBy =
                Environment.UserName;

            SelectedCapa.VerifiedDate =
                DateTime.UtcNow;

            _repository.Update(
                SelectedCapa);

            Load();

            MessageBox.Show(
                "CAPA verified.");
        }
    }
}