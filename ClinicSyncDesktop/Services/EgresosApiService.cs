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
using System.Linq;
using ClinicSyncDesktop.Models;

namespace ClinicSyncDesktop.Services
{
    public class EgresosApiService
    {
        // LISTAR EGRESOS POR RANGO DE FECHAS
        public async Task<List<Egreso>> ListarAsync(
            DateTime desde,
            DateTime hasta,
            bool? activo = null,
            CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            string ruta =
                "api/finanzas/egresos/" +
                $"?fechaDesde={desde.Date.ToString("yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture)}" +
                $"&fechaHasta={hasta.Date.AddDays(1).AddSeconds(-1).ToString("yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture)}";

            if (activo.HasValue)
            {
                ruta += $"&activo={activo.Value.ToString().ToLowerInvariant()}";
            }

            return await ClienteApi.ObtenerAutenticadoAsync<List<Egreso>>(
                ruta);
        }

        // REGISTRAR EGRESO
        public async Task<int> RegistrarAsync(
            string tipo,
            DateTime fecha,
            string concepto,
            string medioPago,
            decimal monto,
            IEnumerable<(int IdMaterial, decimal Cantidad)>? materiales = null,
            CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            var solicitud = new
            {
                Tipo = tipo,
                Fecha = fecha.ToString(
                    "O",
                    CultureInfo.InvariantCulture),
                Concepto = concepto,
                MedioPago = medioPago,
                Monto = monto,
                Materiales = materiales?
                    .Select(m => new { m.IdMaterial, m.Cantidad })
                    .ToList()
            };

            using var respuesta =
                await ClienteApi.PostAutenticadoAsync(
                    "api/finanzas/egresos/",
                    solicitud,
                    ct);

            if (!respuesta.IsSuccessStatusCode)
            {
                string mensaje = await ObtenerMensajeErrorAsync(
                    respuesta,
                    "No fue posible registrar el egreso.",
                    ct);

                throw new HttpRequestException(mensaje);
            }

            var resultado =
                await respuesta.Content.ReadFromJsonAsync<RespuestaRegistro>(
                    cancellationToken: ct);

            return resultado?.IdEgreso
                ?? throw new HttpRequestException(
                    "La API no devolvió el identificador del egreso.");
        }

        public async Task<Egreso> ObtenerAsync(
            int idEgreso,
            CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            return await ClienteApi.ObtenerAutenticadoAsync<Egreso>(
                $"api/finanzas/egresos/{idEgreso}");
        }

        public async Task ActualizarAsync(
            int idEgreso,
            DateTime fecha,
            string concepto,
            string medioPago,
            decimal monto,
            bool? activo = null,
            CancellationToken ct = default)
        {
            var solicitud = new
            {
                Fecha = fecha.ToString("O", CultureInfo.InvariantCulture),
                Concepto = concepto,
                MedioPago = medioPago,
                Monto = monto,
                Activo = activo
            };

            using var respuesta = await ClienteApi.PutAutenticadoAsync(
                $"api/finanzas/egresos/{idEgreso}", solicitud, ct);

            if (!respuesta.IsSuccessStatusCode)
            {
                string mensaje = await ObtenerMensajeErrorAsync(
                    respuesta,
                    "No fue posible actualizar el egreso.",
                    ct);

                throw new HttpRequestException(mensaje);
            }
        }


        // LEER MENSAJE DE ERROR DE LA API
        private static async Task<string> ObtenerMensajeErrorAsync(
            HttpResponseMessage respuesta,
            string mensajePredeterminado,
            CancellationToken ct)
        {
            if (respuesta.StatusCode == HttpStatusCode.Forbidden)
            {
                return "No tienes permiso para realizar esta acción.";
            }

            string contenido =
                await respuesta.Content.ReadAsStringAsync(ct);

            try
            {
                using var documento = JsonDocument.Parse(contenido);

                if (documento.RootElement.TryGetProperty(
                    "detail", out var detalle))
                {
                    string? texto = detalle.GetString();

                    if (!string.IsNullOrWhiteSpace(texto))
                    {
                        return texto;
                    }
                }
            }
            catch (JsonException)
            {
                // La respuesta no tiene formato JSON.
            }

            return mensajePredeterminado;
        }

        // RESPUESTA DEL REGISTRO
        private sealed class RespuestaRegistro
        {
            public int IdEgreso { get; set; }
        }
    }
}
