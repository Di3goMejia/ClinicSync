using PermisosSeguridad = ClinicSyncServer.Common.Seguridad.Permisos;

namespace ClinicSyncServer.Features.Permisos;

/// <summary>
/// 
/// </summary>
public static class PermisosEndpoints
{
    /// <summary>
    /// Maps the permisos endpoints.
    /// </summary>
    /// <param name="app">The application.</param>
    public static void MapPermisosEndpoints(this WebApplication app)
    {
        app.MapGet("/api/permisos", async (IPermisosService service, CancellationToken ct) =>
                Results.Ok(await service.ListarAsync(ct)))
            .RequireAuthorization(PermisosSeguridad.RolesLeer)
            .WithName("ListarPermisos");
    }
}
