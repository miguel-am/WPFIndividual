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
    /// Lógica de interacción para DniVerification.xaml
    /// </summary>
    public partial class DniVerificationDialog : Window
    {
        public string DniIntroducido { get; private set; }

        public DniVerificationDialog()
        {
            InitializeComponent();
            TxtDni.Focus();
        }

        private void Verify_Click(object sender, RoutedEventArgs e)
        {
            DniIntroducido = TxtDni.Text.Trim();
            DialogResult = true; // Cierra la ventana y devuelve "éxito"
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
