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
    public partial class WeldTraceabilityEditorViewModel
        : ObservableObject
    {
        private readonly WeldTraceabilityRepository
            _repository;

        [ObservableProperty]
        private ObservableCollection<WeldTraceabilityRecord>
            records = new();

        [ObservableProperty]
        private WeldTraceabilityRecord? selectedRecord;

        private readonly Guid _weldId;

        public WeldTraceabilityEditorViewModel(
            Guid weldId)
        {
            _weldId = weldId;

            _repository =
                new WeldTraceabilityRepository(
                    DatabasePath.GetConnectionString());

            Load();
        }

        private void Load()
        {
            Records.Clear();

            var items =
                _repository.GetByWeld(
                    _weldId);

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

            Load();
        }
    }
}