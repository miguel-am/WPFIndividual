using DesktopApp.Models;
using DesktopApp.ViewModels;
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

namespace DesktopApp.Views
{
    /// <summary>
    /// Interaction logic for FormRoomsView.xaml
    /// </summary>
    public partial class FormRoomsView : Window
    {
        public FormRoomsView()
        {
            InitializeComponent();
            DataContext = new FormRoomsViewModel();
        }
        public FormRoomsView(Rooms room) 
        {
            InitializeComponent();
            DataContext = new FormRoomsViewModel(room);
        }
    }
}
