
using System.Windows;

namespace ClinicSyncDesktop.Views
{
    public partial class ConfirmarDescarteWindow : Window
    {
        public ConfirmarDescarteWindow(string mensaje = "¿Seguro que desea descartar este registro?")
        {
            InitializeComponent();
            txtMensaje.Text = mensaje;
        }

        // SÍ: DESCARTAR EL REGISTRO
        private void Si_Click(
            object sender,
            RoutedEventArgs e)
        {
            DialogResult = true;
        }

        // NO: REGRESAR AL FORMULARIO
        private void No_Click(
            object sender,
            RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
