using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DesktopApp.Models;
using DesktopApp.Services;
using System.Windows;

namespace DesktopApp.ViewModels
{
    public class RoomReviewsViewModel : INotifyPropertyChanged
    {
        private readonly ApiClient _api = ApiClient.Instance;

        public ObservableCollection<Reviews> Reviews { get; } = new();

        public async Task LoadAsync(string roomId)
        {
            try
            {
                var list = await _api.GetReviewIdRoom(roomId);
                Reviews.Clear();
                foreach (var r in list) Reviews.Add(r);
            }
            catch (System.Exception e)
            {
                MessageBox.Show(e.Message, "Error");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
