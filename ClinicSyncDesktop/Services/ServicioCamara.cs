using System;
using Emgu.CV;

namespace ClinicSyncDesktop.Services
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class ServicioCamara : IDisposable
    {
        /// <summary>
        /// The camara
        /// </summary>
        private VideoCapture? _camara;
        /// <summary>
        /// The liberado
        /// </summary>
        private bool _liberado;

        /// <summary>
        /// Gets a value indicating whether [esta activa].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [esta activa]; otherwise, <c>false</c>.
        /// </value>
        public bool EstaActiva =>
            _camara != null && _camara.IsOpened;

        /// <summary>
        /// Iniciars the specified indice camara.
        /// </summary>
        /// <param name="indiceCamara">The indice camara.</param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException">ServicioCamara</exception>
        public bool Iniciar(int indiceCamara = 0)
        {
            if (_liberado)
            {
                throw new ObjectDisposedException(
                    nameof(ServicioCamara));
            }

            if (EstaActiva)
                return true;

            _camara?.Dispose();
            _camara = null;

            _camara = new VideoCapture(indiceCamara);

            if (!_camara.IsOpened)
            {
                _camara.Dispose();
                _camara = null;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Obteners the fotograma.
        /// </summary>
        /// <returns></returns>
        public Mat? ObtenerFotograma()
        {
            if (!EstaActiva)
                return null;

            Mat fotograma = _camara!.QueryFrame();

            if (fotograma.IsEmpty)
            {
                fotograma.Dispose();
                return null;
            }

            return fotograma;
        }

        /// <summary>
        /// Deteners this instance.
        /// </summary>
        public void Detener()
        {
            _camara?.Dispose();
            _camara = null;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_liberado)
                return;

            Detener();
            _liberado = true;

            GC.SuppressFinalize(this);
        }
    }
}
