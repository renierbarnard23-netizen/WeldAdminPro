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
    public partial class TurnoverPackageViewModel
        : ObservableObject
    {
        private readonly TurnoverPackageRepository
            _repository;

        [ObservableProperty]
        private ObservableCollection<TurnoverPackageRecord>
            packages = new();

        [ObservableProperty]
        private TurnoverPackageRecord? selectedPackage;

        public TurnoverPackageViewModel()
        {
            _repository =
                new TurnoverPackageRepository(
                    DatabasePath.GetConnectionString());

            Load();
        }

        private void Load()
        {
            Packages.Clear();

            var all =
                _repository.GetAll();

            foreach (var item in all)
            {
                Packages.Add(item);
            }
        }

        [RelayCommand]
        private void CreatePackage()
        {
            var package =
                new TurnoverPackageRecord
                {
                    Id = Guid.NewGuid(),

                    PackageNumber =
                        $"TOP-{DateTime.Now:yyyyMMddHHmmss}",

                    CreatedDate =
                        DateTime.UtcNow,

                    CreatedBy =
                        Environment.UserName,

                    IsApproved = false
                };

            _repository.Add(
                package);

            Load();

            MessageBox.Show(
                "Turnover package created.");
        }

        [RelayCommand]
        private void ApprovePackage()
        {
            if (SelectedPackage == null)
                return;

            SelectedPackage.IsApproved = true;

            SelectedPackage.ApprovedBy =
                Environment.UserName;

            SelectedPackage.ApprovedDate =
                DateTime.UtcNow;

            _repository.Update(
                SelectedPackage);

            Load();

            MessageBox.Show(
                "Turnover package approved.");
        }
    }
}