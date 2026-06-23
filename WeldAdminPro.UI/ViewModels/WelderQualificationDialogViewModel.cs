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

        [ObservableProperty] private string welderNumber = "";
        [ObservableProperty] private string process = "";
        [ObservableProperty] private string position = "";
        [ObservableProperty] private DateTime qualificationDate = DateTime.Today;
        [ObservableProperty] private DateTime expiryDate = DateTime.Today.AddYears(2);
        [ObservableProperty] private string materialGroup = "";
        [ObservableProperty] private double thicknessMin;
        [ObservableProperty] private double thicknessMax;

        public WelderQualification? Result { get; private set; }

        

        partial void OnQualificationDateChanged(DateTime value)
        {
            ExpiryDate = value.AddMonths(6);
        }

        [RelayCommand]
        private void Save()
        {
            // 🔥 BASIC VALIDATION
            if (string.IsNullOrWhiteSpace(WelderNumber))
            {
                MessageBox.Show("Welder number required");
                return;
            }

            if (ExpiryDate <= QualificationDate)
            {
                MessageBox.Show("Expiry must be after qualification date");
                return;
            }

            Result = new WelderQualification
            {
                WelderNumber = WelderNumber,
                Process = Process,
                Position = Position,

                QualificationDate = QualificationDate,
                InitialQualificationDate = QualificationDate,

                ExpiryDate = ExpiryDate,

                MaterialGroup = MaterialGroup,

                ThicknessMin = ThicknessMin,
                ThicknessMax = ThicknessMax
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