
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ClinicSyncDesktop.Views
{
    public partial class AvisoNotificacion : UserControl
    {
        private readonly DispatcherTimer temporizador;

        public AvisoNotificacion(
            string mensaje,
            bool exitoso)
        {
            InitializeComponent();

            // MOSTRAR MENSAJE
            txtMensaje.Text = mensaje;

            // SELECCIONAR ÍCONO
            string icono = exitoso
                ? "IconoCheque.png"
                : "IconoError.png";

            // CARGAR IMAGEN
            imgEstado.Source = new BitmapImage(
                new Uri(
                    "pack://application:,,,/Resources/ImagenesAvisos/" +
                    icono,
                    UriKind.Absolute));

            // TEMPORIZADOR
            temporizador = new DispatcherTimer();

            temporizador.Interval =
                TimeSpan.FromSeconds(3);

            temporizador.Tick += Temporizador_Tick;

            temporizador.Start();

            Unloaded += AvisoNotificacion_Unloaded;
        }

        // DESAPARECER AUTOMÁTICAMENTE
        private void Temporizador_Tick(
            object? sender,
            EventArgs e)
        {
            CerrarAviso();
        }

        // CERRAR CON LA X
        private void Cerrar_Click(
            object sender,
            RoutedEventArgs e)
        {
            CerrarAviso();
        }

        // QUITAR AVISO
        private void CerrarAviso()
        {
            temporizador.Stop();

            if (Parent is Panel panel)
            {
                panel.Children.Remove(this);

                // OCULTAR CONTENEDOR VACÍO
                if (panel.Children.Count == 0)
                {
                    panel.Visibility =
                        Visibility.Collapsed;
                }
            }
        }

        // DETENER TEMPORIZADOR AL QUITARLO
        private void AvisoNotificacion_Unloaded(
            object sender,
            RoutedEventArgs e)
        {
            temporizador.Stop();
        }
    }
}
