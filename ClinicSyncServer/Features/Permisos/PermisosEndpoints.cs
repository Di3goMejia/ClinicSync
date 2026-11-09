using PermisosSeguridad = ClinicSyncServer.Common.Seguridad.Permisos;

namespace ClinicSyncServer.Features.Permisos;

public static class PermisosEndpoints
{
    public static void MapPermisosEndpoints(this WebApplication app)
    {
        app.MapGet("/api/permisos", async (IPermisosService service, CancellationToken ct) =>
                Results.Ok(await service.ListarAsync(ct)))
            .RequireAuthorization(PermisosSeguridad.RolesLeer)
            .WithName("ListarPermisos");
    }
}
