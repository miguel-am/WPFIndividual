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
    // Cambia Window por UserControl
    public partial class ListLogsView : UserControl
    {
        public ListLogsView()
        {
            InitializeComponent();
            DataContext = new ViewModels.LogsViewModel();
        }
    }
}
