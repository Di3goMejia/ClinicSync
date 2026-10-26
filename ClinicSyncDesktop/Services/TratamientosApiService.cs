using ClinicSyncDesktop.Data;
using ClinicSyncDesktop.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using System.Diagnostics;

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

        public async Task<Tratamiento> ModificarAsync(int idTratamiento,
            string nombre,
            decimal precio,
            string? descripcion = null,
            CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            using var respuesta = await ClienteApi.PutAutenticadoAsync(
                $"api/tratamientos/{idTratamiento}", 
                new { nombre, precio, descripcion },
                ct
            );


            if (!respuesta.IsSuccessStatusCode)
            {
                string contenido =
                    await respuesta.Content.ReadAsStringAsync(ct);

                string mensaje = "No fue posible editar el tratamiento.";

                try
                {
                    using var documento = JsonDocument.Parse(contenido);

                    if (documento.RootElement.TryGetProperty(
                        "detail", out var detalle))
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

            var resultado = await respuesta.Content.ReadFromJsonAsync
                <Tratamiento>(cancellationToken: ct);

            return resultado ?? throw new HttpRequestException(
                "La API no devolvió el nuevo tratamiento.");
        }

        public async Task CambiarEstadoAsync(int idTratamiento,
            bool activo,
            CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            using var respuesta = await ClienteApi.PatchAutenticadoAsync(
                $"api/tratamientos/{idTratamiento}/estado",
                new { activo },
                ct
            );


            if (!respuesta.IsSuccessStatusCode)
            {
                string contenido =
                    await respuesta.Content.ReadAsStringAsync(ct);

                string mensaje = "No fue posible cambiar el estado del tratamiento.";

                try
                {
                    using var documento = JsonDocument.Parse(contenido);

                    if (documento.RootElement.TryGetProperty(
                        "detail", out var detalle))
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
                        "detail", out var detalle))
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
