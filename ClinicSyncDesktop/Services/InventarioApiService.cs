
using System;
using System.IO;
using ClinicSyncDesktop.Data;
using ClinicSyncDesktop.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicSyncDesktop.Services
{
    public class InventarioApiService
    {
        // LISTAR MATERIALES
        public async Task<List<Material>> ListarAsync(
            string? buscar = null,
            bool? activo = null,
            CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            var parametros = new List<string>();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                parametros.Add(
                    $"Buscar={Uri.EscapeDataString(buscar.Trim())}");
            }

            if (activo.HasValue)
            {
                parametros.Add(
                    $"Activo={activo.Value.ToString().ToLowerInvariant()}");
            }

            string ruta = "api/inventario/materiales/";

            if (parametros.Count > 0)
            {
                ruta += "?" + string.Join("&", parametros);
            }

            return await ClienteApi.ObtenerAutenticadoAsync<List<Material>>(
                ruta);
        }

        // OBTENER MATERIAL POR ID
        public async Task<Material> ObtenerPorIdAsync(
            int idMaterial,
            CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            return await ClienteApi.ObtenerAutenticadoAsync<Material>(
                $"api/inventario/materiales/{idMaterial}");
        }

        // REGISTRAR MATERIAL
        public async Task<int> RegistrarAsync(
            string codigo,
            string nombre,
            string unidadMedida,
            decimal stockMinimo,
            decimal cantidadInicial,
            decimal precioCompra,
            string? imagen = null,
            string? descripcion = null,
            CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            string? referenciaImagen = await ResolverImagenAsync(
                imagen,
                ct);

            ct.ThrowIfCancellationRequested();

            var solicitud = new
            {
                Codigo = codigo,
                Nombre = nombre,
                UnidadMedida = unidadMedida,
                StockMinimo = stockMinimo,
                CantidadInicial = cantidadInicial,
                PrecioCompra = precioCompra,
                Imagen = referenciaImagen,
                Descripcion = descripcion
            };

            using var respuesta =
                await ClienteApi.PostAutenticadoAsync(
                    "api/inventario/materiales/",
                    solicitud,
                    ct);

            if (!respuesta.IsSuccessStatusCode)
            {
                string mensaje = await ObtenerMensajeErrorAsync(
                    respuesta,
                    "No fue posible registrar el material.",
                    ct);

                throw new HttpRequestException(mensaje);
            }

            var resultado =
                await respuesta.Content.ReadFromJsonAsync<RespuestaRegistro>(
                    cancellationToken: ct);

            return resultado?.IdMaterial
                ?? throw new HttpRequestException(
                    "La API no devolvió el identificador del material.");
        }

        // MODIFICAR MATERIAL, INCLUYENDO EL STOCK
        public async Task<Material> ModificarAsync(
            int idMaterial,
            string codigo,
            string nombre,
            string unidadMedida,
            decimal stockMinimo,
            decimal precioCompra,
            string? imagen = null,
            string? descripcion = null,
            decimal? stockActual = null,
            decimal? stockEsperado = null,
            string? motivo = null,
            CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            string? referenciaImagen = await ResolverImagenAsync(
                imagen,
                ct);

            ct.ThrowIfCancellationRequested();

            var solicitud = new
            {
                Codigo = codigo,
                Nombre = nombre,
                UnidadMedida = unidadMedida,
                StockMinimo = stockMinimo,
                PrecioCompra = precioCompra,
                Imagen = referenciaImagen,
                Descripcion = descripcion,

                // Si son null, el procedimiento no modifica el stock.
                StockActual = stockActual,
                StockEsperado = stockEsperado,
                Motivo = motivo
            };

            using var respuesta =
                await ClienteApi.PutAutenticadoAsync(
                    $"api/inventario/materiales/{idMaterial}",
                    solicitud,
                    ct);

            if (!respuesta.IsSuccessStatusCode)
            {
                string mensaje = await ObtenerMensajeErrorAsync(
                    respuesta,
                    "No fue posible modificar el material.",
                    ct);

                throw new HttpRequestException(mensaje);
            }

            var resultado =
                await respuesta.Content.ReadFromJsonAsync<Material>(
                    cancellationToken: ct);

            return resultado
                ?? throw new HttpRequestException(
                    "La API no devolvió el material actualizado.");
        }

        // RESOLVER LA IMAGEN DEL MATERIAL
        private static async Task<string?> ResolverImagenAsync(
            string? imagen,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(imagen))
            {
                return null;
            }

            imagen = imagen.Trim();

            const string prefijoImagen =
                "/api/inventario/materiales/imagenes/";

            if (imagen.StartsWith(
                prefijoImagen,
                StringComparison.OrdinalIgnoreCase))
            {
                return imagen;
            }

            if (Uri.TryCreate(
                    imagen,
                    UriKind.Absolute,
                    out Uri? uri)
                && (uri.Scheme == Uri.UriSchemeHttp
                    || uri.Scheme == Uri.UriSchemeHttps))
            {
                return imagen;
            }

            if (!File.Exists(imagen))
            {
                throw new FileNotFoundException(
                    "No se encontró la imagen seleccionada. " +
                    "Selecciona nuevamente el archivo.",
                    imagen);
            }

            ct.ThrowIfCancellationRequested();

            string referencia = await ClienteApi.SubirImagenMaterialAsync(
                imagen,
                ct);

            if (string.IsNullOrWhiteSpace(referencia))
            {
                throw new HttpRequestException(
                    "La API no devolvió una referencia válida para la imagen.");
            }

            return referencia;
        }

        // CAMBIAR ESTADO DEL MATERIAL
        public async Task CambiarEstadoAsync(
            int idMaterial,
            bool activo,
            CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            using var respuesta =
                await ClienteApi.PatchAutenticadoAsync(
                    $"api/inventario/materiales/{idMaterial}/estado",
                    new { Activo = activo },
                    ct);

            if (!respuesta.IsSuccessStatusCode)
            {
                string mensaje = await ObtenerMensajeErrorAsync(
                    respuesta,
                    "No fue posible cambiar el estado del material.",
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
            string contenido =
                await respuesta.Content.ReadAsStringAsync(ct);

            string codigo =
                $"{(int)respuesta.StatusCode} ({respuesta.StatusCode})";

            try
            {
                using var documento = JsonDocument.Parse(contenido);

                if (documento.RootElement.TryGetProperty(
                    "detail", out var detalle))
                {
                    return $"HTTP {codigo}: " +
                        (detalle.GetString() ?? mensajePredeterminado);
                }

                if (documento.RootElement.TryGetProperty(
                    "title", out var titulo))
                {
                    return $"HTTP {codigo}: " +
                        (titulo.GetString() ?? mensajePredeterminado);
                }
            }
            catch (JsonException)
            {
                // La respuesta no tiene formato JSON.
            }

            if (!string.IsNullOrWhiteSpace(contenido))
            {
                return $"HTTP {codigo}. Respuesta del servidor: {contenido}";
            }

            return $"HTTP {codigo}: {mensajePredeterminado}";
        }

        // RESPUESTA DEL REGISTRO
        private sealed class RespuestaRegistro
        {
            public int IdMaterial { get; set; }
        }
    }
}
