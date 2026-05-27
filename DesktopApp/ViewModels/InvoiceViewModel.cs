using DesktopApp.Commands;
using DesktopApp.Models;
using DesktopApp.Services; 
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DesktopApp.ViewModels
{
    public class InvoicesViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Invoice> _facturas;
        public ObservableCollection<Invoice> Facturas
        {
            get => _facturas;
            set
            {
                _facturas = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FacturasFiltradas));
            }
        }

        private Invoice _facturaSeleccionada;
        public Invoice FacturaSeleccionada
        {
            get => _facturaSeleccionada;
            set
            {
                _facturaSeleccionada = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
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
                OnPropertyChanged(nameof(FacturasFiltradas)); 
            }
        }

        private DateTime? _fechaFiltro;

        public DateTime? FechaFiltro
        {
            get => _fechaFiltro;
            set
            {
                _fechaFiltro = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FacturasFiltradas));
            }
        }



        public ObservableCollection<Invoice> FacturasFiltradas
        {
            get
            {
                if (Facturas == null) return new ObservableCollection<Invoice>();

                var filtrados = Facturas.AsEnumerable();

                // 1. Filtro por Nombre o ID (Texto)
                if (!string.IsNullOrWhiteSpace(TextoBusqueda))
                {
                    string busqueda = TextoBusqueda.ToLower();
                    filtrados = filtrados.Where(f =>
                        (f.ClienteNombre != null && f.ClienteNombre.ToLower().Contains(busqueda)) ||
                        (f.Id != null && f.Id.Contains(busqueda)));
                }

                // 2. Filtro por Fecha (Conversión segura de String a DateTime)
                if (FechaFiltro.HasValue)
                {
                    filtrados = filtrados.Where(f =>
                    {
                        // Si f.Fecha ya es DateTime, usa f.Fecha.Date
                        // Si f.Fecha es string, intentamos convertirlo:
                        if (DateTime.TryParse(f.Fecha.ToString(), out DateTime fechaDoc))
                        {
                            return fechaDoc.Date == FechaFiltro.Value.Date;
                        }
                        return false;
                    });
                }

                return new ObservableCollection<Invoice>(filtrados);
            }
        }


        public ICommand LimpiarFiltrosCommand => new RelayCommand(_ => {
            TextoBusqueda = string.Empty;
            FechaFiltro = null;
        });
        public ICommand ReenviarEmailCommand { get; }

        public ICommand DescargarCommand { get; }

        public InvoicesViewModel()
        {
            DescargarCommand = new RelayCommand(async _ => await DescargarPDF(), _ => FacturaSeleccionada != null);
            ReenviarEmailCommand = new RelayCommand(async _ => await ReenviarEmail(), _ => FacturaSeleccionada != null);
            _ = CargarFacturas();
        }

        private async Task DescargarPDF()
        {
            try
            {
                
                string token = ApiClient.Instance.GetToken();
                using var client = new HttpClient { BaseAddress = new Uri("http://localhost:3000/") };
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync($"/invoices/{FacturaSeleccionada.Id}/download");

                if (response.IsSuccessStatusCode)
                {
                    var pdfBytes = await response.Content.ReadAsByteArrayAsync();
                    string tempPath = Path.Combine(Path.GetTempPath(), $"Factura_{FacturaSeleccionada.Id}.pdf");
                    await File.WriteAllBytesAsync(tempPath, pdfBytes);
                    Process.Start(new ProcessStartInfo(tempPath) { UseShellExecute = true });
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private async Task CargarFacturas()
        {
            try
            {
                var lista = await ApiClient.Instance.GetInvoicesAsync();
                Facturas = new ObservableCollection<Invoice>(lista);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar la lista de facturas: " + ex.Message);
            }
        }

        private async Task ReenviarEmail()
        {
            if (FacturaSeleccionada == null) return;

            bool ok = await ApiClient.Instance.PostResendInvoiceAsync(FacturaSeleccionada.Id);

            if (ok)
            {
                MessageBox.Show("Factura reenviada correctamente a su email.", "Éxito");
            }
            else
            {
                MessageBox.Show("El servidor denegó la petición. Revisa si el ID es correcto o si la sesión ha expirado.", "Error");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}