using DesktopApp.Commands;
using DesktopApp.Models;
using DesktopApp.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace DesktopApp.ViewModels
{
    public class LogsViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<AuditLog> _allLogs;
        public ICollectionView FiltroLogs { get; set; }

        private string _filtroNombre;
        public string FiltroNombre
        {
            get => _filtroNombre;
            set { _filtroNombre = value; FiltroLogs.Refresh(); OnPropertyChanged(); }
        }

        private string _filtroAccion;
        public string FiltroAccion
        {
            get => _filtroAccion;
            set { _filtroAccion = value; FiltroLogs.Refresh(); OnPropertyChanged(); }
        }

        private AuditLog? _logSeleccionado;
        public AuditLog? LogSeleccionado
        {
            get => _logSeleccionado;
            set
            {
                _logSeleccionado = value;
                OnPropertyChanged(); 
            }
        }

        // Propiedades para los filtros de fecha
        private DateTime? _filtroFechaModificacion;
        public DateTime? FiltroFechaModificacion
        {
            get => _filtroFechaModificacion;
            set { _filtroFechaModificacion = value; FiltroLogs.Refresh(); OnPropertyChanged(); }
        }

        private DateTime? _filtroFechaReserva;
        public DateTime? FiltroFechaReserva
        {
            get => _filtroFechaReserva;
            set { _filtroFechaReserva = value; FiltroLogs.Refresh(); OnPropertyChanged(); }
        }
        public ObservableCollection<string> AccionesDisponibles { get; set; }
        public ICommand LimpiarFiltrosCommand { get; }

        // --- ESTADO LOGS (Switch) ---
        private bool _isInitializing = false; // Flag para evitar bucles

        private bool _logsEnabled;
        public bool LogsEnabled
        {
            get => _logsEnabled;
            set
            {
                if (_logsEnabled != value)
                {
                    _logsEnabled = value;
                    OnPropertyChanged();
                    if (!_isInitializing)
                    {
                        _ = GuardarConfiguracionApi(value);
                    }
                }
            }
        }

        public bool IsAdmin => ApiClient.Instance.UserRole?.ToLower() == "admin";

        public Visibility VisibilidadAdmin => IsAdmin ? Visibility.Visible : Visibility.Collapsed;

        public LogsViewModel()
        {
            LimpiarFiltrosCommand = new RelayCommand(_ => EjecutarLimpiarFiltros());
            _allLogs = new ObservableCollection<AuditLog>();
            FiltroLogs = CollectionViewSource.GetDefaultView(_allLogs);
            FiltroLogs.Filter = FiltrarLogica;

            AccionesDisponibles = new ObservableCollection<string> { "TODAS", "CREACION", "CANCELACION", "CHECK-IN", "CHECK-OUT", "ELIMINACION" };
            _filtroAccion = "TODAS";

            _ = CargarDatosIniciales();
            _ = CargarEstadoConfiguracion();
        }

        private void EjecutarLimpiarFiltros()
        {
            FiltroNombre = string.Empty;
            FiltroAccion = "TODAS";
            FiltroFechaModificacion = null; 
            FiltroFechaReserva = null;     

            FiltroLogs.Refresh();
        }

        private bool FiltrarLogica(object item)
        {
            var log = item as AuditLog;
            if (log == null) return false;

            bool nombreOk = string.IsNullOrEmpty(FiltroNombre) ||
                            (log.NombreActor != null && log.NombreActor.ToLower().Contains(FiltroNombre.ToLower()));

            bool accionOk = FiltroAccion == "TODAS" || log.Action == FiltroAccion;

            bool fechaModOk = !FiltroFechaModificacion.HasValue ||
                       log.Timestamp.Date == FiltroFechaModificacion.Value.Date;

            // Filtro por Fecha de la Reserva
            bool fechaResOk = !FiltroFechaReserva.HasValue ||
                              (log.FechaReserva.HasValue && log.FechaReserva.Value.Date == FiltroFechaReserva.Value.Date);

            return nombreOk && accionOk && fechaModOk && fechaResOk;
        }

        public async Task CargarDatosIniciales()
        {
            try
            {
                var logs = await ApiClient.Instance.GetAllLogsAsync();
                _allLogs.Clear();
                if (logs != null)
                {
                    foreach (var l in logs) _allLogs.Add(l);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al cargar logs: " + ex.Message);
            }
        }

        private async Task GuardarConfiguracionApi(bool valor)
        {
            bool exito = await ApiClient.Instance.SetLogsEnabledAsync(valor);
            if (!exito)
            {
                MessageBox.Show("Error al actualizar la configuración en el servidor.");
            }
        }

        // Dentro de LogsViewModel.cs

        public async Task CargarEstadoConfiguracion()
        {
            try
            {
                _isInitializing = true; 
                bool estaActivado = await ApiClient.Instance.GetLogsEnabledAsync();
                LogsEnabled = estaActivado; 
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al cargar configuración de logs: " + ex.Message);
            }
            finally
            {
                _isInitializing = false; 
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}