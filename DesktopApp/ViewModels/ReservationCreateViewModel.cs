using DesktopApp.Commands;
using DesktopApp.Models;
using DesktopApp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DesktopApp.ViewModels
{
    public class ReservationCreateViewModel : INotifyPropertyChanged
    {
        private readonly ApiClient _apiClient;
        private List<Reservations> _todasLasReservas = new(); // Caché local de reservas

        public ReservationCreateViewModel()
        {
            _apiClient = new ApiClient();

            // Inicialización de colecciones
            HabitacionesDisponibles = new ObservableCollection<Rooms>();

            // Suscribirse a cambios en la colección de habitaciones seleccionadas
            SelectedRooms.CollectionChanged += (s, e) => ActualizarPrecioTotal();

            CrearReservaCommand = new RelayCommand(_ => { _ = CrearReservaAsync(); });

            // Carga inicial de datos
            _ = InicializarDatosAsync();
        }

        private async Task InicializarDatosAsync()
        {
            await CargarUsuariosAsync();
            await CargarHabitacionesBaseAsync();
            await CargarReservasExistentesAsync();
            FiltrarHabitaciones(); // Filtro inicial
        }

        private float _precioTotal;
        public float PrecioTotal
        {
            get => _precioTotal;
            set { _precioTotal = value; OnPropertyChanged(); }
        }

        private int _numeroPersonas = 1;
        public int NumeroPersonas
        {
            get => _numeroPersonas;
            set
            {
                _numeroPersonas = value;
                OnPropertyChanged();
                FiltrarHabitaciones(); // Refiltramos cuando cambia el número de personas
                ValidarCapacidadSeleccionada();
            }
        }

        private string _mensajeAvisoCapacidad;
        public string MensajeAvisoCapacidad
        {
            get => _mensajeAvisoCapacidad;
            set { _mensajeAvisoCapacidad = value; OnPropertyChanged(); }
        }

        private string _filtroNumeroHabitacion;
        public string FiltroNumeroHabitacion
        {
            get => _filtroNumeroHabitacion;
            set { _filtroNumeroHabitacion = value; OnPropertyChanged(); FiltrarHabitaciones(); }
        }

        private DateTime? _checkIn;
        public DateTime? CheckIn
        {
            get => _checkIn;
            set { _checkIn = value; OnPropertyChanged(); FiltrarHabitaciones(); }
        }

        private DateTime? _checkOut;
        public DateTime? CheckOut
        {
            get => _checkOut;
            set { _checkOut = value; OnPropertyChanged(); FiltrarHabitaciones(); }
        }

        private User _usuarioSeleccionado;
        public User? UsuarioSeleccionado
        {
            get => _usuarioSeleccionado;
            set { _usuarioSeleccionado = value; OnPropertyChanged(); ActualizarPrecioTotal(); }
        }

        private string _dniBusqueda;
        public string DNIBusqueda
        {
            get => _dniBusqueda;
            set { _dniBusqueda = value; OnPropertyChanged(); FiltrarUsuarios(); }
        }

        private string _selectedStatus = "confirmada";
        public string SelectedStatus
        {
            get => _selectedStatus;
            set { _selectedStatus = value; OnPropertyChanged(); }
        }
        

        // Todas las habitaciones que existen (maestras)
        public List<Rooms> HabitacionesMaestras { get; } = new();

        // Las que se muestran en el UI tras filtrar por fecha y número
        private ObservableCollection<Rooms> _habitacionesDisponibles;
        public ObservableCollection<Rooms> HabitacionesDisponibles
        {
            get => _habitacionesDisponibles;
            set { _habitacionesDisponibles = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Rooms> SelectedRooms { get; } = new();
        public ObservableCollection<User> Usuarios { get; } = new();
        public ObservableCollection<User> UsuariosFiltrados { get; } = new();

        public ICommand CrearReservaCommand { get; }

        #region Métodos de Carga API

        private async Task CargarHabitacionesBaseAsync()
        {
            try
            {
                var rooms = await _apiClient.GetRooms();
                HabitacionesMaestras.Clear();
                // Solo cargamos las que no están fuera de servicio permanentemente
                HabitacionesMaestras.AddRange(rooms.Where(r => r.availability.ToString() == "Available"));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar habitaciones: " + ex.Message);
            }
        }

        private async Task CargarReservasExistentesAsync()
        {
            try
            {
                // Importante: Asegúrate de que este método exista en tu ApiClient
                var lista = await _apiClient.GetReservasAsync();
                _todasLasReservas = lista.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al obtener reservas para filtro: " + ex.Message);
            }
        }

        private async Task CargarUsuariosAsync()
        {
            try
            {
                var lista = await _apiClient.GetUsersByRolAsync("Usuario");
                Usuarios.Clear();
                foreach (var u in lista) Usuarios.Add(u);
                FiltrarUsuarios();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar usuarios: " + ex.Message);
            }
        }
        #endregion

        #region Lógica de Filtrado

        private void FiltrarHabitaciones()
        {
            if (CheckIn == null || CheckOut == null || CheckIn >= CheckOut)
            {
                HabitacionesDisponibles.Clear();
                MensajeAvisoCapacidad = ""; // Limpiar si no hay fechas
                return;
            }

            //Filtrar disponibilidad real por fechas y número
            var disponiblesPorFecha = HabitacionesMaestras.Where(room =>
            {
                if (!string.IsNullOrWhiteSpace(FiltroNumeroHabitacion) &&
                    !room.numRoom.ToString().Contains(FiltroNumeroHabitacion))
                    return false;

                bool tieneConflicto = _todasLasReservas.Any(res =>
                    res.RoomIds.Contains(room.Id) &&
                    res.Status != "cancelada" && res.Status != "terminada" &&
                    CheckIn < res.CheckOut &&
                    CheckOut > res.CheckIn
                );
                return !tieneConflicto;
            }).ToList();

            //Gestión de la lista y mensajes básicos
            if (disponiblesPorFecha.Count == 0)
            {
                MensajeAvisoCapacidad = "No hay habitaciones disponibles para esas fechas.";
                HabitacionesDisponibles.Clear();
            }
            else
            {
                ActualizarLista(disponiblesPorFecha);

                // Llamamos a la validación de capacidad
                ValidarCapacidadSeleccionada();
            }
        }

        // En tu ViewModel
        public void ValidarCapacidadSeleccionada()
        {
            // Si no hay fechas o habitaciones, no mostramos error de "selección" aún
            if (CheckIn == null || CheckOut == null || HabitacionesDisponibles.Count == 0)
            {
                return;
            }

            int capacidadTotal = SelectedRooms.Sum(r => r.maxOccupancy);

            if (SelectedRooms.Count > 0 && capacidadTotal < NumeroPersonas)
            {
                MensajeAvisoCapacidad = $"Capacidad insuficiente: Tienes espacio para {capacidadTotal} de {NumeroPersonas} personas. Selecciona más habitaciones.";
            }
            else if (HabitacionesDisponibles.All(r => r.maxOccupancy < NumeroPersonas) && SelectedRooms.Count == 0)
            {
                // Este es el aviso preventivo antes de que seleccionen nada
                MensajeAvisoCapacidad = "El grupo es grande. Por favor, selecciona varias habitaciones para cubrir el total de personas.";
            }
            else
            {
                // Si todo está correcto o no hay habitaciones disponibles (que ya lo maneja FiltrarHabitaciones)
                // Solo limpiamos si no hay un error de "No hay habitaciones"
                if (HabitacionesDisponibles.Count > 0)
                    MensajeAvisoCapacidad = "";
            }
        }

        private void ActualizarLista(List<Rooms> lista)
        {
            HabitacionesDisponibles.Clear();
            foreach (var h in lista) HabitacionesDisponibles.Add(h);
        }

        private void FiltrarUsuarios()
        {
            var filtro = string.IsNullOrWhiteSpace(DNIBusqueda)
                ? Usuarios.ToList()
                : Usuarios.Where(u => !string.IsNullOrEmpty(u.DNI) &&
                                      u.DNI.ToUpper().Contains(DNIBusqueda.ToUpper())).ToList();

            UsuariosFiltrados.Clear();
            foreach (var u in filtro) UsuariosFiltrados.Add(u);
            UsuarioSeleccionado = UsuariosFiltrados.FirstOrDefault();
        }
        #endregion

        public async Task CrearReservaAsync()
        {
            if (UsuarioSeleccionado == null || SelectedRooms.Count == 0 || CheckIn == null || CheckOut == null)
            {
                MessageBox.Show("Por favor, completa todos los campos y selecciona al menos una habitación.");
                return;
            }
            int capacidadTotalSeleccionada = SelectedRooms.Sum(r => r.maxOccupancy);
            if (capacidadTotalSeleccionada < NumeroPersonas)
            {
                MessageBox.Show($"Las habitaciones seleccionadas solo tienen capacidad para {capacidadTotalSeleccionada} personas. Necesitas {NumeroPersonas}.");
                return;
            }

            try
            {
                var nuevaReserva = new Reservations
                {
                    User = UsuarioSeleccionado.Id,
                    RoomIds = SelectedRooms.Select(r => r.Id).ToList(),
                    CheckIn = CheckIn.Value.Date.AddHours(12),
                    CheckOut = CheckOut.Value.Date.AddHours(12),
                    Status = SelectedStatus,
                    NumGuests = NumeroPersonas
                };
                var errorJson = await _apiClient.PostReservationAsync(nuevaReserva);

                if (string.IsNullOrEmpty(errorJson))
                {
                    MessageBox.Show("Reserva creada correctamente.");
                    // Limpiar y refrescar
                    SelectedRooms.Clear();

                    CheckIn = null;
                    CheckOut = null;

                    //Resetear datos de usuario y búsqueda
                    UsuarioSeleccionado = null;
                    DNIBusqueda = string.Empty;
                    FiltroNumeroHabitacion = string.Empty;

                    //Resetear contadores y mensajes
                    NumeroPersonas = 1;
                    MensajeAvisoCapacidad = string.Empty;
                    PrecioTotal = 0;
                    await CargarReservasExistentesAsync(); // Recargamos para que la nueva reserva bloquee las fechas
                    FiltrarHabitaciones();
                }
                else
                {
                    MessageBox.Show("Error:\n" + errorJson);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inesperado: " + ex.Message);
            }
        }

        public void ActualizarPrecioTotal()
        {
            if (CheckIn == null || CheckOut == null || CheckIn >= CheckOut || SelectedRooms.Count == 0)
            {
                PrecioTotal = 0;
                return;
            }

            // Calcular noches
            int noches = (CheckOut.Value.Date - CheckIn.Value.Date).Days;

            // Sumar precios de habitaciones seleccionadas (asumiendo propiedad 'price')
            float precioPorNoche = SelectedRooms.Sum(r => r.pricePerNight);
            float totalBase = precioPorNoche * noches;

            if (UsuarioSeleccionado != null && UsuarioSeleccionado.VipStatus)
            {
                PrecioTotal = totalBase * 0.80f; // Aplicar descuento visual
            }
            else
            {
                PrecioTotal = totalBase;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}