using DesktopApp.ViewModels;
using System.Windows;

namespace DesktopApp.Views
{
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();

            var vm = new LoginViewModel();
            vm.LoginSucceeded += (s, e) =>
            {
                this.DialogResult = true; // Marca login exitoso
                this.Close();
            };

            this.DataContext = vm;
        }
    }
}
