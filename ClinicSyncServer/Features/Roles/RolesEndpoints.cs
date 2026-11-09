using ClinicSyncServer.Features.Roles.Requests;
using Microsoft.AspNetCore.Mvc;
using PermisosSeguridad = ClinicSyncServer.Common.Seguridad.Permisos;

namespace ClinicSyncServer.Features.Roles;

public static class RolesEndpoints
{
    public static void MapRolesEndpoints(this WebApplication app)
    {
        var roles = app.MapGroup("/api/roles").RequireAuthorization();

        roles.MapGet("", async (
            [AsParameters] ListarRolesQuery query,
            IRolesService service,
            CancellationToken ct) =>
        {
            var listado = await service.ListarAsync(query, ct);
            return Results.Ok(listado);
        })
        .WithName("ListarRoles")
        .RequireAuthorization(PermisosSeguridad.RolesLeer);

        roles.MapPost("", async (
            [FromBody] CrearRolRequest request,
            IRolesService service,
            CancellationToken ct) =>
        {
            var respuesta = await service.CrearAsync(request, ct);
            return Results.Created($"/api/roles/{respuesta.IdRol}", respuesta);
        })
        .WithName("CrearRol")
        .RequireAuthorization(PermisosSeguridad.RolesCrear);

        roles.MapGet("/{id:int}", async (
            int id,
            IRolesService service,
            CancellationToken ct) =>
        {
            var rol = await service.ObtenerPorIdAsync(id, ct);
            return rol is null ? Results.NotFound() : Results.Ok(rol);
        })
        .WithName("ObtenerRol")
        .RequireAuthorization(PermisosSeguridad.RolesLeer);

        roles.MapPut("/{id:int}", async (
            int id,
            [FromBody] ModificarRolRequest request,
            IRolesService service,
            CancellationToken ct) =>
        {
            await service.ModificarAsync(id, request, ct);
            return Results.NoContent();
        })
        .WithName("ModificarRol")
        .RequireAuthorization(PermisosSeguridad.RolesModificar);

        roles.MapPatch("/{id:int}/estado", async (
            int id,
            [FromBody] CambiarEstadoRolRequest request,
            IRolesService service,
            CancellationToken ct) =>
        {
            await service.CambiarEstadoAsync(id, request, ct);
            return Results.NoContent();
        })
        .WithName("CambiarEstadoRol")
        .RequireAuthorization(PermisosSeguridad.RolesEstado);
    }
}
