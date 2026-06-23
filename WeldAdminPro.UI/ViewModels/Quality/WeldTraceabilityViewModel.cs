using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Data;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.ViewModels.Quality
{
    public partial class WeldTraceabilityViewModel
        : ObservableObject
    {
        private readonly WeldTraceabilityRepository
            _repository;

        [ObservableProperty]
        private ObservableCollection<WeldTraceabilityRecord>
            records = new();

        [ObservableProperty]
        private WeldTraceabilityRecord? selectedRecord;

        public WeldTraceabilityViewModel(
            Guid weldId)
        {
            _repository =
                new WeldTraceabilityRepository(
                    DatabasePath.GetConnectionString());

            Load(weldId);
        }

        private void Load(Guid weldId)
        {
            Records.Clear();

            var items =
                _repository.GetByWeld(
                    weldId);

            foreach (var item in items)
            {
                Records.Add(item);
            }
        }

        [RelayCommand]
        private void Save()
        {
            if (SelectedRecord == null)
                return;

            _repository.Update(
                SelectedRecord);

            MessageBox.Show(
                "Traceability updated.");
        }
    }
}