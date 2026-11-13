
using ClinicSyncDesktop.Data;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicSyncDesktop.Services
{
    public sealed class FacialApiService
    {
        // REGISTRAR CREDENCIAL FACIAL
        public async Task RegistrarCredencialAsync(
            IReadOnlyCollection<string> muestras,
            CancellationToken ct = default)
        {
            if (muestras == null ||
                muestras.Count < 5 ||
                muestras.Count > 10)
            {
                throw new ArgumentException(
                    "Se necesitan entre 5 y 10 muestras faciales.",
                    nameof(muestras));
            }

            using var respuesta =
                await ClienteApi.PostAutenticadoAsync(
                    "api/auth/2fa/facial",
                    new { Muestras = muestras },
                    ct);

            await VerificarRespuestaAsync(respuesta, ct);
        }

        // DESACTIVAR CREDENCIAL FACIAL
        public async Task DesactivarCredencialAsync()
        {
            using var respuesta =
                await ClienteApi.DeleteAutenticadoAsync(
                    "api/auth/2fa/facial");

            await VerificarRespuestaAsync(respuesta);
        }

        // INTERPRETAR RESPUESTA DE LA API
        private static async Task VerificarRespuestaAsync(
            HttpResponseMessage respuesta,
            CancellationToken ct = default)
        {
            if (respuesta.IsSuccessStatusCode)
                return;

            string contenido =
                await respuesta.Content.ReadAsStringAsync(ct);

            string mensaje =
                "No fue posible completar la operación de reconocimiento facial.";

            try
            {
                using var documento =
                    JsonDocument.Parse(contenido);

                if (documento.RootElement.TryGetProperty(
                    "detail", out var detalle))
                {
                    mensaje = detalle.GetString() ?? mensaje;
                }
                else if (documento.RootElement.TryGetProperty(
                    "mensaje", out var mensajeApi))
                {
                    mensaje = mensajeApi.GetString() ?? mensaje;
                }
            }
            catch (JsonException)
            {
            }

            throw new HttpRequestException(
                mensaje,
                null,
                respuesta.StatusCode);
        }
    }
}
