using System;
using System.Windows;
using System.Windows.Controls;
using WeldAdminPro.UI.ViewModels;
using WeldAdminPro.UI.ViewModels.Quality;

namespace WeldAdminPro.UI.Views.Quality
{
    public partial class WeldTraceabilityEditorView
        : UserControl
    {
        public WeldTraceabilityEditorView(
            Guid weldId)
        {
            InitializeComponent();

            DataContext =
                new WeldTraceabilityEditorViewModel(
                    weldId);
        }

        private void OpenTraceability_Click(
    object sender,
    RoutedEventArgs e)
        {
            if (DataContext is not
                WeldRegisterViewModel vm)
            {
                return;
            }

            if (vm.SelectedWeld == null)
            {
                MessageBox.Show(
                    "Select a weld first.");

                return;
            }

            var window =
                new Window
                {
                    Title =
                        $"Traceability - " +
                        $"{vm.SelectedWeld.WeldNumber}",

                    Width = 1200,
                    Height = 700,

                    Content =
                        new WeldTraceabilityEditorView(
                            vm.SelectedWeld.Id)
                };

            window.ShowDialog();
        }
    }
}