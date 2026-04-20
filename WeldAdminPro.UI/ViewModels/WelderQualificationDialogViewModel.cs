using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.Windows;
using WeldAdminPro.Core.Quality;

namespace WeldAdminPro.UI.ViewModels
{
    public partial class WelderQualificationDialogViewModel : ObservableObject
    {
        public Action? CloseAction { get; set; }

        [ObservableProperty] private string welderName = "";
        [ObservableProperty] private string process = "";
        [ObservableProperty] private string position = "";
        [ObservableProperty] private DateTime qualificationDate = DateTime.Today;
        [ObservableProperty] private DateTime expiryDate = DateTime.Today.AddYears(2);

        public WelderQualification? Result { get; private set; }

        [RelayCommand]
        private void Save()
        {
            // 🔥 BASIC VALIDATION
            if (string.IsNullOrWhiteSpace(WelderName))
            {
                MessageBox.Show("Welder name required");
                return;
            }

            if (ExpiryDate <= QualificationDate)
            {
                MessageBox.Show("Expiry must be after qualification date");
                return;
            }

            Result = new WelderQualification
            {
                WelderName = WelderName,
                Process = Process,
                Position = Position,
                QualificationDate = QualificationDate,
                ExpiryDate = ExpiryDate
            };

            CloseAction?.Invoke();
        }

        [RelayCommand]
        private void Cancel()
        {
            CloseAction?.Invoke();
        }        
    }
}