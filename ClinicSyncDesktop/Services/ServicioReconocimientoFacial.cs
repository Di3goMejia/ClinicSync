using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Face;
using Emgu.CV.Util;
using Emgu.CV.Structure;
using System.Linq;
using System.Text.Json;

namespace ClinicSyncDesktop.Services
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class ServicioReconocimientoFacial : IDisposable
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
        /// Initializes a new instance of the <see cref="ServicioReconocimientoFacial"/> class.
        /// </summary>
        /// <exception cref="FileNotFoundException">No se encontró el clasificador facial. " +
        ///                     "Comprueba que el archivo XML se copie al directorio de salida.</exception>
        public ServicioReconocimientoFacial()
        {
            string rutaClasificador = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Resources",
                "haarcascade_frontalface_default.xml");

            if (!File.Exists(rutaClasificador))
            {
                throw new FileNotFoundException(
                    "No se encontró el clasificador facial. " +
                    "Comprueba que el archivo XML se copie al directorio de salida.",
                    rutaClasificador);
            }

            _clasificador = new CascadeClassifier(rutaClasificador);
        }

        /// <summary>
        /// Detectars the rostros.
        /// </summary>
        /// <param name="imagen">The imagen.</param>
        /// <returns></returns>
        public Rectangle[] DetectarRostros(Mat imagen)
        {
            ComprobarNoLiberado();

            if (imagen == null || imagen.IsEmpty)
                return Array.Empty<Rectangle>();

            using Mat escalaGrises = ConvertirAEscalaGrises(imagen);

            Rectangle[] candidatos = _clasificador.DetectMultiScale(
                escalaGrises,
                1.1,
                8,
                new Size(70, 70));

            var filtrados = new List<Rectangle>();

            foreach (Rectangle rostro in candidatos)
            {
                double proporcion =
                    (double)rostro.Width / rostro.Height;

                if (proporcion >= 0.70 &&
                    proporcion <= 1.35 &&
                    rostro.Width >= 70 &&
                    rostro.Height >= 70)
                {
                    filtrados.Add(rostro);
                }
            }

            return filtrados.ToArray();
        }

        /// <summary>
        /// Preparars the rostro.
        /// </summary>
        /// <param name="imagen">The imagen.</param>
        /// <param name="rostro">The rostro.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">
        /// La imagen está vacía.
        /// or
        /// La región del rostro no es válida. - rostro
        /// </exception>
        public Mat PrepararRostro(Mat imagen, Rectangle rostro)
        {
            ComprobarNoLiberado();

            if (imagen == null || imagen.IsEmpty)
                throw new ArgumentException("La imagen está vacía.");

            Rectangle limites = new Rectangle(
                0,
                0,
                imagen.Width,
                imagen.Height);

            if (rostro.Width <= 0 ||
                rostro.Height <= 0 ||
                !limites.Contains(rostro))
            {
                throw new ArgumentException(
                    "La región del rostro no es válida.",
                    nameof(rostro));
            }

            using Mat escalaGrises = ConvertirAEscalaGrises(imagen);

            Mat recorte = new Mat(escalaGrises, rostro);
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
            finally
            {
                recorte.Dispose();
            }
        }


        /// <summary>
        /// Serializars the muestras.
        /// </summary>
        /// <param name="muestras">The muestras.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">
        /// Debes proporcionar al menos una muestra facial. - muestras
        /// or
        /// Cada muestra debe ser una imagen gris de 100 x 100 píxeles.
        /// </exception>
        public byte[] SerializarMuestras(IList<Mat> muestras)
        {
            ComprobarNoLiberado();

            if (muestras == null || muestras.Count == 0)
                throw new ArgumentException(
                    "Debes proporcionar al menos una muestra facial.",
                    nameof(muestras));

            var muestrasSerializadas = new List<byte[]>();

            foreach (Mat muestra in muestras)
            {
                if (muestra == null ||
                    muestra.IsEmpty ||
                    muestra.Width != AnchoRostro ||
                    muestra.Height != AltoRostro ||
                    muestra.NumberOfChannels != 1)
                {
                    throw new ArgumentException(
                        "Cada muestra debe ser una imagen gris de 100 x 100 píxeles.");
                }

                byte[] pixeles = new byte[AnchoRostro * AltoRostro];
                muestra.CopyTo(pixeles);
                muestrasSerializadas.Add(pixeles);
            }

            return JsonSerializer.SerializeToUtf8Bytes(muestrasSerializadas);
        }


        /// <summary>
        /// Deserializars the muestras.
        /// </summary>
        /// <param name="datos">The datos.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">
        /// Los datos de las muestras están vacíos. - datos
        /// or
        /// No se encontraron muestras faciales válidas.
        /// or
        /// Una muestra facial tiene un tamaño incorrecto.
        /// </exception>
        public List<Mat> DeserializarMuestras(byte[] datos)
        {
            ComprobarNoLiberado();

            if (datos == null || datos.Length == 0)
                throw new ArgumentException(
                    "Los datos de las muestras están vacíos.",
                    nameof(datos));

            List<byte[]>? muestrasSerializadas =
                JsonSerializer.Deserialize<List<byte[]>>(datos);

            if (muestrasSerializadas == null ||
                muestrasSerializadas.Count == 0)
            {
                throw new ArgumentException(
                    "No se encontraron muestras faciales válidas.");
            }

            var muestras = new List<Mat>();

            try
            {
                foreach (byte[] pixeles in muestrasSerializadas)
                {
                    if (pixeles == null ||
                        pixeles.Length != AnchoRostro * AltoRostro)
                    {
                        throw new ArgumentException(
                            "Una muestra facial tiene un tamaño incorrecto.");
                    }

                    Mat muestra = new Mat(
                        AltoRostro,
                        AnchoRostro,
                        DepthType.Cv8U,
                        1);

                    try
                    {
                        muestra.SetTo(new Emgu.CV.Structure.MCvScalar(0));

                        System.Runtime.InteropServices.Marshal.Copy(
                            pixeles,
                            0,
                            muestra.DataPointer,
                            pixeles.Length);

                        muestras.Add(muestra);
                    }
                    catch
                    {
                        muestra.Dispose();
                        throw;
                    }
                }

                return muestras;
            }
            catch
            {
                foreach (Mat muestra in muestras)
                    muestra.Dispose();

                throw;
            }
        }

        /// <summary>
        /// Recuperars the muestras desde credencial.
        /// </summary>
        /// <param name="credencial">The credencial.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">La credencial facial está vacía. - credencial</exception>
        public List<Mat> RecuperarMuestrasDesdeCredencial(byte[] credencial)
        {
            ComprobarNoLiberado();

            if (credencial == null || credencial.Length == 0)
            {
                throw new ArgumentException(
                    "La credencial facial está vacía.",
                    nameof(credencial));
            }

            return DeserializarMuestras(credencial);
        }

        /// <summary>
        /// Entrenars the specified rostros.
        /// </summary>
        /// <param name="rostros">The rostros.</param>
        /// <param name="etiquetas">The etiquetas.</param>
        /// <exception cref="ArgumentNullException">Las imágenes y las etiquetas son obligatorias.</exception>
        /// <exception cref="ArgumentException">
        /// Debe existir una etiqueta por cada imagen facial.
        /// or
        /// Cada rostro debe ser una imagen gris de 100 x 100 píxeles.
        /// </exception>
        public void Entrenar(
            IList<Mat> rostros,
            IList<int> etiquetas)
        {
            ComprobarNoLiberado();

            if (rostros == null || etiquetas == null)
                throw new ArgumentNullException(
                    "Las imágenes y las etiquetas son obligatorias.");

            if (rostros.Count == 0 ||
                rostros.Count != etiquetas.Count)
            {
                throw new ArgumentException(
                    "Debe existir una etiqueta por cada imagen facial.");
            }

            foreach (Mat rostro in rostros)
            {
                if (rostro == null ||
                    rostro.IsEmpty ||
                    rostro.Width != AnchoRostro ||
                    rostro.Height != AltoRostro ||
                    rostro.NumberOfChannels != 1)
                {
                    throw new ArgumentException(
                        "Cada rostro debe ser una imagen gris de 100 x 100 píxeles.");
                }
            }

            using var imagenes = new VectorOfMat();
            using var etiquetasVector = new VectorOfInt();

            foreach (Mat rostro in rostros)
                imagenes.Push(rostro);

            etiquetasVector.Push(etiquetas.ToArray());

            var nuevoReconocedor = new LBPHFaceRecognizer(
                1,
                8,
                8,
                8,
                100);

            try
            {
                nuevoReconocedor.Train(
                    imagenes,
                    etiquetasVector);

                _reconocedor?.Dispose();
                _reconocedor = nuevoReconocedor;
            }
            catch
            {
                nuevoReconocedor.Dispose();
                throw;
            }
        }


        /// <summary>
        /// Guardars the modelo.
        /// </summary>
        /// <param name="rutaArchivo">The ruta archivo.</param>
        /// <exception cref="ArgumentException">Debes indicar la ruta del modelo. - rutaArchivo</exception>
        public void GuardarModelo(string rutaArchivo)
        {
            ComprobarNoLiberado();
            ComprobarReconocedor();

            if (string.IsNullOrWhiteSpace(rutaArchivo))
                throw new ArgumentException(
                    "Debes indicar la ruta del modelo.",
                    nameof(rutaArchivo));

            string? directorio = Path.GetDirectoryName(
                Path.GetFullPath(rutaArchivo));

            if (!string.IsNullOrEmpty(directorio))
                Directory.CreateDirectory(directorio);

            _reconocedor!.Write(rutaArchivo);
        }


        /// <summary>
        /// Exportars the modelo bytes.
        /// </summary>
        /// <returns></returns>
        public byte[] ExportarModeloBytes()
        {
            ComprobarNoLiberado();
            ComprobarReconocedor();

            string rutaTemporal = Path.Combine(
                Path.GetTempPath(),
                $"ClinicSync_{Guid.NewGuid():N}.yml");

            try
            {
                _reconocedor!.Write(rutaTemporal);
                return File.ReadAllBytes(rutaTemporal);
            }
            finally
            {
                if (File.Exists(rutaTemporal))
                    File.Delete(rutaTemporal);
            }
        }

        /// <summary>
        /// Cargars the modelo desde bytes.
        /// </summary>
        /// <param name="datosModelo">The datos modelo.</param>
        /// <exception cref="ArgumentException">Los datos del modelo facial están vacíos. - datosModelo</exception>
        public void CargarModeloDesdeBytes(byte[] datosModelo)
        {
            ComprobarNoLiberado();

            if (datosModelo == null || datosModelo.Length == 0)
                throw new ArgumentException(
                    "Los datos del modelo facial están vacíos.",
                    nameof(datosModelo));

            string rutaTemporal = Path.Combine(
                Path.GetTempPath(),
                $"ClinicSync_{Guid.NewGuid():N}.yml");

            try
            {
                File.WriteAllBytes(rutaTemporal, datosModelo);
                CargarModelo(rutaTemporal);
            }
            finally
            {
                if (File.Exists(rutaTemporal))
                    File.Delete(rutaTemporal);
            }
        }


        /// <summary>
        /// Cargars the modelo.
        /// </summary>
        /// <param name="rutaArchivo">The ruta archivo.</param>
        /// <exception cref="FileNotFoundException">No se encontró el modelo facial entrenado.</exception>
        public void CargarModelo(string rutaArchivo)
        {
            ComprobarNoLiberado();

            if (!File.Exists(rutaArchivo))
            {
                throw new FileNotFoundException(
                    "No se encontró el modelo facial entrenado.",
                    rutaArchivo);
            }

            var nuevoReconocedor = new LBPHFaceRecognizer(
                1,
                8,
                8,
                8,
                100);

            try
            {
                nuevoReconocedor.Read(rutaArchivo);

                _reconocedor?.Dispose();
                _reconocedor = nuevoReconocedor;
            }
            catch
            {
                nuevoReconocedor.Dispose();
                throw;
            }
        }


        /// <summary>
        /// Intentars the reconocer.
        /// </summary>
        /// <param name="imagen">The imagen.</param>
        /// <param name="rostro">The rostro.</param>
        /// <param name="etiqueta">The etiqueta.</param>
        /// <param name="distancia">The distancia.</param>
        /// <returns></returns>
        public bool IntentarReconocer(
            Mat imagen,
            Rectangle rostro,
            out int etiqueta,
            out double distancia)
        {
            ComprobarNoLiberado();
            ComprobarReconocedor();

            etiqueta = -1;
            distancia = double.MaxValue;

            using Mat rostroPreparado = PrepararRostro(
                imagen,
                rostro);

            var resultado = _reconocedor!.Predict(rostroPreparado);

            etiqueta = resultado.Label;
            distancia = resultado.Distance;


            return etiqueta >= 0 &&
                   distancia <= 100;
        }

        /// <summary>
        /// Convertirs a escala grises.
        /// </summary>
        /// <param name="imagen">The imagen.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">La imagen debe tener 1, 3 o 4 canales.</exception>
        private static Mat ConvertirAEscalaGrises(Mat imagen)
        {
            Mat escalaGrises = new Mat();

            try
            {
                if (imagen.NumberOfChannels == 1)
                {
                    imagen.CopyTo(escalaGrises);
                }
                else if (imagen.NumberOfChannels == 3)
                {
                    CvInvoke.CvtColor(
                        imagen,
                        escalaGrises,
                        ColorConversion.Bgr2Gray);
                }
                else if (imagen.NumberOfChannels == 4)
                {
                    CvInvoke.CvtColor(
                        imagen,
                        escalaGrises,
                        ColorConversion.Bgra2Gray);
                }
                else
                {
                    throw new ArgumentException(
                        "La imagen debe tener 1, 3 o 4 canales.");
                }

                return escalaGrises;
            }
            catch
            {
                escalaGrises.Dispose();
                throw;
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

            _clasificador.Dispose();
            _reconocedor?.Dispose();

            _liberado = true;

            GC.SuppressFinalize(this);
        }
    }
}
