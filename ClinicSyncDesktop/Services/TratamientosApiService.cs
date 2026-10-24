using ClinicSyncDesktop.Data;
using ClinicSyncDesktop.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;

namespace ClinicSyncDesktop.Services
{
    public class TratamientosApiService
    {
        public async Task<List<Tratamiento>> ListarAsync(
            CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            return await ClienteApi.ObtenerAutenticadoAsync<
                List<Tratamiento>>("api/tratamientos/");
        }

        public async Task<int> RegistrarAsync(
            string nombre,
            decimal precio,
            string? descripcion = null,
            CancellationToken ct = default)
        {
            var solicitud = new
            {
                Nombre = nombre,
                Precio = precio,
                Descripcion = descripcion
            };

            using var respuesta =
                await ClienteApi.PostAutenticadoAsync(
                    "api/tratamientos/",
                    solicitud,
                    ct);

            if (!respuesta.IsSuccessStatusCode)
            {
                string contenido =
                    await respuesta.Content.ReadAsStringAsync(ct);

                string mensaje = "No fue posible registrar el tratamiento.";

                try
                {
                    using var documento = JsonDocument.Parse(contenido);

                    if (documento.RootElement.TryGetProperty(
                        "mensaje", out var detalle))
                    {
                        mensaje = detalle.GetString() ?? mensaje;
                    }
                }
                catch (JsonException)
                {
                    // Conservar el mensaje general.
                }

                throw new HttpRequestException(
                    mensaje,
                    null,
                    respuesta.StatusCode);
            }

            var resultado =
                await respuesta.Content.ReadFromJsonAsync<
                    RegistroTratamientoRespuesta>(
                        cancellationToken: ct);

            return resultado?.IdTratamiento
                ?? throw new HttpRequestException(
                    "La API no devolvió el identificador del tratamiento.");
        }

        private sealed class RegistroTratamientoRespuesta
        {
            public int IdTratamiento { get; set; }
        }
    }
}
