
using System.Windows;

namespace ClinicSyncDesktop.Views
{
    public partial class DescartarPacienteWindow : Window
    {
        public DescartarPacienteWindow()
        {
            InitializeComponent();
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
