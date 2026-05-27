using DesktopApp.Models;
using DesktopApp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DesktopApp.Views.Reservation
{
    /// <summary>
    /// Lógica de interacción para detailReservationView.xaml
    /// </summary>
    public partial class DetailReservationView : Window
    {
        private Reservations _reserva;
        private ObservableCollection<Communication> _historial;

        public DetailReservationView(Reservations reserva)
        {
            InitializeComponent();
            _reserva = reserva;
            this.Title = $"Historial - Reserva: {reserva.UserNombre}";
            CargarDatos();
        }

        private async void CargarDatos()
        {
            var datos = await ApiClient.Instance.GetCommunicationsAsync(_reserva.Id);
            _historial = new ObservableCollection<Communication>(datos);
            icHistory.ItemsSource = _historial; 
        }

        private async void BtnGuardarNota_Click(object sender, RoutedEventArgs e)
        {
            string nota = txtNuevaNota.Text.Trim();
            if (string.IsNullOrEmpty(nota)) return;

            bool ok = await ApiClient.Instance.AddNoteAsync(_reserva.Id, nota);
            if (ok)
            {
                txtNuevaNota.Clear();
                CargarDatos(); 
            }
            else
            {
                MessageBox.Show("Error al guardar la nota.");
            }
        }
    }
}

