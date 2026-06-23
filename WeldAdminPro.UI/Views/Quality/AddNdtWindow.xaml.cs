using System;
using System.Windows;
using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.UI.ViewModels.Quality;

namespace WeldAdminPro.UI.Views.Quality
{
    public partial class AddNdtWindow : Window
    {
        private readonly WeldNdtRepository _repository;

    public NdtEntryViewModel ViewModel { get; }

        public WeldNdtResult? SavedResult
        {
            get;
            private set;
        }

        public AddNdtWindow(
            string connectionString,
            Guid weldId)
        {
            InitializeComponent();

            _repository = new WeldNdtRepository(
                connectionString);

            ViewModel = new NdtEntryViewModel
            {
                WeldId = weldId
            };

            DataContext = ViewModel;
        }

        private void Save_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                var model = ViewModel.BuildModel();

                _repository.Add(model);

                SavedResult = model;

                DialogResult = true;

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to save NDT result:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(
            object sender,
            RoutedEventArgs e)
        {
            DialogResult = false;

            Close();
        }
    }
}
