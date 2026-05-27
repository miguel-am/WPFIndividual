using DesktopApp.Commands;
using DesktopApp.Models;
using DesktopApp.Services;
using DesktopApp.Views.Reservation;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DesktopApp.ViewModels
{
    public class ReservationListViewModel : INotifyPropertyChanged
    {
        private readonly ApiClient _apiClient;

        public ReservationListViewModel()
        {
            _apiClient = new ApiClient();

            LimpiarCommand = new RelayCommand(_ => LimpiarFiltro());
            NuevaReservaCommand = new RelayCommand(_ => NuevaReserva());
            EliminarReservaCommand = new RelayCommand(_ => EliminarReserva());
            CancelarReservaCommand = new RelayCommand(async _ => await CancelarReservaAsync());
            DescargarFacturaCommand = new RelayCommand(async _ => await DescargarPDFAsync(), _ => PuedeDescargar()); _ = CargarReservasAsync();
        }

        public ObservableCollection<Reservations> Reservas { get; } = new();
        private ObservableCollection<Reservations> _todas;

        private Reservations _reservaSeleccionada;
        public Reservations ReservaSeleccionada
        {
            get => _reservaSeleccionada;
            set
            {
                _reservaSeleccionada = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
                OnPropertyChanged(); 
            }
        }

        private string _textoBusqueda;
        public string TextoBusqueda
        {
            get => _textoBusqueda;
            set
            {
                _textoBusqueda = value;
                OnPropertyChanged();
                AplicarFiltro();
            }
        }

        private bool _ocultarCanceladas = true;
        public bool OcultarCanceladas
        {
            get => _ocultarCanceladas;
            set
            {
                _ocultarCanceladas = value;
                OnPropertyChanged();
                AplicarFiltro();
            }
        }

        private bool _ocultarTerminadas = true;
        public bool OcultarTerminadas
        {
            get => _ocultarTerminadas;
            set
            {
                _ocultarTerminadas = value;
                OnPropertyChanged();
                AplicarFiltro();
            }
        }

        public ICommand DescargarFacturaCommand { get; }


        // 3. Lógica de validación
        private bool PuedeDescargar() => ReservaSeleccionada?.Status == "terminada";
        public ICommand NuevaReservaCommand { get; }
        public ICommand CancelarReservaCommand { get; }
        public ICommand LimpiarCommand { get; }
        public ICommand EliminarReservaCommand { get; }

        public async Task CargarReservasAsync()
        {
            try
            {
                var reservations = await _apiClient.GetReservasAsync();
                var usuarios = await _apiClient.GetUsersByRolAsync("Usuario");


                foreach (var reservation in reservations)
                {
                    var rooms = await Task.WhenAll(
                        reservation.RoomIds.Select(id => _apiClient.GetRoomsId(id))
                    );

                    reservation.Rooms = rooms.Where(r => r != null).ToList();

                    var user = usuarios.FirstOrDefault(u => u.Id == reservation.User);
                    if (user != null)
                    {
                        reservation.UserDNI = user.DNI;
                        reservation.UserNombre = user.NombreCompleto;
                    }
                }


                _todas = new ObservableCollection<Reservations>(reservations);
                AplicarFiltro();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar reservas: " + ex.Message);
            }
        }

        private void AplicarFiltro()
        {
            if (_todas == null) return;

            var filtradas = _todas.AsEnumerable();

            //Buscador Global (Habitación, Nombre o DNI)
            if (!string.IsNullOrWhiteSpace(TextoBusqueda))
            {
                string busqueda = TextoBusqueda.ToLower().Trim();

                filtradas = filtradas.Where(r =>
                    // Buscar en Habitaciones
                    (r.Rooms != null && r.Rooms.Any(h => h.numRoom.ToString().Contains(busqueda))) ||

                    // Buscar en el Nombre del Usuario
                    (!string.IsNullOrEmpty(r.UserNombre) && r.UserNombre.ToLower().Contains(busqueda)) ||

                    // Buscar por DNI
                    (!string.IsNullOrEmpty(r.UserDNI) && r.UserDNI.ToLower().Contains(busqueda))
                );
            }

            // Filtro de Canceladas
            if (OcultarCanceladas)
            {
                filtradas = filtradas.Where(r =>
                    !string.Equals(r.Status, "cancelada", StringComparison.OrdinalIgnoreCase)
                );
            }

            if (OcultarTerminadas)
            {
                filtradas = filtradas.Where(r =>
                    !string.Equals(r.Status, "terminada", StringComparison.OrdinalIgnoreCase)
                );
            }

            // Actualizar la colección de la UI
            // Usamos una lista temporal para evitar múltiples refrescos visuales si la lista es muy grande
            var listaFinal = filtradas.ToList();

            Reservas.Clear();
            foreach (var r in listaFinal)
            {
                Reservas.Add(r);
            }
        }

        private void LimpiarFiltro()
        {
            TextoBusqueda = "";
            OcultarCanceladas = true;
            OcultarTerminadas = true;
            AplicarFiltro();
        }

        private void NuevaReserva()
        {
            var ventana = new Views.Reservation.AddReservationView();
            ventana.Owner = Application.Current.MainWindow;
            ventana.ShowDialog();
        }

        private void EliminarReserva()
        {
            var ventana = new Views.Reservation.DeleteCancelledReservationsView();
            ventana.Owner = Application.Current.MainWindow;
            ventana.ShowDialog();
        }

        private async Task CancelarReservaAsync()
        {
            if (ReservaSeleccionada == null) return;

            try
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("¿Seguro que quieres cancelar la reserva?", "Cancelar reserva", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    bool exito = await _apiClient.CancelReservationAsync(ReservaSeleccionada.Id);

                    if (exito)
                    {
                        ReservaSeleccionada.Status = "cancelada";
                        MessageBox.Show($"Reserva ID: {ReservaSeleccionada.Id} cancelada correctamente.");
                        AplicarFiltro();
                    }
                    else
                    {
                        MessageBox.Show("No se pudo cancelar la reserva.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cancelar reserva: " + ex.Message);
            }
        }

        private async Task DescargarPDFAsync()
        {
            if (ReservaSeleccionada == null) return;

            try
            {
                string token = _apiClient.GetToken();

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    // CORRECCIÓN DE LA RUTA: Añade /api/invoices/ para que coincida con tu imagen
                    string url = $"http://localhost:3000/invoices/{ReservaSeleccionada.Id}/download";

                    var response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var pdfBytes = await response.Content.ReadAsByteArrayAsync();
                        string nombreArchivo = $"Factura_{ReservaSeleccionada.Id}.pdf";
                        string tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), nombreArchivo);

                        await System.IO.File.WriteAllBytesAsync(tempPath, pdfBytes);
                        Process.Start(new ProcessStartInfo(tempPath) { UseShellExecute = true });
                    }
                    else
                    {
                        string errorMsg = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Error del servidor: {errorMsg}");
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show($"Error crítico: {ex.Message}"); }
        }



        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }


}

