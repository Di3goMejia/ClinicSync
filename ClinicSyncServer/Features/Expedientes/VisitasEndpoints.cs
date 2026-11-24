using ClinicSyncServer.Features.Expedientes.Data;
using ClinicSyncServer.Features.Expedientes.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

using PermisosSeguridad = ClinicSyncServer.Common.Seguridad.Permisos;

namespace ClinicSyncServer.Features.Expedientes
{
    /// <summary>
    /// 
    /// </summary>
    public static class VisitasEndpoints
    {
        public static void MapVisitasEndpoints(
            this WebApplication app)
        {
            var grupo = app.MapGroup("/api/visitas")
                .RequireAuthorization();

            // OBTENER VISITA
            grupo.MapGet(
                "/{idVisita:int}",
                async (
                    int idVisita,
                    IVisitasService svc,
                    CancellationToken ct) =>
                {
                    var visita = await svc.ObtenerPorIdAsync(idVisita, ct);
                    return visita is null
                        ? Results.NotFound()
                        : Results.Ok(visita);
                }
            ).RequireAuthorization(PermisosSeguridad.ExpedientesLeer);

            // LISTAR TRATAMIENTOS DE UNA VISITA
            grupo.MapGet(
                "/{idVisita:int}/tratamientos",
                async (
                    int idVisita,
                    IVisitasService svc,
                    CancellationToken ct) =>
                {
                    var visita = await svc.ObtenerPorIdAsync(idVisita, ct);
                    if (visita is null)
                        return Results.NotFound();

                    var tratamientos = await svc.ListarTratamientosAsync(idVisita, ct);
                    if (tratamientos is null)
                        return Results.NotFound();

                    return Results.Ok(tratamientos);
                }
            ).RequireAuthorization(PermisosSeguridad.ExpedientesLeer);

            // CREAR VISITA
            grupo.MapPost(
                "/",
                async (
                    [FromBody] CrearVisitaRequest request,
                    IVisitasService svc,
                    CancellationToken ct) =>
                {
                    request.Motivo = request.Motivo.Trim();

                    var idVisita = await svc.InsertarAsync(request, ct);
                    return Results.Created(
                        $"/api/visitas/{idVisita}",
                        new { IdVisita = idVisita }
                    );
                }
            ).RequireAuthorization(PermisosSeguridad.ExpedientesModificar);

            // MODIFICAR VISITA
            grupo.MapPut(
                "/{idVisita:int}",
                async (
                    int idVisita,
                    [FromBody] ModificarVisitaRequest request,
                    IVisitasService svc,
                    CancellationToken ct) =>
                {
                    request.Motivo = request.Motivo.Trim();

                    await svc.ModificarAsync(idVisita, request, ct);
                    return Results.NoContent();
                }
            ).RequireAuthorization(PermisosSeguridad.ExpedientesModificar);

            // CREAR VISITA COMPLETA CON TRATAMIENTOS
            grupo.MapPost(
                "/completa",
                async (
                    [FromBody] CrearVisitaCompletaRequest request,
                    IVisitasService svc,
                    CancellationToken ct) =>
                {
                    request.Motivo = request.Motivo.Trim();

                    int idVisita = await svc.InsertarCompletaAsync(request, ct);

                    return Results.Created(
                        $"/api/visitas/{idVisita}",
                        new { IdVisita = idVisita });                    
                }
            ).RequireAuthorization(PermisosSeguridad.ExpedientesModificar);


            // MODIFICAR VISITA COMPLETA CON TRATAMIENTOS
            grupo.MapPut(
                "/{idVisita:int}/completa",
                async (
                    int idVisita,
                    [FromBody] ModificarVisitaCompletaRequest request,
                    IVisitasService svc,
                    CancellationToken ct) =>
                {
                    request.Motivo = request.Motivo.Trim();

                    await svc.ModificarCompletaAsync(idVisita, request, ct);
                    return Results.NoContent();
                }
            ).RequireAuthorization(PermisosSeguridad.ExpedientesModificar);
        }
    }
}
