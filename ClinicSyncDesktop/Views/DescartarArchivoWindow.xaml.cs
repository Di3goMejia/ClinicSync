
using System.Windows;

namespace ClinicSyncDesktop.Views
{
    public partial class DescartarArchivoWindow : Window
    {
        public DescartarArchivoWindow()
        {
            InitializeComponent();
        }

        // CONFIRMAR DESACTIVACIÓN
        private void Si_Click(
            object sender,
            RoutedEventArgs e)
        {
            DialogResult = true;
        }

        // CANCELAR
        private void No_Click(
            object sender,
            RoutedEventArgs e)
        {
            DialogResult = false;
        }

        // CERRAR CON LA X
        private void Cerrar_Click(
            object sender,
            RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
