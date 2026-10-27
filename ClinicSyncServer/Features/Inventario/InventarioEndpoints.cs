using System.Security.Claims;
using ClinicSyncServer.Features.Inventario.Requests;
using ClinicSyncServer.Features.Inventario.Responses;
using Microsoft.AspNetCore.Mvc;
using PermisosSeguridad = ClinicSyncServer.Common.Seguridad.Permisos;

namespace ClinicSyncServer.Features.Inventario
{
    public static class InventarioEndpoints
    {
        public static void MapInventarioEndpoints(this WebApplication app)
        {
            var materiales = app.MapGroup("/api/inventario/materiales").RequireAuthorization();

            materiales.MapGet("/", async ([AsParameters] ListarMaterialesQuery query, IInventarioService svc, CancellationToken ct) =>
                Results.Ok(await svc.ListarAsync(query, ct)))
                .WithName("ListarMateriales")
                .RequireAuthorization(PermisosSeguridad.InventarioLeer);

            materiales.MapGet("/{id:int}", async (int id, IInventarioService svc, CancellationToken ct) =>
            {
                if (id <= 0) return Results.BadRequest();
                var material = await svc.ObtenerPorIdAsync(id, ct);
                return material is null ? Results.NotFound() : Results.Ok(material);
            })
                .WithName("ObtenerMaterial")
                .RequireAuthorization(PermisosSeguridad.InventarioLeer);

            materiales.MapPost("/", async ([FromBody] RegistrarMaterialRequest req, ClaimsPrincipal usuario, IInventarioService svc, CancellationToken ct) =>
            {
                if (!int.TryParse(usuario.FindFirstValue("sub"), out int idUsuario))
                    return Results.Unauthorized();
                var id = await svc.InsertarAsync(req, idUsuario, ct);
                return Results.Created($"/api/inventario/materiales/{id}", new RegistrarMaterialResponse(id));
            })
                .WithName("CrearMaterial")
                .RequireAuthorization(PermisosSeguridad.InventarioCrear);

            materiales.MapPut("/{id:int}", async (int id, [FromBody] MaterialRequest req, IInventarioService svc, CancellationToken ct) =>
            {
                if (id <= 0) return Results.BadRequest();
                var material = await svc.ModificarAsync(id, req, ct);
                return material is null ? Results.NotFound() : Results.Ok(material);
            })
                .WithName("ModificarMaterial")
                .RequireAuthorization(PermisosSeguridad.InventarioModificar);

            materiales.MapPatch("/{id:int}/estado", async (int id, [FromBody] CambiarEstadoMaterialRequest req, IInventarioService svc, CancellationToken ct) =>
            {
                if (id <= 0) return Results.BadRequest();
                var cambiado = await svc.CambiarEstadoAsync(id, req.Activo, ct);
                return cambiado ? Results.NoContent() : Results.NotFound();
            })
                .WithName("CambiarEstadoMaterial")
                .RequireAuthorization(PermisosSeguridad.InventarioEstado);
        }
    }
}
