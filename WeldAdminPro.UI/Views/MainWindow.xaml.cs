using System.Windows;

namespace WeldAdminPro.UI.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainContent.Content = new ProjectsView();
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new HomeView();
        }

        private void Projects_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new ProjectsView();
        }

        private void Reports_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new ReportsView();
        }
	private void Stock_Click(object sender, RoutedEventArgs e)
	{
    	    MainContent.Content = new StockView();
	}


        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
