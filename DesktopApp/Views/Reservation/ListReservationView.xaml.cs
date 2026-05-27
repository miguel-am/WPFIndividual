using DesktopApp.Models;
using DesktopApp.Services;
using DesktopApp.ViewModels;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DesktopApp.Views.Reservation
{
    public partial class ListReservationView : UserControl
    {
        private readonly ApiClient _apiClient;
        private readonly ReservationListViewModel _vm = new ReservationListViewModel();


        public ListReservationView()
        {
            InitializeComponent();
            DataContext =  _vm;

        }

        private async void Eliminar_Reserva(object sender, RoutedEventArgs e)
        {
            var form = new DeleteCancelledReservationsView();
            form.Owner = Application.Current.MainWindow;
            var ok = form.ShowDialog();
            await _vm.CargarReservasAsync();
        }


        private async void Nueva_Reserva(object sender, RoutedEventArgs e)
        {

            var form = new AddReservationView();
            form.Owner = Application.Current.MainWindow;
            var ok = form.ShowDialog();
            await _vm.CargarReservasAsync();
        }

        private void dgReservation_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //Obtenemos el elemento sobre el que se hizo clic
            var dep = (DependencyObject)e.OriginalSource;

            // Buscamos si ese elemento pertenece a una fila 
            while ((dep != null) && !(dep is DataGridRow))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }

            if (dep == null) return;

            if (dgReservation.SelectedItem is Reservations res)
            {
                var detalleWin = new DetailReservationView(res);
                detalleWin.Owner = Application.Current.MainWindow;
                detalleWin.ShowDialog();
            }
        }
    }
}
