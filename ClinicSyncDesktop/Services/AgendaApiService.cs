using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ClinicSyncDesktop.Data;
using ClinicSyncDesktop.Models;
using System.Linq;

namespace ClinicSyncDesktop.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class AgendaApiService
    {
        // Las fechas viajan SIN zona horaria (ej. 2026-11-10T09:00:00).
        /// <summary>
        /// Fechas the specified fecha.
        /// </summary>
        /// <param name="fecha">The fecha.</param>
        /// <returns></returns>
        private static string Fecha(DateTime fecha) =>
            fecha.ToString("yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture);

        // CALENDARIO (vistas Día / Semana / Mes)
        /// <summary>
        /// Listars the asynchronous.
        /// </summary>
        /// <param name="desde">The desde.</param>
        /// <param name="hasta">The hasta.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public async Task<List<EventoAgenda>> ListarAsync(
            DateTime desde,
            DateTime hasta,
            CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            string ruta =
                $"api/agenda?desde={Uri.EscapeDataString(Fecha(desde))}" +
                $"&hasta={Uri.EscapeDataString(Fecha(hasta))}";

            return await ClienteApi.ObtenerAutenticadoAsync<List<EventoAgenda>>(ruta);
        }

        // PRÓXIMAS CITAS ("Ver más..." = una cantidad mayor)
        /// <summary>
        /// Proximases the asynchronous.
        /// </summary>
        /// <param name="cantidad">The cantidad.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public async Task<List<ProximaCita>> ProximasAsync(
            int cantidad = 4,
            CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            string ruta =
                $"api/agenda/proximas?desde={Uri.EscapeDataString(Fecha(DateTime.Now))}" +
                $"&cantidad={cantidad}";

            return await ClienteApi.ObtenerAutenticadoAsync<List<ProximaCita>>(ruta);
        }

        // ¿EL HORARIO ESTÁ LIBRE?
        /// <summary>
        /// Verificars the disponibilidad asynchronous.
        /// </summary>
        /// <param name="inicio">The inicio.</param>
        /// <param name="fin">The fin.</param>
        /// <param name="ct">The ct.</param>
        /// <param name="idEventoExcluir">The identifier evento excluir.</param>
        /// <returns></returns>
        public async Task<DisponibilidadAgenda> VerificarDisponibilidadAsync(
            DateTime inicio,
            DateTime fin,
            CancellationToken ct = default,
            int? idEventoExcluir = null)
        {
            ct.ThrowIfCancellationRequested();

            string ruta =
                $"api/agenda/disponibilidad?fechaInicio={Uri.EscapeDataString(Fecha(inicio))}" +
                $"&fechaFinal={Uri.EscapeDataString(Fecha(fin))}";

            if (idEventoExcluir.HasValue)
            {
                ruta += $"&idEventoExcluir={idEventoExcluir.Value}";
            }

            return await ClienteApi.ObtenerAutenticadoAsync<DisponibilidadAgenda>(ruta);

        }


        // PACIENTES ACTIVOS (para el selector del formulario)
        /// <summary>
        /// Listars the pacientes activos asynchronous.
        /// </summary>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public async Task<List<PacienteOpcion>> ListarPacientesActivosAsync(
            CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            var pacientes = await ClienteApi
                .ObtenerAutenticadoAsync<List<PacienteOpcion>>("api/pacientes");

            return pacientes
                .Where(p => p.Activo)
                .OrderBy(p => p.Apellidos)
                .ThenBy(p => p.Nombres)
                .ToList();
        }


        // OBTENER UNA CITA POR ID
        /// <summary>
        /// Obteners the por identifier asynchronous.
        /// </summary>
        /// <param name="idEvento">The identifier evento.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">El identificador de la cita no es válido.</exception>
        public async Task<EventoAgenda> ObtenerPorIdAsync(
            int idEvento,
            CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            if (idEvento <= 0)
                throw new ArgumentException("El identificador de la cita no es válido.");

            return await ClienteApi.ObtenerAutenticadoAsync<EventoAgenda>(
                $"api/agenda/{idEvento}");
        }

        // REGISTRAR CITA
        // Si el horario está ocupado, lanza HttpRequestException con
        // StatusCode = Conflict (409) y el mensaje "Esta hora no esta disponible."
        /// <summary>
        /// Registrars the asynchronous.
        /// </summary>
        /// <param name="idPaciente">The identifier paciente.</param>
        /// <param name="titulo">The titulo.</param>
        /// <param name="inicio">The inicio.</param>
        /// <param name="fin">The fin.</param>
        /// <param name="color">The color.</param>
        /// <param name="direccion">The direccion.</param>
        /// <param name="notas">The notas.</param>
        /// <param name="todoElDia">if set to <c>true</c> [todo el dia].</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        /// <exception cref="HttpRequestException">
        /// null
        /// or
        /// La API no devolvió el identificador de la cita.
        /// </exception>
        public async Task<int> RegistrarAsync(
            int idPaciente,
            string titulo,
            DateTime inicio,
            DateTime fin,
            string? color = null,
            string? direccion = null,
            string? notas = null,
            bool todoElDia = false,
            CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            var solicitud = new
            {
                IdPaciente = idPaciente,
                Titulo = titulo,
                FechaInicio = DateTime.SpecifyKind(inicio, DateTimeKind.Unspecified),
                FechaFinal = DateTime.SpecifyKind(fin, DateTimeKind.Unspecified),
                Color = color,
                Direccion = direccion,
                Notas = notas,
                TodoElDia = todoElDia
            };

            using var respuesta = await ClienteApi.PostAutenticadoAsync(
                "api/agenda/citas",
                solicitud,
                ct);

            if (!respuesta.IsSuccessStatusCode)
            {
                string mensaje = await ObtenerMensajeErrorAsync(
                    respuesta,
                    "No fue posible registrar la cita.",
                    ct);

                throw new HttpRequestException(mensaje, null, respuesta.StatusCode);
            }

            var resultado = await respuesta.Content
                .ReadFromJsonAsync<RespuestaRegistro>(cancellationToken: ct);

            return resultado?.IdEvento
                ?? throw new HttpRequestException(
                    "La API no devolvió el identificador de la cita.");
        }

        // MODIFICAR CITA
        /// <summary>
        /// Modificars the asynchronous.
        /// </summary>
        /// <param name="idEvento">The identifier evento.</param>
        /// <param name="idPaciente">The identifier paciente.</param>
        /// <param name="titulo">The titulo.</param>
        /// <param name="inicio">The inicio.</param>
        /// <param name="fin">The fin.</param>
        /// <param name="color">The color.</param>
        /// <param name="direccion">The direccion.</param>
        /// <param name="notas">The notas.</param>
        /// <param name="todoElDia">if set to <c>true</c> [todo el dia].</param>
        /// <param name="ct">The ct.</param>
        /// <exception cref="ArgumentException">El identificador de la cita no es válido.</exception>
        /// <exception cref="HttpRequestException">null</exception>
        public async Task ModificarAsync(
            int idEvento,
            int idPaciente,
            string titulo,
            DateTime inicio,
            DateTime fin,
            string? color = null,
            string? direccion = null,
            string? notas = null,
            bool todoElDia = false,
            CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            if (idEvento <= 0)
                throw new ArgumentException("El identificador de la cita no es válido.");

            var solicitud = new
            {
                IdPaciente = idPaciente,
                Titulo = titulo,
                FechaInicio = DateTime.SpecifyKind(inicio, DateTimeKind.Unspecified),
                FechaFinal = DateTime.SpecifyKind(fin, DateTimeKind.Unspecified),
                Color = color,
                Direccion = direccion,
                Notas = notas,
                TodoElDia = todoElDia
            };

            using var respuesta = await ClienteApi.PutAutenticadoAsync(
                $"api/agenda/citas/{idEvento}",
                solicitud,
                ct);

            if (!respuesta.IsSuccessStatusCode)
            {
                string mensaje = await ObtenerMensajeErrorAsync(
                    respuesta,
                    "No fue posible modificar la cita.",
                    ct);

                throw new HttpRequestException(
                    mensaje,
                    null,
                    respuesta.StatusCode);
            }
        }

        // CANCELAR CITA
        /// <summary>
        /// Cancelars the asynchronous.
        /// </summary>
        /// <param name="idEvento">The identifier evento.</param>
        /// <param name="ct">The ct.</param>
        /// <exception cref="ArgumentException">El identificador de la cita no es válido.</exception>
        /// <exception cref="HttpRequestException">null</exception>
        public async Task CancelarAsync(
            int idEvento,
            CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            if (idEvento <= 0)
                throw new ArgumentException("El identificador de la cita no es válido.");

            using var respuesta = await ClienteApi.PostAutenticadoAsync(
                $"api/agenda/citas/{idEvento}/cancelar",
                new { },
                ct);

            if (!respuesta.IsSuccessStatusCode)
            {
                string mensaje = await ObtenerMensajeErrorAsync(
                    respuesta,
                    "No fue posible cancelar la cita.",
                    ct);

                throw new HttpRequestException(
                    mensaje,
                    null,
                    respuesta.StatusCode);
            }
        }

        // LEER EL MENSAJE DE ERROR DE LA API
        /// <summary>
        /// Obteners the mensaje error asynchronous.
        /// </summary>
        /// <param name="respuesta">The respuesta.</param>
        /// <param name="mensajePredeterminado">The mensaje predeterminado.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        private static async Task<string> ObtenerMensajeErrorAsync(
            HttpResponseMessage respuesta,
            string mensajePredeterminado,
            CancellationToken ct)
        {
            if (respuesta.StatusCode == HttpStatusCode.Forbidden)
                return "No tienes permiso para realizar esta acción.";

            string contenido = await respuesta.Content.ReadAsStringAsync(ct);

            try
            {
                using var documento = JsonDocument.Parse(contenido);
                var raiz = documento.RootElement;

                // Errores de reglas de negocio y conflictos (400 / 409).
                if (raiz.TryGetProperty("detail", out var detalle)
                    && detalle.ValueKind == JsonValueKind.String
                    && !string.IsNullOrWhiteSpace(detalle.GetString()))
                {
                    return detalle.GetString()!;
                }

                // Errores de validación de campos (400): se toma el primero.
                if (raiz.TryGetProperty("errors", out var errores)
                    && errores.ValueKind == JsonValueKind.Object)
                {
                    foreach (var campo in errores.EnumerateObject())
                    {
                        foreach (var mensaje in campo.Value.EnumerateArray())
                        {
                            string? texto = mensaje.GetString();

                            if (!string.IsNullOrWhiteSpace(texto))
                                return texto;
                        }
                    }
                }
            }
            catch (JsonException)
            {
                // La respuesta no tiene formato JSON.
            }

            return mensajePredeterminado;
        }

        /// <summary>
        /// 
        /// </summary>
        private sealed class RespuestaRegistro
        {
            /// <summary>
            /// Gets or sets the identifier evento.
            /// </summary>
            /// <value>
            /// The identifier evento.
            /// </value>
            public int IdEvento { get; set; }
        }
    }
}
