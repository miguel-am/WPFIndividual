using DesktopApp;
using DesktopApp.Views;
using System.Windows;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Crear ventana de login
        var loginWindow = new LoginView();
        bool? result = loginWindow.ShowDialog();

        // Si login fue exitoso, abrir MainWindow
        if (result == true)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
        else
        {
            // Si no logueó correctamente, cerramos la app
            Shutdown();
        }
    }
}
