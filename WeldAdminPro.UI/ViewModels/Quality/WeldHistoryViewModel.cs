using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Data;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.ViewModels.Quality
{
    public partial class WeldHistoryViewModel
    : ObservableObject
    {
        private readonly WeldHistoryRepository
        _repository;

    [ObservableProperty]
        private ObservableCollection<WeldHistoryEntry>
        historyEntries = new();

        public WeldHistoryViewModel(
            Guid weldId)
        {
            _repository =
                new WeldHistoryRepository(
                    DatabasePath.GetConnectionString());

            Load(weldId);
        }

        private void Load(
            Guid weldId)
        {
            var entries =
                _repository.GetByWeld(
                    weldId);

            HistoryEntries.Clear();

            foreach (var entry
                in entries)
            {
                HistoryEntries.Add(
                    entry);
            }
        }
    }
}
