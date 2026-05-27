using DesktopApp.Commands;
using DesktopApp.Services;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DesktopApp.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly ApiClient _apiClient;

        private string _email;
        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        public ICommand LoginCommand { get; }

        public event EventHandler LoginSucceeded;

        public LoginViewModel()
        {
            _apiClient = ApiClient.Instance;
            LoginCommand = new RelayCommand(async param => await LoginAsync(param));
        }

        private async Task LoginAsync(object parameter)
        {
            var passwordBox = parameter as PasswordBox;
            string password = passwordBox?.Password;

            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Ingrese usuario y contraseña.");
                return;
            }

            try
            {
                var token = await _apiClient.LoginAsync(Email, password);

                if (!string.IsNullOrEmpty(token))
                {
                    // 🔹 No hace falta volver a SetToken porque login ya lo hace
                    LoginSucceeded?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    MessageBox.Show("Usuario o contraseña incorrectos.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al hacer login: " + ex.Message);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
