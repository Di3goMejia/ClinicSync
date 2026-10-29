using System.Security.Claims;
using ClinicSyncServer.Features.Egresos.Requests;
using ClinicSyncServer.Features.Egresos.Responses;
using Microsoft.AspNetCore.Mvc;
using PermisosSeguridad = ClinicSyncServer.Common.Seguridad.Permisos;

namespace ClinicSyncServer.Features.Egresos
{
    public static class EgresosEndpoints
    {
        public static void MapEgresosEndpoints(this WebApplication app)
        {
            var egresos = app
                .MapGroup("/api/finanzas/egresos")
                .RequireAuthorization();

            // LISTAR EGRESOS POR RANGO DE FECHAS
            egresos.MapGet(
                "/",
                async (
                    [FromQuery] DateTime fechaDesde,
                    [FromQuery] DateTime fechaHasta,
                    [FromQuery] bool? activo,
                    IEgresosService svc,
                    CancellationToken ct) =>
                {
                    return Results.Ok(
                        await svc.ListarAsync(
                            fechaDesde, fechaHasta, activo, ct));
                })
                .WithName("ListarEgresos")
                .RequireAuthorization(PermisosSeguridad.EgresosLeer);

            // REGISTRAR EGRESO
            egresos.MapPost(
                "/",
                async (
                    [FromBody] RegistrarEgresoRequest req,
                    ClaimsPrincipal usuario,
                    IEgresosService svc,
                    CancellationToken ct) =>
                {
                    if (!int.TryParse(
                        usuario.FindFirstValue("sub"),
                        out int idUsuario))
                    {
                        return Results.Unauthorized();
                    }

                    var id = await svc.InsertarAsync(req, idUsuario, ct);

                    return Results.Created(
                        $"/api/finanzas/egresos/{id}",
                        new RegistrarEgresoResponse(id));
                })
                .WithName("CrearEgreso")
                .RequireAuthorization(PermisosSeguridad.EgresosCrear);
        }
    }
}