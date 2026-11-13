
using System.Security.Cryptography;
using Emgu.CV;
using ClinicSyncServer.Features.Auth.Data;
using ClinicSyncServer.Infrastructure.Facial;

namespace ClinicSyncServer.Features.Auth.Services
{
    public sealed class RegistroCredencialFacialService(
        Metodos2FARepository metodos2FA,
        ServicioReconocimientoFacial reconocimiento)
    {
        private const int MinimoMuestras = 5;
        private const int MaximoMuestras = 10;
        private const int MaximoBytesImagen = 5 * 1024 * 1024;

        public async Task RegistrarAsync(
            int idUsuario,
            IList<string> muestras,
            CancellationToken ct = default)
        {
            if (idUsuario <= 0)
            {
                throw new ArgumentException(
                    "El identificador del usuario no es válido.");
            }

            if (muestras == null ||
                muestras.Count < MinimoMuestras ||
                muestras.Count > MaximoMuestras)
            {
                throw new ArgumentException(
                    "Se necesitan entre 5 y 10 muestras faciales.");
            }

            if (await metodos2FA.TieneMetodoActivoAsync(idUsuario, ct))
            {
                throw new InvalidOperationException(
                    "El usuario ya tiene un método facial activo.");
            }

            var rostros = new List<Mat>();

            byte[]? modeloBytes = null;

            try
            {
                foreach (string muestra in muestras)
                {
                    ct.ThrowIfCancellationRequested();

                    if (string.IsNullOrWhiteSpace(muestra))
                    {
                        throw new ArgumentException(
                            "Una de las muestras está vacía.");
                    }

                    int maximoCaracteres =
                        ((MaximoBytesImagen + 2) / 3) * 4;

                    if (muestra.Length > maximoCaracteres)
                    {
                        throw new ArgumentException(
                            "Una imagen supera el tamaño permitido.");
                    }

                    byte[] imagenBytes =
                        Convert.FromBase64String(muestra);

                    try
                    {
                        if (imagenBytes.Length == 0 ||
                            imagenBytes.Length > MaximoBytesImagen)
                        {
                            throw new ArgumentException(
                                "Una imagen tiene un tamaño no permitido.");
                        }

                        Mat rostro =
                            reconocimiento.DetectarYPrepararRostro(
                                imagenBytes);

                        rostros.Add(rostro);
                    }
                    finally
                    {
                        CryptographicOperations.ZeroMemory(imagenBytes);
                    }
                }

                ct.ThrowIfCancellationRequested();

                reconocimiento.Entrenar(rostros);

                modeloBytes =
                    reconocimiento.ExportarModeloBytes();

                ct.ThrowIfCancellationRequested();

                await metodos2FA.RegistrarAsync(
                    idUsuario,
                    modeloBytes,
                    ct);
            }
            finally
            {
                foreach (Mat rostro in rostros)
                {
                    rostro.Dispose();
                }

                if (modeloBytes != null)
                {
                    CryptographicOperations.ZeroMemory(modeloBytes);
                }
            }
        }
    }
}
