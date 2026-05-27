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
    /// Lógica de interacción para LobyPanel.xaml
    /// </summary>
    public partial class LobyPanel : UserControl
    {
        public LobyPanel()
        {
            InitializeComponent();
            DataContext = new ReceptionViewModel();
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

    }
}
