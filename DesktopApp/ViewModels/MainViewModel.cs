using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DesktopApp.Commands;
using System.Windows.Input;
using DesktopApp.Views;
using DesktopApp.Views.Reservation;
using System.Windows;
using DesktopApp.Services;

namespace DesktopApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private bool _logsEnabled = true;
        public bool LogsEnabled
        {
            get => _logsEnabled;
            set
            {
                if (_logsEnabled != value)
                {
                    _logsEnabled = value;
                    OnPropertyChanged();
                    if (IsAdmin)
                    {
                        _ = ApiClient.Instance.SetLogsEnabledAsync(value);
                    }
                }
            }
        }

        public bool IsAdmin => ApiClient.Instance.UserRole?.ToLower() == "admin";

        public string UserName => "admin"; 
        public string UserRole => ApiClient.Instance.UserRole ?? "Sin Rol";

        private object? _currentView;
        public object? CurrentView { get => _currentView; set { _currentView = value; OnPropertyChanged(); } }

        private string? _selectedMenu;
        public string? SelectedMenu { get => _selectedMenu; set { _selectedMenu = value; OnPropertyChanged(); } }

        public ICommand NavigateCommand { get; }

        public MainViewModel()
        {
            SelectedMenu = "dashboard";
            CurrentView = new LobyPanel();
            NavigateCommand = new RelayCommand(Navigation);
            _ = InicializarEstadoLogs();
        }
        public void Navigation(object? p)
        {
            var key = p?.ToString()?.ToLower();
            if (SelectedMenu != key)
                SelectedMenu = key;
            switch (key)
            {
                case "dashboard":
                    CurrentView = new LobyPanel();
                    break;
                case "bookings":
                    CurrentView = new ListReservationView();
                    break;
                case "rooms":
                    CurrentView = new ListRoomsView();
                    break;
                case "invoices":
                    CurrentView = new ListInvoicesView();
                    break;
                case "audit":
                    if (IsAdmin)
                    {
                        CurrentView = new ListLogsView { DataContext = new LogsViewModel() };
                    }
                    else
                    {
                        MessageBox.Show("Acceso restringido a administradores.");
                    }
                    break;
                
                case "power":
                    Logout();
                    break;
            }

        }
        private void Logout()
        {
            ApiClient.Instance.Logout();
            var currentMain = Application.Current.MainWindow;
            currentMain?.Hide();
            var login = new LoginView();
            if (currentMain != null)
                login.Owner = currentMain;
            bool? ok = login.ShowDialog();
            if (ok != true)
            {
                Application.Current.Shutdown();
                return;
            }
            var newMain = new MainWindow(skipLogin: true);
            Application.Current.MainWindow = newMain;
            newMain.Show();
            currentMain?.Close();

        }

        private async Task InicializarEstadoLogs()
        {
            bool estadoReal = await ApiClient.Instance.GetLogsEnabledAsync();

            _logsEnabled = estadoReal;
            OnPropertyChanged(nameof(LogsEnabled));
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
