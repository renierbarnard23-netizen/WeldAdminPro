using System.Collections.Generic;
using System.Windows;
using WeldAdminPro.Core.Quality;

namespace WeldAdminPro.UI.Views.Quality
{
    public partial class LinkPqrWindow : Window
    {
        public Pqr? SelectedPqr { get; private set; }

        public LinkPqrWindow(List<Pqr> pqrs)
        {
            InitializeComponent();
            PqrListBox.ItemsSource = pqrs;
        }

        private void OnLink(object sender, RoutedEventArgs e)
        {
            SelectedPqr = PqrListBox.SelectedItem as Pqr;

            if (SelectedPqr == null)
            {
                MessageBox.Show("Please select a PQR");
                return;
            }

            DialogResult = true;
            Close();
        }
    }
}