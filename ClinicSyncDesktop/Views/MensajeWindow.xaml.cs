
using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ClinicSyncDesktop.Views
{
    public partial class MensajeWindow : Window
    {
        public bool Confirmado { get; private set; }

        private MensajeWindow()
        {
            InitializeComponent();
        }

        public static void MostrarExito(
            Window? propietario,
            string mensaje)
        {
            Mostrar(
                propietario,
                "ÉXITO!",
                mensaje,
                "IconoCheque.png");
        }

        public static void MostrarError(
            Window? propietario,
            string mensaje)
        {
            Mostrar(
                propietario,
                "ERROR!",
                mensaje,
                "IconoError.png");
        }

        public static void MostrarAviso(
            Window? propietario,
            string mensaje)
        {
            Mostrar(
                propietario,
                "AVISO!",
                mensaje,
                "IconoAlerta.png");
        }

        public static void MostrarInformacion(
            Window? propietario,
            string mensaje)
        {
            Mostrar(
                propietario,
                "",
                mensaje,
                null);
        }

        public static bool MostrarConfirmacion(
         Window? propietario,
         string mensaje,
         string titulo = "Descartar")
            {
                var ventana = new MensajeWindow();

                ventana.Configurar(
                    titulo,
                    mensaje,
                "IconoAviso.png",
                true);

            if (propietario is not null)
                ventana.Owner = propietario;

            ventana.ShowDialog();

            return ventana.Confirmado;
        }

        private static void Mostrar(
            Window? propietario,
            string titulo,
            string mensaje,
            string? icono)
        {
            var ventana = new MensajeWindow();

            ventana.Configurar(
                titulo,
                mensaje,
                icono,
                false);

            if (propietario is not null)
                ventana.Owner = propietario;

            ventana.ShowDialog();
        }

        private void Configurar(
            string titulo,
            string mensaje,
            string? icono,
            bool esConfirmacion)
        {
            txtTitulo.Text = titulo;
            txtMensaje.Text = mensaje;

            txtTitulo.Visibility =
                string.IsNullOrWhiteSpace(titulo)
                    ? Visibility.Collapsed
                    : Visibility.Visible;

            if (!string.IsNullOrWhiteSpace(icono))
            {
                imgIcono.Source = new BitmapImage(
                    new Uri(
                        $"pack://application:,,,/Resources/ImagenesAvisos/{icono}",
                        UriKind.Absolute));

                imgIcono.Visibility = Visibility.Visible;
            }
            else
            {
                imgIcono.Source = null;
                imgIcono.Visibility = Visibility.Collapsed;
            }

            if (esConfirmacion)
            {
                btnAceptar.Visibility = Visibility.Collapsed;
                btnSi.Visibility = Visibility.Visible;
                btnNo.Visibility = Visibility.Visible;
            }
            else
            {
                btnAceptar.Visibility = Visibility.Visible;
                btnSi.Visibility = Visibility.Collapsed;
                btnNo.Visibility = Visibility.Collapsed;
            }

            SizeToContent = SizeToContent.Height;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        private void Cerrar_Click(
            object sender,
            RoutedEventArgs e)
        {
            Confirmado = false;
            Close();
        }

        private void Aceptar_Click(
            object sender,
            RoutedEventArgs e)
        {
            Confirmado = true;
            Close();
        }

        private void Si_Click(
            object sender,
            RoutedEventArgs e)
        {
            Confirmado = true;
            Close();
        }

        private void No_Click(
            object sender,
            RoutedEventArgs e)
        {
            Confirmado = false;
            Close();
        }
    }
}