
using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Face;
using Emgu.CV.Util;

namespace ClinicSyncServer.Infrastructure.Facial
{
    public sealed class ServicioReconocimientoFacial : IDisposable
    {
        private readonly CascadeClassifier _clasificador;
        private LBPHFaceRecognizer? _reconocedor;
        private bool _liberado;

        private const int AnchoRostro = 100;
        private const int AltoRostro = 100;
        private const int MaximoBytesImagen = 5 * 1024 * 1024;
        private const int MaximoBytesModelo = 20 * 1024 * 1024;

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

        private static string CrearRutaTemporal()
        {
            return Path.Combine(
                Path.GetTempPath(),
                $"ClinicSync_{Guid.NewGuid():N}.yml");
        }

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

        private void ComprobarReconocedor()
        {
            if (_reconocedor == null)
            {
                throw new InvalidOperationException(
                    "Primero debes entrenar o cargar un modelo facial.");
            }
        }

        private void ComprobarNoLiberado()
        {
            if (_liberado)
            {
                throw new ObjectDisposedException(
                    nameof(ServicioReconocimientoFacial));
            }
        }

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
