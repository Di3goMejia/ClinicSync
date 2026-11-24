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
    /// <summary>
    /// 
    /// </summary>
    public class TratamientosApiService
    {
        /// <summary>
        /// Listars the asynchronous.
        /// </summary>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public async Task<List<Tratamiento>> ListarAsync(
            CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            return await ClienteApi.ObtenerAutenticadoAsync<
                List<Tratamiento>>("api/tratamientos/");
        }

        /// <summary>
        /// Modificars the asynchronous.
        /// </summary>
        /// <param name="idTratamiento">The identifier tratamiento.</param>
        /// <param name="nombre">The nombre.</param>
        /// <param name="precio">The precio.</param>
        /// <param name="descripcion">The descripcion.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        /// <exception cref="HttpRequestException">
        /// null
        /// or
        /// La API no devolvió el nuevo tratamiento.
        /// </exception>
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

        /// <summary>
        /// Cambiars the estado asynchronous.
        /// </summary>
        /// <param name="idTratamiento">The identifier tratamiento.</param>
        /// <param name="activo">if set to <c>true</c> [activo].</param>
        /// <param name="ct">The ct.</param>
        /// <exception cref="HttpRequestException">null</exception>
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

        /// <summary>
        /// Registrars the asynchronous.
        /// </summary>
        /// <param name="nombre">The nombre.</param>
        /// <param name="precio">The precio.</param>
        /// <param name="descripcion">The descripcion.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        /// <exception cref="HttpRequestException">
        /// null
        /// or
        /// La API no devolvió el identificador del tratamiento.
        /// </exception>
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

        /// <summary>
        /// 
        /// </summary>
        private sealed class RegistroTratamientoRespuesta
        {
            /// <summary>
            /// Gets or sets the identifier tratamiento.
            /// </summary>
            /// <value>
            /// The identifier tratamiento.
            /// </value>
            public int IdTratamiento { get; set; }
        }
    }
}
