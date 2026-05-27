using DesktopApp.Models;
using DesktopApp.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace DesktopApp.Views.Reservation
{
    public partial class AddReservationView : Window
    {
        public AddReservationView()
        {
            InitializeComponent();
            DataContext = new ReservationCreateViewModel();
            lbHabitaciones.SelectionChanged += LbHabitaciones_SelectionChanged;
        }

        private void LbHabitaciones_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is ReservationCreateViewModel vm)
            {
                vm.SelectedRooms.Clear();
                foreach (Rooms room in lbHabitaciones.SelectedItems)
                {
                    vm.SelectedRooms.Add(room);
                }
                vm.ValidarCapacidadSeleccionada();
                vm.ActualizarPrecioTotal();
            }            
        }
    }
}
