using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Core.Quality.Services;
using WeldAdminPro.Data;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.UI.Views.Quality;
using WeldAdminPro.Core.Services;

namespace WeldAdminPro.UI.ViewModels.Quality
{
    public partial class HoldPointViewModel
        : ObservableObject
    {
        private readonly HoldPointRepository
            _repository;

        private readonly HoldPointWorkflowService
            _workflowService;

        [ObservableProperty]
        private ObservableCollection<WeldHoldPoint>
            holdPoints = new();

        [ObservableProperty]
        private WeldHoldPoint? selectedHoldPoint;

        public HoldPointViewModel()
        {
            _repository =
                new HoldPointRepository(
                    DatabasePath.GetConnectionString());

            _workflowService =
                new HoldPointWorkflowService();
        }

        public void Load(Guid weldId)
        {
            var items =
                _repository.GetByWeld(
                    weldId);

            HoldPoints =
                new ObservableCollection<WeldHoldPoint>(
                    items);

            MessageBox.Show(
                $"Loaded {items.Count} hold points");
        }

        [RelayCommand]
        private void Approve()
        {
            if (SelectedHoldPoint == null)
                return;

            try
            {
                _workflowService.Approve(
                    SelectedHoldPoint,
                    CurrentUserContext.Username,
                    "Approved");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message);

                return;
            }

            _repository.Update(
                SelectedHoldPoint);

            Load(SelectedHoldPoint.WeldId);

            MessageBox.Show(
                "Hold point approved.");
        }

        [RelayCommand]
        private void Reject()
        {
            if (SelectedHoldPoint == null)
                return;

            _workflowService.Reject(
                SelectedHoldPoint,
                "Rejected");

            _repository.Update(
                SelectedHoldPoint);

            Load(SelectedHoldPoint.WeldId);

            MessageBox.Show(
                "Hold point rejected.");
        }

        [RelayCommand]
        private void AddHoldPoint()
        {
            if (!HoldPoints.Any())
                return;

            var weldId =
                HoldPoints.First().WeldId;

            var vm =
                new AddHoldPointViewModel(
                    weldId);

            var window =
                new AddHoldPointWindow
                {
                    DataContext = vm
                };

            window.ShowDialog();

            if (vm.Result == null)
                return;

            _repository.Add(vm.Result);

            HoldPoints.Add(vm.Result);

            MessageBox.Show(
                "Hold point added.");
        }

        [RelayCommand]
        private void RemoveHoldPoint()
        {
            if (SelectedHoldPoint == null)
                return;

            HoldPoints.Remove(
                SelectedHoldPoint);

            MessageBox.Show(
                "Hold point removed from view.");
        }
    }
}