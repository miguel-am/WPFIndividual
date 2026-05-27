using DesktopApp.Commands;
using DesktopApp.Models; // Asegúrate de que apunte a tu modelo Invoice o Reservation
using DesktopApp.Services;
using DesktopApp.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace DesktopApp.ViewModels
{
    public class ReceptionViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Invoice> _reservasHoy;
        public ObservableCollection<Invoice> ReservasHoy
        {
            get => _reservasHoy;
            set { _reservasHoy = value; 
                OnPropertyChanged();
                OnPropertyChanged(nameof(ReservasFiltradas)); ; }
        }

        private string _textoBusqueda;
        public string TextoBusqueda
        {
            get => _textoBusqueda;
            set
            {
                _textoBusqueda = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ReservasFiltradas));
            }
        }

        public ObservableCollection<Invoice> ReservasFiltradas
        {
            get
            {
                if (string.IsNullOrEmpty(TextoBusqueda)) return ReservasHoy;
                return new ObservableCollection<Invoice>(
                    ReservasHoy.Where(f => (f.ClienteNombre != null && f.ClienteNombre.ToLower().Contains(TextoBusqueda.ToLower())) ||
                                        (f.Id != null && f.Id.Contains(TextoBusqueda)))
                );
            }
        }
        public ICommand CheckInCommand { get; }
        public ICommand CheckOutCommand { get; }

        public ICommand RecargarCommand { get; }

        public ReceptionViewModel()
        {
            // Comandos para los botones de la lista 
            CheckInCommand = new RelayCommand(async (p) => await EjecutarCheckIn(p as Invoice));
            CheckOutCommand = new RelayCommand(async (p) => await EjecutarCheckOut(p as Invoice));
            RecargarCommand = new RelayCommand(async (p) => await CargarDatosHoy());

            _ = CargarDatosHoy();
        }

        private async Task CargarDatosHoy()
        {
            try
            {

                var lista = await ApiClient.Instance.GetTodayArrivalsAsync();

                if (lista != null)
                { 
                    ReservasHoy = new ObservableCollection<Invoice>(lista);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar: {ex.Message}");
            }
        }

        private async Task EjecutarCheckIn(Invoice reserva)
        {
            if (reserva == null) return;

            var dialog = new DniVerificationDialog();
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true)
            {
                string dniInput = dialog.DniIntroducido;

                if (reserva.Usuario != null && !string.IsNullOrEmpty(reserva.Usuario.Dni))
                {
                    if (dniInput.Equals(reserva.Usuario.Dni, StringComparison.OrdinalIgnoreCase))
                    {
                        bool exito = await ApiClient.Instance.CheckInAsync(reserva.Id);
                        if (exito)
                        {
                            MessageBox.Show($"Check-in exitoso. El cliente {reserva.ClienteNombre} ya puede acceder a su habitación.",
                                            "Check-in Completado", MessageBoxButton.OK, MessageBoxImage.Information);
                            await CargarDatosHoy();
                        }
                        else
                        {
                            MessageBox.Show("Error al comunicar con el servidor para realizar el check-in.", "Error de Red", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("EL DNI NO COINCIDE.\n\nLa identidad del cliente no ha podido ser verificada. Por seguridad, no se permite realizar el check-in.",
                                        "Error de Identidad",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("No hay un DNI registrado para este usuario en el sistema. Por favor, actualice los datos del cliente antes de continuar.",
                                    "Datos Faltantes", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task EjecutarCheckOut(Invoice reserva)
        {
            if (reserva == null) return;

            var confirm = MessageBox.Show($"¿Cerrar estancia de {reserva.ClienteNombre}?", "Confirmar Check-out", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            var dialog = new DniVerificationDialog();
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true)
            {
                string dniInput = dialog.DniIntroducido;

                if (reserva.Usuario != null && !string.IsNullOrEmpty(reserva.Usuario.Dni))
                {
                    if (dniInput.Equals(reserva.Usuario.Dni, StringComparison.OrdinalIgnoreCase))
                    {
                        bool exito = await ApiClient.Instance.CheckOutAsync(reserva.Id);
                        if (exito)
                        {
                            MessageBox.Show("Check-out realizado correctamente. Se ha generado la factura.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                            await CargarDatosHoy();
                        }
                        else
                        {
                            MessageBox.Show("Error al procesar el Check-out en el servidor.", "Error API", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("El DNI introducido NO coincide con los datos del titular. No se puede realizar el check-out por seguridad.",
                                        "Error de Identidad",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("No se encontró un DNI registrado para este usuario. Contacte con administración.", "Datos Incompletos", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}