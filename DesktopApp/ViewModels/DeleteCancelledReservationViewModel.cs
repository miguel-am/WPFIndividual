using DesktopApp.Commands;
using DesktopApp.Models;
using DesktopApp.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DesktopApp.ViewModels
{
    public class DeleteCancelledReservationsViewModel : INotifyPropertyChanged
    {
        private readonly ApiClient _apiClient;

        public ObservableCollection<Reservations> Reservas { get; } = new();
        private ObservableCollection<Reservations> _todas;


        private Reservations _selectedReservation;
        public Reservations SelectedReservation
        {
            get => _selectedReservation;
            set { _selectedReservation = value; OnPropertyChanged(); }
        }

        public ICommand DeleteReservationCommand { get; }
        public ICommand LimpiarCommand { get; }


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

        public DeleteCancelledReservationsViewModel()
        {
            _apiClient = new ApiClient();
            DeleteReservationCommand = new RelayCommand(async _ => await DeleteReservationAsync(), _ => SelectedReservation != null);
            LimpiarCommand = new RelayCommand(_ => LimpiarFiltro());
            _ = CargarReservasAsync();
        }

      

        private async Task CargarReservasAsync()
        {
            try
            {
                var reservations = await _apiClient.GetReservasAsync();
                var canceladas = await _apiClient.GetReservasAsync();
                var usuarios = await _apiClient.GetUsersByRolAsync("Usuario");

                canceladas.Clear();

                // Cargar habitaciones para cada reserva
                foreach (var reservation in reservations)
                {

                    if (reservation.Status?.ToLower() == "cancelada" || reservation.Status?.ToLower() == "terminada")
                    {
                        canceladas.Add(reservation);
                    }
                }

                foreach (var r in canceladas)
                {
                    var rooms = await Task.WhenAll(
                           r.RoomIds.Select(id => _apiClient.GetRoomsId(id))
                       );
                    r.Rooms = rooms.Where(r => r != null).ToList();

                    var user = usuarios.FirstOrDefault(u => u.Id == r.User);

                    if (user != null)
                    {
                        r.UserDNI = user.DNI;
                        r.UserNombre = user.NombreCompleto;
                    }
                }

                _todas = new ObservableCollection<Reservations>(canceladas);
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

            //Actualizar la colección de la UI
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
            AplicarFiltro();
        }


        private async Task DeleteReservationAsync()
        {
            if (SelectedReservation == null) return;

            var result = MessageBox.Show(
                $"¿Deseas eliminar la reserva {SelectedReservation.Id} definitivamente?",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result != MessageBoxResult.Yes) return;

            try
            {
                bool success = await _apiClient.DeleteReservationAsync(SelectedReservation.Id);
                if (success)
                {
                    var reservaABorrar = SelectedReservation;

                    //Quitar de la lista filtrada
                    Reservas.Remove(reservaABorrar);

                    //Quitar de la fuente de datos original
                    if (_todas != null && _todas.Contains(reservaABorrar))
                    {
                        _todas.Remove(reservaABorrar);
                    }

                    //Limpiar la selección para evitar errores
                    SelectedReservation = null; 
                    MessageBox.Show("Reserva eliminada correctamente.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar la reserva: " + ex.Message);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
