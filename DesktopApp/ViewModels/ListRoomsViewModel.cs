using DesktopApp.Commands;
using DesktopApp.Models;
using DesktopApp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DesktopApp.ViewModels
{
    public class ListRoomsViewModel : INotifyPropertyChanged
    {
        private readonly ApiClient _api = new ApiClient();
        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<Rooms> Rooms { get; set; } = new();
        public ObservableCollection<Reviews> Reviews { get; set; } = new();

        private Rooms? _selectedRoom;
        public Rooms? SelectedRoom
        {
            get => _selectedRoom;
            set { _selectedRoom = value; OnPropertyChanged(); }
        }

        public ICommand DeleteRoomCommand { get; }
        public ListRoomsViewModel()
        {
            _ = LoadRoomsAsync();
            DeleteRoomCommand = new RelayCommand(async _ => await DeleteDataRooms(), _ => SelectedRoom != null);
        }
        public async Task LoadRoomsAsync()
        {
            try
            {
                var list = await _api.GetRooms();

                Rooms.Clear();

                foreach (var r in list)
                {
                    Rooms.Add(r);
                }

                foreach (var room in Rooms)
                {
                    var reviews = await _api.GetReviewIdRoom(room.Id);
                }
                OnPropertyChanged(nameof(Rooms));
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        private async Task DeleteDataRooms()
        {
            try
            {
                if (SelectedRoom is null)
                {
                    MessageBox.Show("Selecciona una habitación para eliminar.");
                    return;
                }
                var ok = MessageBox.Show(
                    $"¿Eliminar la habitación '{SelectedRoom.numRoom}'?",
                    "Confirmar eliminación",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning) == MessageBoxResult.Yes;

                if (!ok) return;

                await _api.DeleteIdRoom(SelectedRoom!.Id);
                MessageBox.Show(
                    "Habitación Eliminada\n\n" +
                    $"Número habitación: {SelectedRoom.numRoom}\n" +
                    $"Planta: {SelectedRoom.numFloor}",
                    "Éxito",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
                await LoadRoomsAsync();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

        }
        private void OnPropertyChanged([CallerMemberName] string? n = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
