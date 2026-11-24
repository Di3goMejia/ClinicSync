
using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Face;
using Emgu.CV.Util;

namespace ClinicSyncServer.Infrastructure.Facial
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public sealed class ServicioReconocimientoFacial : IDisposable
    {
        /// <summary>
        /// The clasificador
        /// </summary>
        private readonly CascadeClassifier _clasificador;
        /// <summary>
        /// The reconocedor
        /// </summary>
        private LBPHFaceRecognizer? _reconocedor;
        /// <summary>
        /// The liberado
        /// </summary>
        private bool _liberado;

        /// <summary>
        /// The ancho rostro
        /// </summary>
        private const int AnchoRostro = 100;
        /// <summary>
        /// The alto rostro
        /// </summary>
        private const int AltoRostro = 100;
        /// <summary>
        /// The maximo bytes imagen
        /// </summary>
        private const int MaximoBytesImagen = 5 * 1024 * 1024;
        /// <summary>
        /// The maximo bytes modelo
        /// </summary>
        private const int MaximoBytesModelo = 20 * 1024 * 1024;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServicioReconocimientoFacial"/> class.
        /// </summary>
        /// <exception cref="FileNotFoundException">No se encontró el clasificador facial.</exception>
        public ServicioReconocimientoFacial()
        {
            string rutaClasificador = Path.Combine(
                AppContext.BaseDirectory,
                "Resources",
                "haarcascade_frontalface_default.xml");

            if (!File.Exists(rutaClasificador))
            {
                throw new FileNotFoundException(
                    "No se encontró el clasificador facial.",
                    rutaClasificador);
            }

            _clasificador = new CascadeClassifier(rutaClasificador);
        }

        /// <summary>
        /// Detectars the y preparar rostro.
        /// </summary>
        /// <param name="imagenBytes">The imagen bytes.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">
        /// La imagen está vacía. - imagenBytes
        /// or
        /// La imagen supera el tamaño máximo permitido. - imagenBytes
        /// or
        /// No se pudo procesar la imagen recibida.
        /// or
        /// La resolución de la imagen no está permitida.
        /// or
        /// </exception>
        public Mat DetectarYPrepararRostro(byte[] imagenBytes)
        {
            ComprobarNoLiberado();

            if (imagenBytes == null || imagenBytes.Length == 0)
            {
                throw new ArgumentException(
                    "La imagen está vacía.",
                    nameof(imagenBytes));
            }

            if (imagenBytes.Length > MaximoBytesImagen)
            {
                throw new ArgumentException(
                    "La imagen supera el tamaño máximo permitido.",
                    nameof(imagenBytes));
            }

            using Mat imagen = new Mat();

            CvInvoke.Imdecode(
                imagenBytes,
                ImreadModes.ColorBgr,
                imagen);

            if (imagen.IsEmpty)
            {
                throw new ArgumentException(
                    "No se pudo procesar la imagen recibida.");
            }

            if (imagen.Width > 1920 ||
                imagen.Height > 1080 ||
                imagen.Width < 70 ||
                imagen.Height < 70)
            {
                throw new ArgumentException(
                    "La resolución de la imagen no está permitida.");
            }

            using Mat escalaGrises = new Mat();

            CvInvoke.CvtColor(
                imagen,
                escalaGrises,
                ColorConversion.Bgr2Gray);

            Rectangle[] candidatos = _clasificador.DetectMultiScale(
                escalaGrises,
                1.1,
                8,
                new Size(70, 70));

            var rostrosValidos = candidatos
                .Where(r =>
                {
                    double proporcion = (double)r.Width / r.Height;

                    return proporcion >= 0.70 &&
                           proporcion <= 1.35 &&
                           r.Width >= 70 &&
                           r.Height >= 70;
                })
                .ToArray();

            if (rostrosValidos.Length != 1)
            {
                throw new ArgumentException(
                    rostrosValidos.Length == 0
                        ? "No se detectó ningún rostro."
                        : "Se detectaron varios rostros. " +
                          "Debe aparecer una sola persona.");
            }

            using Mat recorte = new Mat(
                escalaGrises,
                rostrosValidos[0]);

            Mat rostroPreparado = new Mat();

            try
            {
                CvInvoke.Resize(
                    recorte,
                    rostroPreparado,
                    new Size(AnchoRostro, AltoRostro));

                CvInvoke.EqualizeHist(
                    rostroPreparado,
                    rostroPreparado);

                return rostroPreparado;
            }
            catch
            {
                rostroPreparado.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Entrenars the specified muestras.
        /// </summary>
        /// <param name="muestras">The muestras.</param>
        /// <exception cref="ArgumentException">
        /// Se necesitan al menos cinco muestras faciales. - muestras
        /// or
        /// Las muestras deben ser imágenes grises de 100 x 100.
        /// </exception>
        public void Entrenar(IList<Mat> muestras)
        {
            ComprobarNoLiberado();

            if (muestras == null || muestras.Count < 5)
            {
                throw new ArgumentException(
                    "Se necesitan al menos cinco muestras faciales.",
                    nameof(muestras));
            }

            foreach (Mat muestra in muestras)
            {
                if (muestra == null ||
                    muestra.IsEmpty ||
                    muestra.Width != AnchoRostro ||
                    muestra.Height != AltoRostro ||
                    muestra.NumberOfChannels != 1)
                {
                    throw new ArgumentException(
                        "Las muestras deben ser imágenes grises de 100 x 100.");
                }
            }

            using var imagenes = new VectorOfMat();
            using var etiquetas = new VectorOfInt();

            foreach (Mat muestra in muestras)
            {
                imagenes.Push(muestra);
            }

            // Todas las muestras pertenecen al mismo usuario.
            int[] etiquetasUsuario = Enumerable
                .Repeat(1, muestras.Count)
                .ToArray();

            etiquetas.Push(etiquetasUsuario);

            var nuevoReconocedor = new LBPHFaceRecognizer(
                1,
                8,
                8,
                8,
                100);

            try
            {
                nuevoReconocedor.Train(imagenes, etiquetas);

                LBPHFaceRecognizer? reconocedorAnterior = _reconocedor;
                _reconocedor = nuevoReconocedor;

                reconocedorAnterior?.Dispose();
            }
            catch
            {
                nuevoReconocedor.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Exportars the modelo bytes.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">El modelo facial tiene un tamaño no permitido.</exception>
        public byte[] ExportarModeloBytes()
        {
            ComprobarNoLiberado();
            ComprobarReconocedor();

            string rutaTemporal = CrearRutaTemporal();

            try
            {
                _reconocedor!.Write(rutaTemporal);

                byte[] modeloBytes = File.ReadAllBytes(rutaTemporal);

                if (modeloBytes.Length == 0 ||
                    modeloBytes.Length > MaximoBytesModelo)
                {
                    throw new InvalidOperationException(
                        "El modelo facial tiene un tamaño no permitido.");
                }

                return modeloBytes;
            }
            finally
            {
                EliminarArchivoTemporal(rutaTemporal);
            }
        }

        /// <summary>
        /// Cargars the modelo desde bytes.
        /// </summary>
        /// <param name="datosModelo">The datos modelo.</param>
        /// <exception cref="ArgumentException">Los datos del modelo facial son inválidos. - datosModelo</exception>
        public void CargarModeloDesdeBytes(byte[] datosModelo)
        {
            ComprobarNoLiberado();

            if (datosModelo == null ||
                datosModelo.Length == 0 ||
                datosModelo.Length > MaximoBytesModelo)
            {
                throw new ArgumentException(
                    "Los datos del modelo facial son inválidos.",
                    nameof(datosModelo));
            }

            string rutaTemporal = CrearRutaTemporal();

            var nuevoReconocedor = new LBPHFaceRecognizer(
                1,
                8,
                8,
                8,
                100);

            try
            {
                File.WriteAllBytes(rutaTemporal, datosModelo);

                nuevoReconocedor.Read(rutaTemporal);

                LBPHFaceRecognizer? reconocedorAnterior = _reconocedor;
                _reconocedor = nuevoReconocedor;

                reconocedorAnterior?.Dispose();
            }
            catch
            {
                nuevoReconocedor.Dispose();
                throw;
            }
            finally
            {
                EliminarArchivoTemporal(rutaTemporal);
            }
        }

        /// <summary>
        /// Predecirs the specified imagen bytes.
        /// </summary>
        /// <param name="imagenBytes">The imagen bytes.</param>
        /// <returns></returns>
        public (int Etiqueta, double Distancia) Predecir(
            byte[] imagenBytes)
        {
            ComprobarNoLiberado();
            ComprobarReconocedor();

            using Mat rostroPreparado =
                DetectarYPrepararRostro(imagenBytes);

            var resultado = _reconocedor!.Predict(rostroPreparado);

            return (
                resultado.Label,
                resultado.Distance
            );
        }

        /// <summary>
        /// Crears the ruta temporal.
        /// </summary>
        /// <returns></returns>
        private static string CrearRutaTemporal()
        {
            return Path.Combine(
                Path.GetTempPath(),
                $"ClinicSync_{Guid.NewGuid():N}.yml");
        }

        /// <summary>
        /// Eliminars the archivo temporal.
        /// </summary>
        /// <param name="ruta">The ruta.</param>
        private static void EliminarArchivoTemporal(string ruta)
        {
            try
            {
                if (File.Exists(ruta))
                {
                    File.Delete(ruta);
                }
            }
            catch (IOException)
            {
                // La limpieza no debe ocultar el error original.
            }
            catch (UnauthorizedAccessException)
            {
                // La limpieza no debe ocultar el error original.
            }
        }

        /// <summary>
        /// Comprobars the reconocedor.
        /// </summary>
        /// <exception cref="InvalidOperationException">Primero debes entrenar o cargar un modelo facial.</exception>
        private void ComprobarReconocedor()
        {
            if (_reconocedor == null)
            {
                throw new InvalidOperationException(
                    "Primero debes entrenar o cargar un modelo facial.");
            }
        }

        /// <summary>
        /// Comprobars the no liberado.
        /// </summary>
        /// <exception cref="ObjectDisposedException">ServicioReconocimientoFacial</exception>
        private void ComprobarNoLiberado()
        {
            if (_liberado)
            {
                throw new ObjectDisposedException(
                    nameof(ServicioReconocimientoFacial));
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_liberado)
                return;

            _reconocedor?.Dispose();
            _clasificador.Dispose();

            _reconocedor = null;
            _liberado = true;

            GC.SuppressFinalize(this);
        }
    }
}
