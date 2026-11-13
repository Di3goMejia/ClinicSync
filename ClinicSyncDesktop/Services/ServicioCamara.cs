using System;
using Emgu.CV;

namespace ClinicSyncDesktop.Services
{
    public class ServicioCamara : IDisposable
    {
        private VideoCapture? _camara;
        private bool _liberado;

        public bool EstaActiva =>
            _camara != null && _camara.IsOpened;

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

        public void Detener()
        {
            _camara?.Dispose();
            _camara = null;
        }

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
