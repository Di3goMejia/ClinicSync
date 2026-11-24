using Amazon.Runtime.Internal.Util;
using ClinicSyncServer.Features.Auth.Data;
using ClinicSyncServer.Infrastructure.Facial;
using System.Globalization;
using System.Security.Cryptography;


namespace ClinicSyncServer.Features.Auth.Factores
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="ClinicSyncServer.Features.Auth.Factores.IFactorSegundoService" />
    public sealed class FactorSegundoFacial(
        Metodos2FARepository metodos2FA,
        ServicioReconocimientoFacial reconocimiento,
        IConfiguration config) : IFactorSegundoService
    {
        /// <summary>
        /// The maximo bytes imagen
        /// </summary>
        private const int MaximoBytesImagen = 5 * 1024 * 1024;

        /// <summary>
        /// Verificars the asynchronous.
        /// </summary>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="prueba">The prueba.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">El umbral de reconocimiento facial no está " +
        ///                         "configurado correctamente.</exception>
        public async Task<bool> VerificarAsync(
            int idUsuario,
            string prueba,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(prueba))
                return false;

            // Limitar el tamaño antes de convertir Base64 a bytes.
            int maximoCaracteresBase64 =
                ((MaximoBytesImagen + 2) / 3) * 4;

            if (prueba.Length > maximoCaracteresBase64)
                return false;

            byte[]? imagenBytes = null;
            byte[]? modeloBytes = null;

            try
            {
                imagenBytes = Convert.FromBase64String(prueba);

                if (imagenBytes.Length == 0 ||
                    imagenBytes.Length > MaximoBytesImagen)
                {
                    return false;
                }

                // Recuperar el modelo facial descifrado.
                modeloBytes =
                    await metodos2FA.ObtenerCredencialActivaAsync(
                        idUsuario, ct);

                if (modeloBytes == null || modeloBytes.Length == 0)
                    return false;

                // Cargar el modelo correspondiente al usuario.
                reconocimiento.CargarModeloDesdeBytes(modeloBytes);

                // Comparar la captura con el modelo almacenado.
                var resultado = reconocimiento.Predecir(imagenBytes);

                // El umbral debe estar calibrado y configurado.
                string? valorUmbral =
                    config["Facial:UmbralDistancia"];

                if (!double.TryParse(
                        valorUmbral,
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out double umbral) ||
                    !double.IsFinite(umbral) ||
                    umbral <= 0)
                {
                    throw new InvalidOperationException(
                        "El umbral de reconocimiento facial no está " +
                        "configurado correctamente.");
                }

                bool etiquetaCorrecta = resultado.Etiqueta == 1;

                bool distanciaAceptable =
                    double.IsFinite(resultado.Distancia) &&
                    resultado.Distancia <= umbral;

                return etiquetaCorrecta && distanciaAceptable;
            }
            catch (FormatException)
            {
                // La prueba recibida no tiene formato Base64 válido.
                return false;
            }
            catch (ArgumentException)
            {
                // La imagen no cumple las validaciones faciales.
                return false;
            }
            catch (CryptographicException)
            {
                // La credencial no pudo descifrarse.
                return false;
            }
            finally
            {
                if (imagenBytes != null)
                    CryptographicOperations.ZeroMemory(imagenBytes);

                if (modeloBytes != null)
                    CryptographicOperations.ZeroMemory(modeloBytes);
            }
        }
    }
}
