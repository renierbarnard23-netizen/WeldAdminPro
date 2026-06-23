using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentFormat.OpenXml.Vml.Office;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Data;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.ViewModels.Quality
{
    public partial class QcpRuleManagementViewModel
        : ObservableObject
    {
        private readonly QcpInspectionRuleRepository
            _repository;

        [ObservableProperty]
        private ObservableCollection<QcpInspectionRule>
            rules = new();

        [ObservableProperty]
        private QcpInspectionRule? selectedRule;

        public QcpRuleManagementViewModel()
        {
            _repository =
                new QcpInspectionRuleRepository(
                    DatabasePath.GetConnectionString());

            Load();
        }

        private void Load()
        {
            Rules.Clear();

            var all =
                _repository.GetAll();

            foreach (var item in all)
            {
                Rules.Add(item);
            }
        }

        [RelayCommand]
        private void AddRule()
        {
            var rule =
                new QcpInspectionRule
                {
                    Id = Guid.NewGuid(),

                    WeldType =
                        "PressureButt",

                    InspectionPercentage =
                        10,

                    RequiresClientWitness =
                        true,

                    RequiresHoldPoint =
                        true
                };

            _repository.Add(rule);

            Load();

            MessageBox.Show(
                "QCP rule added.");
        }

        [RelayCommand]
        private void SaveRule()
        {
            if (SelectedRule == null)
                return;

            _repository.Update(
                SelectedRule);

            MessageBox.Show(
                "QCP rule saved.");
        }
    }
}