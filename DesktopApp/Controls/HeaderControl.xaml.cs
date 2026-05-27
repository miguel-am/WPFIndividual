using System.Windows.Controls;
using DesktopApp.ViewModels;

namespace DesktopApp.Controls
{
    public partial class HeaderControl : UserControl
    {
        public HeaderControl()
        {
            InitializeComponent();
            var vm = new LogsViewModel();
            this.DataContext = vm;

            // Ejecutamos la carga inicial cuando el control esté listo
            this.Loaded += async (s, e) =>
            {
                await vm.CargarEstadoConfiguracion();
            };
        }
       
    }
}