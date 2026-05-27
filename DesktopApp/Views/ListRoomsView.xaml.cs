using DesktopApp.Models;
using DesktopApp.ViewModels;
using DesktopApp.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DesktopApp.Views
{
    /// <summary>
    /// Interaction logic for ListRoomsView.xaml
    /// </summary>
    public partial class ListRoomsView : UserControl
    {
        private readonly ListRoomsViewModel _vm = new ListRoomsViewModel();
        public ListRoomsView()
        {
            InitializeComponent();
            DataContext = _vm;
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            var form = new FormRoomsView(); 
            form.Owner = Application.Current.MainWindow;
            var ok = form.ShowDialog();
            await _vm.LoadRoomsAsync();
        }
        private async void Update_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not Rooms room)
            {
                MessageBox.Show("Selecciona una habitación para editar.");
                return;
            }

            // Sincronizamos selección
            _vm.SelectedRoom = room;

            var form = new FormRoomsView(_vm.SelectedRoom);
            form.Owner = Application.Current.MainWindow;
            var ok = form.ShowDialog();
            await _vm.LoadRoomsAsync();
        }

        private void Reviews_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not Rooms room)
            {
                MessageBox.Show("Selecciona una habitación.");
                return;
            }

            var win = new RoomReviewsWindow(room.Id, room.numRoom);
            win.Owner = Application.Current.MainWindow;
            win.ShowDialog();
        }
    }
}
