using System.Security.Claims;
using ClinicSyncServer.Features.Agenda.Requests;
using Microsoft.AspNetCore.Mvc;
using PermisosSeguridad = ClinicSyncServer.Common.Seguridad.Permisos;

namespace ClinicSyncServer.Features.Agenda
{
    public static class AgendaEndpoints
    {
        public static void MapAgendaEndpoints(this WebApplication app)
        {
            var agenda = app
                .MapGroup("/api/agenda")
                .RequireAuthorization();

            // CALENDARIO (vistas Día / Semana / Mes)
            agenda.MapGet(
                "/",
                async (
                    [AsParameters] ListarAgendaQuery query,
                    IAgendaService svc,
                    CancellationToken ct) =>
                {
                    return Results.Ok(
                        await svc.ListarAsync(query, ct));
                })
                .WithName("ListarAgenda")
                .RequireAuthorization(
                    PermisosSeguridad.AgendaLeer);

            // PRÓXIMAS CITAS ("Ver más..." = cantidad mayor)
            agenda.MapGet(
                "/proximas",
                async (
                    [AsParameters] ProximasCitasQuery query,
                    IAgendaService svc,
                    CancellationToken ct) =>
                {
                    return Results.Ok(
                        await svc.ProximasAsync(query, ct));
                })
                .WithName("ProximasCitas")
                .RequireAuthorization(
                    PermisosSeguridad.AgendaLeer);

            // CONSULTAR DISPONIBILIDAD DE UN HORARIO
            agenda.MapGet(
                "/disponibilidad",
                async (
                    [AsParameters] DisponibilidadQuery query,
                    ClaimsPrincipal usuario,
                    IAgendaService svc,
                    CancellationToken ct) =>
                {
                    if (!int.TryParse(
                        usuario.FindFirstValue("sub"),
                        out int idUsuario))
                    {
                        return Results.Unauthorized();
                    }

                    return Results.Ok(
                        await svc.VerificarDisponibilidadAsync(
                            query,
                            idUsuario,
                            ct));
                })
                .WithName("VerificarDisponibilidad")
                .RequireAuthorization(
                    PermisosSeguridad.AgendaLeer);

            // OBTENER UN EVENTO POR ID (pantalla "Editar cita")
            agenda.MapGet(
                "/{id:int}",
                async (
                    int id,
                    IAgendaService svc,
                    CancellationToken ct) =>
                {
                    if (id <= 0)
                        return Results.BadRequest();

                    var evento =
                        await svc.ObtenerPorIdAsync(id, ct);

                    return evento is null
                        ? Results.NotFound()
                        : Results.Ok(evento);
                })
                .WithName("ObtenerEventoAgenda")
                .RequireAuthorization(
                    PermisosSeguridad.AgendaLeer);

            // REGISTRAR CITA
            agenda.MapPost(
                "/citas",
                async (
                    [FromBody] CitaRequest req,
                    ClaimsPrincipal usuario,
                    IAgendaService svc,
                    CancellationToken ct) =>
                {
                    if (!int.TryParse(
                        usuario.FindFirstValue("sub"),
                        out int idUsuario))
                    {
                        return Results.Unauthorized();
                    }

                    var id = await svc.InsertarAsync(
                        req,
                        idUsuario,
                        ct);

                    return Results.Created(
                        $"/api/agenda/{id}",
                        new { IdEvento = id });
                })
                .WithName("CrearCita")
                .RequireAuthorization(
                    PermisosSeguridad.AgendaCrear);

           
        }
    }
}