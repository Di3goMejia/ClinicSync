using ClinicSyncDesktop.Data;
using ClinicSyncDesktop.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace ClinicSyncDesktop.Services;

public sealed class RolesApiService
{
    public async Task<List<Rol>> ListarAsync(string? nombre = null, bool? activo = null, CancellationToken ct = default)
    {
        var parametros = new List<string>();
        if (!string.IsNullOrWhiteSpace(nombre))
            parametros.Add($"Nombre={Uri.EscapeDataString(nombre.Trim())}");
        if (activo.HasValue)
            parametros.Add($"Activo={activo.Value.ToString().ToLowerInvariant()}");

        var ruta = "api/roles" + (parametros.Count > 0 ? "?" + string.Join("&", parametros) : string.Empty);
        return await ClienteApi.ObtenerAutenticadoAsync<List<Rol>>(ruta);
    }

    public async Task<Rol> ObtenerAsync(int idRol, CancellationToken ct = default)
    {
        return await ClienteApi.ObtenerAutenticadoAsync<Rol>($"api/roles/{idRol}");
    }

    public async Task<RolCreado> CrearAsync(string nombre, string? descripcion, IEnumerable<string>? permisos = null, CancellationToken ct = default)
    {
        using var respuesta = await ClienteApi.PostAutenticadoAsync(
            "api/roles",
            new { nombre, descripcion, permisos },
            ct);
        await VerificarRespuestaAsync(respuesta, "No fue posible crear el rol.", ct);
        return await respuesta.Content.ReadFromJsonAsync<RolCreado>(cancellationToken: ct)
            ?? throw new HttpRequestException("La API no devolvió el rol creado.");
    }

    public async Task ModificarAsync(int idRol, string nombre, string? descripcion, IEnumerable<string>? permisos = null, CancellationToken ct = default)
    {
        using var respuesta = await ClienteApi.PutAutenticadoAsync(
            $"api/roles/{idRol}",
            new { nombre, descripcion, permisos },
            ct);
        await VerificarRespuestaAsync(respuesta, "No fue posible modificar el rol.", ct);
    }

    public async Task CambiarEstadoAsync(int idRol, bool activo, CancellationToken ct = default)
    {
        using var respuesta = await ClienteApi.PatchAutenticadoAsync(
            $"api/roles/{idRol}/estado",
            new { activo },
            ct);
        await VerificarRespuestaAsync(respuesta, "No fue posible cambiar el estado del rol.", ct);
    }

    private static async Task VerificarRespuestaAsync(HttpResponseMessage respuesta, string mensaje, CancellationToken ct)
    {
        if (respuesta.IsSuccessStatusCode)
            return;

        var contenido = await respuesta.Content.ReadAsStringAsync(ct);
        try
        {
            using var documento = JsonDocument.Parse(contenido);
            if (documento.RootElement.TryGetProperty("detail", out var detalle))
                mensaje = detalle.GetString() ?? mensaje;
        }
        catch (JsonException)
        {
        }

        throw new HttpRequestException(mensaje, null, respuesta.StatusCode);
    }
}

public sealed class RolCreado
{
    public int IdRol { get; set; }
    public string Codigo { get; set; } = string.Empty;
}
