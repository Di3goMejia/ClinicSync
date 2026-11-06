using System.Windows;

namespace ClinicSyncDesktop.Views
{
    public partial class HoraNoDisponibleWindow : Window
    {
        private HoraNoDisponibleWindow()
        {
            InitializeComponent();
        }

        // Muestra el aviso encima de la ventana indicada.
        public static void Mostrar(Window? propietario)
        {
            var ventana = new HoraNoDisponibleWindow();

            if (propietario is not null)
                ventana.Owner = propietario;

            ventana.ShowDialog();
        }

        private void Aceptar_Click(
            object sender,
            RoutedEventArgs e)
        {
            Close();
        }

        private void Cerrar_Click(
            object sender,
            RoutedEventArgs e)
        {
            Close();
        }
    }
}