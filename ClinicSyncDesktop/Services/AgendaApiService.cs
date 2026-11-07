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
    public class AgendaApiService
    {
        // Las fechas viajan SIN zona horaria (ej. 2026-11-10T09:00:00).
        private static string Fecha(DateTime fecha) =>
            fecha.ToString("yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture);

        // CALENDARIO (vistas Día / Semana / Mes)
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

        private sealed class RespuestaRegistro
        {
            public int IdEvento { get; set; }
        }
    }
}
