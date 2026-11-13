
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ClinicSyncDesktop.Services;
using Emgu.CV;

namespace ClinicSyncDesktop.Views.Auth
{
    public partial class VerificarFacialWindow : Window
    {
        private readonly ServicioCamara _servicioCamara = new();
        private ServicioReconocimientoFacial? _servicioReconocimiento;

        private DispatcherTimer? _temporizador;

        private bool _cerrando;
        private bool _procesando;

        private const int MaximoBytesImagen = 5 * 1024 * 1024;

        public string? ImagenBase64 { get; private set; }

        public VerificarFacialWindow()
        {
            InitializeComponent();

            Loaded += VerificarFacialWindow_Loaded;
            Closed += VerificarFacialWindow_Closed;
        }

        // ==========================================
        // INICIALIZAR CÁMARA
        // ==========================================

        private void VerificarFacialWindow_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            IniciarCamara();
        }

        private void IniciarCamara()
        {
            try
            {
                TxtEstadoCamara.Text =
                    "Comprobando disponibilidad de la cámara...";

                if (!_servicioCamara.Iniciar())
                {
                    MessageBox.Show(
                        "No se encontró una cámara disponible.",
                        "ClinicSync",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    Close();
                    return;
                }

                _servicioReconocimiento =
                    new ServicioReconocimientoFacial();

                _temporizador = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(33)
                };

                _temporizador.Tick += ActualizarImagen;
                _temporizador.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No fue posible iniciar la cámara.\n\n" +
                    ex.Message,
                    "ClinicSync",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Close();
            }
        }

        // ==========================================
        // ACTUALIZAR VISTA PREVIA
        // ==========================================

        private void ActualizarImagen(
            object? sender,
            EventArgs e)
        {
            if (_cerrando || _procesando)
                return;

            try
            {
                using Mat? fotograma =
                    _servicioCamara.ObtenerFotograma();

                if (fotograma == null || fotograma.IsEmpty)
                    return;

                var rostros =
                    _servicioReconocimiento!.DetectarRostros(
                        fotograma);

                foreach (Rectangle rostro in rostros)
                {
                    CvInvoke.Rectangle(
                        fotograma,
                        rostro,
                        new Emgu.CV.Structure.MCvScalar(
                            0, 220, 150),
                        3);
                }

                using Bitmap imagen = fotograma.ToBitmap();

                IntPtr hBitmap = imagen.GetHbitmap();

                try
                {
                    BitmapSource imagenWpf =
                        System.Windows.Interop.Imaging
                            .CreateBitmapSourceFromHBitmap(
                                hBitmap,
                                IntPtr.Zero,
                                Int32Rect.Empty,
                                BitmapSizeOptions.FromEmptyOptions());

                    imagenWpf.Freeze();

                    ImgCamara.Source = imagenWpf;

                    TxtSinCamara.Visibility =
                        Visibility.Collapsed;
                }
                finally
                {
                    DeleteObject(hBitmap);
                }

                if (rostros.Length == 0)
                {
                    TxtEstadoCamara.Text =
                        "No se detecta ningún rostro. Colócate frente a la cámara.";

                    BtnVerificar.IsEnabled = false;
                }
                else if (rostros.Length > 1)
                {
                    TxtEstadoCamara.Text =
                        "Se detectaron varios rostros. Debe aparecer una sola persona.";

                    BtnVerificar.IsEnabled = false;
                }
                else
                {
                    TxtEstadoCamara.Text =
                        "Rostro detectado. Puedes realizar la verificación.";

                    BtnVerificar.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                _temporizador?.Stop();

                MessageBox.Show(
                    "Ocurrió un problema al procesar la imagen.\n\n" +
                    ex.Message,
                    "ClinicSync",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // ==========================================
        // CAPTURAR IMAGEN PARA VERIFICACIÓN
        // ==========================================

        private async void Verificar_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (_procesando || _cerrando)
                return;

            _procesando = true;
            BtnVerificar.IsEnabled = false;

            _temporizador?.Stop();

            try
            {
                TxtEstadoCamara.Text =
                    "Preparando captura facial...";

                using Mat? fotograma =
                    _servicioCamara.ObtenerFotograma();

                if (fotograma == null || fotograma.IsEmpty)
                {
                    throw new InvalidOperationException(
                        "No se pudo obtener una imagen de la cámara.");
                }

                var rostros =
                    _servicioReconocimiento!.DetectarRostros(
                        fotograma);

                if (rostros.Length != 1)
                {
                    throw new InvalidOperationException(
                        "Debe detectarse exactamente un rostro. Inténtalo nuevamente.");
                }

                string imagenBase64;

                using (Bitmap imagen = fotograma.ToBitmap())
                using (MemoryStream flujo = new())
                {
                    imagen.Save(flujo, ImageFormat.Jpeg);

                    if (flujo.Length > MaximoBytesImagen)
                    {
                        throw new InvalidOperationException(
                            "La imagen supera el tamaño máximo permitido.");
                    }

                    imagenBase64 =
                        Convert.ToBase64String(flujo.ToArray());
                }

                // Devolver la captura a la ventana de inicio de sesión.
                ImagenBase64 = imagenBase64;

                TxtEstadoCamara.Text =
                    "Captura preparada correctamente.";

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                TxtEstadoCamara.Text =
                    "No fue posible preparar la captura facial.";

                MessageBox.Show(
                    ex.Message,
                    "Verificación facial",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                _procesando = false;

                if (!_cerrando)
                    _temporizador?.Start();

                BtnVerificar.IsEnabled = true;
            }
        }

        // ==========================================
        // CANCELAR
        // ==========================================

        private void Cancelar_Click(
            object sender,
            RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        // ==========================================
        // CERRAR VENTANA
        // ==========================================

        private void VerificarFacialWindow_Closed(
            object? sender,
            EventArgs e)
        {
            LiberarRecursos();
        }

        private void LiberarRecursos()
        {
            if (_cerrando)
                return;

            _cerrando = true;

            if (_temporizador != null)
            {
                _temporizador.Stop();
                _temporizador.Tick -= ActualizarImagen;
                _temporizador = null;
            }

            _servicioReconocimiento?.Dispose();
            _servicioReconocimiento = null;

            _servicioCamara.Dispose();

            ImgCamara.Source = null;
        }

        // ==========================================
        // LIBERAR RECURSOS GDI
        // ==========================================

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);
    }
}
