using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using WeldAdminPro.Core.Quality;
using WeldAdminPro.Core.Quality.Services;
using WeldAdminPro.Data;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.ViewModels
{
    public partial class WelderQualificationViewModel : ObservableObject
    {
        private readonly WelderQualificationRepository _repo;
        private readonly WelderQualificationService _service;

        [ObservableProperty]
        private ObservableCollection<WelderQualification> welders = new();

        [ObservableProperty]
        private WelderQualification? selectedWelder;

        public WelderQualificationViewModel()
        {
            _repo = new WelderQualificationRepository(); // ✅ FIXED
            _service = new WelderQualificationService();

            Load();
        }

        public void Load()
        {
            Welders.Clear();

            var data = _repo.GetAll();

            Debug.WriteLine($"WELDERS COUNT: {data.Count}");

            Welders.Clear();

            foreach (var w in data)
                Welders.Add(w);

            foreach (var w in _repo.GetAll())
                Welders.Add(w);
        }

        public string GetStatusIcon(WelderQualification w)
        {
            return _service.GetStatus(w) switch
            {
                QualificationStatus.Valid => "✔",
                QualificationStatus.ExpiringSoon => "⚠",
                QualificationStatus.Expired => "❌",
                _ => ""
            };
        }

        [RelayCommand]
        private void AddWelder()
        {
            var dialog = new Views.Quality.WelderQualificationDialog();
            var vm = new WelderQualificationDialogViewModel();

            dialog.DataContext = vm;
            vm.CloseAction = dialog.Close;

            dialog.ShowDialog();

            if (vm.Result != null)
            {
                _repo.Add(vm.Result);
                Load();
            }
        }

        [RelayCommand]
        private void EditWelder()
        {
            if (SelectedWelder == null) return;

            var vm = new WelderQualificationDialogViewModel
            {
                WelderName = SelectedWelder.WelderName,
                Process = SelectedWelder.Process,
                Position = SelectedWelder.Position,
                QualificationDate = SelectedWelder.QualificationDate,
                ExpiryDate = SelectedWelder.ExpiryDate
            };

            var dialog = new Views.Quality.WelderQualificationDialog
            {
                DataContext = vm
            };

            vm.CloseAction = dialog.Close;

            dialog.ShowDialog();

            if (vm.Result != null)
            {
                vm.Result.Id = SelectedWelder.Id;
                _repo.Update(vm.Result);
                Load();
            }
        }
    }
}