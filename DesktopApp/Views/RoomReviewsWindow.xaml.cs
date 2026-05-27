using System;
using System.Collections.Generic;
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
using DesktopApp.ViewModels;

namespace DesktopApp.Views
{
    /// <summary>
    /// Lógica de interacción para RoomReviewsWindow.xaml
    /// </summary>
    public partial class RoomReviewsWindow : Window
    {
        private readonly RoomReviewsViewModel _vm = new();

        public RoomReviewsWindow(string roomId, int numRoom)
        {
            InitializeComponent();
            DataContext = _vm;
            Title = $"Reviews Habitación {numRoom}";
            Loaded += async (_, __) => await _vm.LoadAsync(roomId);
        }
    }
}
