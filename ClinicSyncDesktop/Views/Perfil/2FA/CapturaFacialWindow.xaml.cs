
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ClinicSyncDesktop.Services;
using Emgu.CV;

namespace ClinicSyncDesktop.Views.Perfil._2FA
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Window" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class CapturaFacialWindow : Window
    {
        // SERVICIOS
        /// <summary>
        /// The servicio camara
        /// </summary>
        private readonly ServicioCamara _servicioCamara = new();
        /// <summary>
        /// The servicio reconocimiento
        /// </summary>
        private ServicioReconocimientoFacial? _servicioReconocimiento;
        /// <summary>
        /// The facial API
        /// </summary>
        private readonly FacialApiService _facialApi = new();
        // CÁMARA Y TEMPORIZADOR
        /// <summary>
        /// The temporizador
        /// </summary>
        private DispatcherTimer? _temporizador;
        // IMÁGENES ORIGINALES PARA ENVIAR A LA API
        /// <summary>
        /// The muestras originales
        /// </summary>
        private readonly List<string> _muestrasOriginales = new();
        /// <summary>
        /// The rostros actuales
        /// </summary>
        private Rectangle[] _rostrosActuales =
            Array.Empty<Rectangle>();
        /// <summary>
        /// The ultima imagen
        /// </summary>
        private BitmapSource? _ultimaImagen;

        // ESTADOS
        /// <summary>
        /// The cerrando
        /// </summary>
        private bool _cerrando;
        /// <summary>
        /// The captura preparada
        /// </summary>
        private bool _capturaPreparada;
        /// <summary>
        /// The captura confirmada
        /// </summary>
        private bool _capturaConfirmada;

        /// <summary>
        /// The total muestras
        /// </summary>
        private const int TotalMuestras = 10;


        /// <summary>
        /// Initializes a new instance of the <see cref="CapturaFacialWindow"/> class.
        /// </summary>
        public CapturaFacialWindow()
        {
            InitializeComponent();

            Loaded += CapturaFacialWindow_Loaded;
            Closed += CapturaFacialWindow_Closed;
        }

        /// <summary>
        /// Deletes the object.
        /// </summary>
        /// <param name="hObject">The h object.</param>
        /// <returns></returns>
        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);


        // ==========================================
        // INICIALIZAR CÁMARA
        // ==========================================

        /// <summary>
        /// Handles the Loaded event of the CapturaFacialWindow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void CapturaFacialWindow_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            IniciarCamara();
        }

        /// <summary>
        /// Iniciars the camara.
        /// </summary>
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

                ActualizarEstadoMuestras();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No fue posible iniciar la captura facial.\n\n" +
                    ex.Message,
                    "ClinicSync",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Close();
            }
        }


        // ==========================================
        // ACTUALIZAR IMAGEN DE LA CÁMARA
        // ==========================================

        private void ActualizarImagen(
            object? sender,
            EventArgs e)
        {
            if (_cerrando)
                return;

            if (_capturaPreparada)
            {
                return;
            }

            try
            {
                using Mat? fotograma =
                    _servicioCamara.ObtenerFotograma();

                if (fotograma == null || fotograma.IsEmpty)
                    return;

                _rostrosActuales =
                    _servicioReconocimiento!.DetectarRostros(
                        fotograma);

                foreach (Rectangle rostro in _rostrosActuales)
                {
                    CvInvoke.Rectangle(
                        fotograma,
                        rostro,
                        new Emgu.CV.Structure.MCvScalar(
                            0, 220, 150),
                        3);
                }

                using var imagen = fotograma.ToBitmap();

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

                    _ultimaImagen = imagenWpf;
                    ImgCamara.Source = imagenWpf;

                    TxtSinCamara.Visibility =
                        Visibility.Collapsed;
                }
                finally
                {
                    DeleteObject(hBitmap);
                }

                ActualizarEstadoMuestras();
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
        // ACTUALIZAR ESTADO DE LAS MUESTRAS
        // ==========================================

        private void ActualizarEstadoMuestras()
        {
            if (_capturaPreparada)
            {
                TxtEstadoCamara.Text =
                    "Revisa las muestras antes de registrar la credencial.";

                return;
            }

            if (_rostrosActuales.Length == 0)
            {
                TxtEstadoCamara.Text =
                    $"Muestras: {_muestrasOriginales.Count}/{TotalMuestras}. " +
                    "Coloca tu rostro frente a la cámara.";
            }
            else if (_rostrosActuales.Length == 1)
            {
                TxtEstadoCamara.Text =
                    $"Rostro detectado. Muestras: " +
                    $"{_muestrasOriginales.Count}/{TotalMuestras}. " +
                    "Pulsa Registrar rostro para tomar una muestra.";
            }
            else
            {
                TxtEstadoCamara.Text =
                    "Se detectaron varios rostros. " +
                    "Debe aparecer una sola persona.";
            }
        }


        // ==========================================
        // REGISTRAR MUESTRAS FACIALES
        // ==========================================

        private void Registrar_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (_capturaConfirmada || _capturaPreparada)
                return;

            if (_muestrasOriginales.Count >= TotalMuestras)
                return;

            if (_rostrosActuales.Length != 1)
            {
                MessageBox.Show(
                    "Debe detectarse exactamente un rostro.",
                    "ClinicSync",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            try
            {
                using Mat? fotograma =
                    _servicioCamara.ObtenerFotograma();

                if (fotograma == null || fotograma.IsEmpty)
                {
                    MessageBox.Show(
                        "No se pudo obtener una imagen de la cámara.",
                        "ClinicSync",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                Rectangle[] rostros =
                    _servicioReconocimiento!.DetectarRostros(
                        fotograma);

                if (rostros.Length != 1)
                {
                    MessageBox.Show(
                        "La nueva imagen no contiene exactamente un rostro.",
                        "ClinicSync",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    return;
                }

                // CONVERTIR LA IMAGEN ORIGINAL A JPEG Y BASE64
                string imagenBase64;

                using (Bitmap imagenOriginal = fotograma.ToBitmap())
                using (MemoryStream flujo = new())
                {
                    imagenOriginal.Save(
                        flujo,
                        ImageFormat.Jpeg);

                    imagenBase64 =
                        Convert.ToBase64String(flujo.ToArray());
                }

                _muestrasOriginales.Add(imagenBase64);

                if (_muestrasOriginales.Count < TotalMuestras)
                {
                    TxtEstadoCamara.Text =
                        $"Muestra {_muestrasOriginales.Count} de " +
                        $"{TotalMuestras} registrada. " +
                        "Cambia ligeramente la posición del rostro.";

                    return;
                }

                // COMPLETAR LAS DIEZ MUESTRAS
                _capturaPreparada = true;

                _temporizador?.Stop();

                ImgCamara.Source = _ultimaImagen;

                PanelRegistrar.Visibility =
                    Visibility.Collapsed;

                PanelRevision.Visibility =
                    Visibility.Visible;

                BtnConfirmar.IsEnabled = true;
                BtnVolverCapturar.IsEnabled = true;

                TxtEstadoCamara.Text =
                    "Diez muestras recopiladas. Puedes registrar tu credencial facial.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No fue posible registrar la muestra.\n\n" +
                    ex.Message,
                    "ClinicSync",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }


        // ==========================================
        // VOLVER A CAPTURAR
        // ==========================================

        private void VolverCapturar_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (_capturaConfirmada)
                return;

            LiberarMuestras();

            _capturaPreparada = false;

            _ultimaImagen = null;

            BtnConfirmar.IsEnabled = true;
            BtnVolverCapturar.IsEnabled = true;

            ImgCamara.Source = null;

            TxtSinCamara.Visibility =
                Visibility.Visible;

            PanelRevision.Visibility =
                Visibility.Collapsed;

            PanelRegistrar.Visibility =
                Visibility.Visible;

            _temporizador?.Start();

            ActualizarEstadoMuestras();
        }


        // ==========================================
        // REGISTRAR CREDENCIAL FACIAL EN EL SERVIDOR
        // ==========================================

        private async void Confirmar_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!_capturaPreparada ||
                _muestrasOriginales.Count != TotalMuestras)
            {
                MessageBox.Show(
                    $"Debes registrar {TotalMuestras} muestras válidas.",
                    "ClinicSync",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            BtnConfirmar.IsEnabled = false;
            BtnVolverCapturar.IsEnabled = false;

            try
            {
                TxtEstadoCamara.Text =
                    "Enviando muestras al servidor. Espera un momento...";

                // REGISTRAR LA CREDENCIAL EN EL SERVIDOR
                await _facialApi.RegistrarCredencialAsync(
                    _muestrasOriginales);

                _capturaConfirmada = true;

                _temporizador?.Stop();

                // DEVOLVER RESULTADO POSITIVO A LA VENTANA ANTERIOR
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                BtnConfirmar.IsEnabled = true;
                BtnVolverCapturar.IsEnabled = true;

                TxtEstadoCamara.Text =
                    "No fue posible registrar la credencial facial.";

                MessageBox.Show(
                    "No fue posible registrar la credencial facial en el servidor.\n\n" +
                    ex.Message,
                    "ClinicSync",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // ==========================================
        // CANCELAR
        // ==========================================

        private void Cancelar_Click(
            object sender,
            RoutedEventArgs e)
        {
            Close();
        }


        // ==========================================
        // CERRAR VENTANA
        // ==========================================

        private void CapturaFacialWindow_Closed(
            object? sender,
            EventArgs e)
        {
            LiberarCamara();
        }


        // ==========================================
        // LIBERAR MUESTRAS
        // ==========================================
        private void LiberarMuestras()
        {
            _muestrasOriginales.Clear();
        }


        // ==========================================
        // LIBERAR CÁMARA Y RECURSOS
        // ==========================================

        private void LiberarCamara()
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

            LiberarMuestras();

            _servicioReconocimiento?.Dispose();
            _servicioReconocimiento = null;

            _servicioCamara.Dispose();

            ImgCamara.Source = null;

            _ultimaImagen = null;
        }
    }
}
