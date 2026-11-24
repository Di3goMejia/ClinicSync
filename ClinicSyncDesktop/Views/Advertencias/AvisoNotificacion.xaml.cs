
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ClinicSyncDesktop.Views
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Controls.UserControl" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class AvisoNotificacion : UserControl
    {
        /// <summary>
        /// The temporizador
        /// </summary>
        private readonly DispatcherTimer temporizador;

        /// <summary>
        /// Initializes a new instance of the <see cref="AvisoNotificacion"/> class.
        /// </summary>
        /// <param name="mensaje">The mensaje.</param>
        /// <param name="exitoso">if set to <c>true</c> [exitoso].</param>
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
        /// <summary>
        /// Temporizadors the tick.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Temporizador_Tick(
            object? sender,
            EventArgs e)
        {
            CerrarAviso();
        }

        // CERRAR CON LA X
        /// <summary>
        /// Handles the Click event of the Cerrar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Cerrar_Click(
            object sender,
            RoutedEventArgs e)
        {
            CerrarAviso();
        }

        // QUITAR AVISO
        /// <summary>
        /// Cerrars the aviso.
        /// </summary>
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
        /// <summary>
        /// Handles the Unloaded event of the AvisoNotificacion control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void AvisoNotificacion_Unloaded(
            object sender,
            RoutedEventArgs e)
        {
            temporizador.Stop();
        }
    }
}
