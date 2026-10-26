
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ClinicSyncDesktop.Views
{
    public partial class AvisoPaciente : UserControl
    {
        private readonly DispatcherTimer temporizador;

        public AvisoPaciente(
            string mensaje,
            bool exitoso)
        {
            InitializeComponent();

            txtMensaje.Text = mensaje;

            string icono = exitoso
                ? "IconoCheque.png"
                : "IconoError.png";

            imgEstado.Source = new BitmapImage(
                new Uri(
                    "pack://application:,,,/Resources/ImagenesAvisos/" +
                    icono,
                    UriKind.Absolute));

            // TEMPORIZADOR DE 5 SEGUNDOS
            temporizador = new DispatcherTimer();

            temporizador.Interval =
                TimeSpan.FromSeconds(3);

            temporizador.Tick += Temporizador_Tick;

            temporizador.Start();

            Unloaded += AvisoPaciente_Unloaded;
        }

        // DESAPARECER AUTOMÁTICAMENTE
        private void Temporizador_Tick(
            object? sender,
            EventArgs e)
        {
            CerrarAviso();
        }

        // CERRAR MANUALMENTE CON LA X
        private void Cerrar_Click(
            object sender,
            RoutedEventArgs e)
        {
            CerrarAviso();
        }

        // QUITAR AVISO DE LA PANTALLA
        private void CerrarAviso()
        {
            temporizador.Stop();

            if (Parent is Panel panel)
            {
                panel.Children.Remove(this);
            }
        }

        // DETENER TEMPORIZADOR SI SE QUITA EL AVISO
        private void AvisoPaciente_Unloaded(
            object sender,
            RoutedEventArgs e)
        {
            temporizador.Stop();
        }
    }
}
