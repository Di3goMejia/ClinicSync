using ClinicSyncDesktop.Data;
using ClinicSyncDesktop.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace ClinicSyncDesktop.Services;

/// <summary>
/// 
/// </summary>
public sealed class RolesApiService
{
    /// <summary>
    /// Listars the asynchronous.
    /// </summary>
    /// <param name="nombre">The nombre.</param>
    /// <param name="activo">The activo.</param>
    /// <param name="ct">The ct.</param>
    /// <returns></returns>
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

    /// <summary>
    /// Obteners the asynchronous.
    /// </summary>
    /// <param name="idRol">The identifier rol.</param>
    /// <param name="ct">The ct.</param>
    /// <returns></returns>
    public async Task<Rol> ObtenerAsync(int idRol, CancellationToken ct = default)
    {
        return await ClienteApi.ObtenerAutenticadoAsync<Rol>($"api/roles/{idRol}");
    }

    /// <summary>
    /// Crears the asynchronous.
    /// </summary>
    /// <param name="nombre">The nombre.</param>
    /// <param name="descripcion">The descripcion.</param>
    /// <param name="permisos">The permisos.</param>
    /// <param name="ct">The ct.</param>
    /// <returns></returns>
    /// <exception cref="HttpRequestException">La API no devolvió el rol creado.</exception>
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

    /// <summary>
    /// Modificars the asynchronous.
    /// </summary>
    /// <param name="idRol">The identifier rol.</param>
    /// <param name="nombre">The nombre.</param>
    /// <param name="descripcion">The descripcion.</param>
    /// <param name="permisos">The permisos.</param>
    /// <param name="ct">The ct.</param>
    public async Task ModificarAsync(int idRol, string nombre, string? descripcion, IEnumerable<string>? permisos = null, CancellationToken ct = default)
    {
        using var respuesta = await ClienteApi.PutAutenticadoAsync(
            $"api/roles/{idRol}",
            new { nombre, descripcion, permisos },
            ct);
        await VerificarRespuestaAsync(respuesta, "No fue posible modificar el rol.", ct);
    }

    /// <summary>
    /// Cambiars the estado asynchronous.
    /// </summary>
    /// <param name="idRol">The identifier rol.</param>
    /// <param name="activo">if set to <c>true</c> [activo].</param>
    /// <param name="ct">The ct.</param>
    public async Task CambiarEstadoAsync(int idRol, bool activo, CancellationToken ct = default)
    {
        using var respuesta = await ClienteApi.PatchAutenticadoAsync(
            $"api/roles/{idRol}/estado",
            new { activo },
            ct);
        await VerificarRespuestaAsync(respuesta, "No fue posible cambiar el estado del rol.", ct);
    }

    /// <summary>
    /// Verificars the respuesta asynchronous.
    /// </summary>
    /// <param name="respuesta">The respuesta.</param>
    /// <param name="mensaje">The mensaje.</param>
    /// <param name="ct">The ct.</param>
    /// <exception cref="HttpRequestException">null</exception>
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

/// <summary>
/// 
/// </summary>
public sealed class RolCreado
{
    /// <summary>
    /// Gets or sets the identifier rol.
    /// </summary>
    /// <value>
    /// The identifier rol.
    /// </value>
    public int IdRol { get; set; }
    /// <summary>
    /// Gets or sets the codigo.
    /// </summary>
    /// <value>
    /// The codigo.
    /// </value>
    public string Codigo { get; set; } = string.Empty;
}
