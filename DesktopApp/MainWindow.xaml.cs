using DesktopApp.ViewModels;
using DesktopApp.Views;
using System.Windows;

namespace DesktopApp
{
    public partial class MainWindow : Window
    {
        public MainWindow() : this(false) { }

        public MainWindow(bool skipLogin = false)
        {
            InitializeComponent();

            if (!skipLogin)
            {
                var loginWindow = new LoginView();
                bool? loginResult = loginWindow.ShowDialog();

                // Si el login falla o se cierra, cerramos la app
                if (loginResult != true)
                {
                    Application.Current.Shutdown();
                    return;
                }
            }

            this.DataContext = new MainViewModel();

            
        }
    }
}