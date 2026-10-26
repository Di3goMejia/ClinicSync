using System.Linq;
using ClinicSyncServer.Features.Expedientes.Data;
using ClinicSyncServer.Features.Expedientes.Requests;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using System;
using System.Threading;

namespace ClinicSyncServer.Features.Expedientes
{
    public static class ExpedientesEndpoints
    {
        public static void MapExpedientesEndpoints(
            this WebApplication app)
        {
            var grupo = app.MapGroup("/api/expedientes")
                .RequireAuthorization();

            // OBTENER EXPEDIENTE DEL PACIENTE
            grupo.MapGet(
                "/paciente/{idPaciente:int}",
                async (
                    int idPaciente,
                    ExpedientesRepository repository,
                    CancellationToken ct) =>
                {
                    if (idPaciente <= 0)
                        return Results.BadRequest(
                            new { mensaje = "Paciente inválido." });

                    try
                    {
                        var expediente =
                            await repository.ObtenerExpedienteAsync(
                                idPaciente, ct);

                        return expediente is null
                            ? Results.NotFound(
                                new { mensaje = "Expediente no encontrado." })
                            : Results.Ok(expediente);
                    }
                    catch (SqlException ex)
                        when (ex.Number == 50001)
                    {
                        return Results.NotFound(
                            new { mensaje = ex.Message });
                    }
                });

            // LISTAR VISITAS DEL PACIENTE
            grupo.MapGet(
                "/paciente/{idPaciente:int}/visitas",
                async (
                    int idPaciente,
                    ExpedientesRepository repository,
                    CancellationToken ct) =>
                {
                    if (idPaciente <= 0)
                        return Results.BadRequest(
                            new { mensaje = "Paciente inválido." });

                    try
                    {
                        var visitas =
                            await repository.ListarVisitasAsync(
                                idPaciente, ct);

                        return Results.Ok(visitas);
                    }
                    catch (SqlException ex)
                        when (ex.Number == 50001)
                    {
                        return Results.NotFound(
                            new { mensaje = ex.Message });
                    }
                });

            // OBTENER VISITA
            grupo.MapGet(
                "/visitas/{idVisita:int}",
                async (
                    int idVisita,
                    ExpedientesRepository repository,
                    CancellationToken ct) =>
                {
                    if (idVisita <= 0)
                        return Results.BadRequest(
                            new { mensaje = "Visita inválida." });

                    try
                    {
                        var visita =
                            await repository.ObtenerVisitaAsync(
                                idVisita, ct);

                        return visita is null
                            ? Results.NotFound(
                                new { mensaje = "Visita no encontrada." })
                            : Results.Ok(visita);
                    }
                    catch (SqlException ex)
                        when (ex.Number == 50001)
                    {
                        return Results.NotFound(
                            new { mensaje = ex.Message });
                    }
                });

            // LISTAR TRATAMIENTOS DE UNA VISITA
            grupo.MapGet(
                "/visitas/{idVisita:int}/tratamientos",
                async (
                    int idVisita,
                    ExpedientesRepository repository,
                    CancellationToken ct) =>
                {
                    if (idVisita <= 0)
                        return Results.BadRequest(
                            new { mensaje = "Visita inválida." });

                    try
                    {
                        var visita =
                            await repository.ObtenerVisitaAsync(
                                idVisita, ct);

                        if (visita is null)
                            return Results.NotFound(
                                new { mensaje = "Visita no encontrada." });

                        var tratamientos =
                            await repository.ListarTratamientosAsync(
                                idVisita, ct);

                        return Results.Ok(tratamientos);
                    }
                    catch (SqlException ex)
                        when (ex.Number == 50001)
                    {
                        return Results.NotFound(
                            new { mensaje = ex.Message });
                    }
                });

            // CREAR VISITA
            grupo.MapPost(
                "/visitas",
                async (
                    CrearVisitaRequest request,
                    ExpedientesRepository repository,
                    CancellationToken ct) =>
                {
                    if (request.IdPaciente <= 0 ||
                        request.IdEmpleado <= 0 ||
                        request.Fecha == default ||
                        string.IsNullOrWhiteSpace(request.Motivo) ||
                        request.Motivo.Trim().Length > 255 ||
                        request.Notas?.Length > 500)
                    {
                        return Results.BadRequest(
                            new { mensaje = "Datos de visita inválidos." });
                    }

                    request.Motivo = request.Motivo.Trim();

                    try
                    {
                        int idVisita =
                            await repository.CrearVisitaAsync(
                                request, ct);

                        return Results.Created(
                            $"/api/expedientes/visitas/{idVisita}",
                            new { IdVisita = idVisita });
                    }
                    catch (SqlException ex)
                        when (ex.Number >= 50000 &&
                              ex.Number < 51000)
                    {
                        return Results.BadRequest(
                            new { mensaje = ex.Message });
                    }
                });

            // MODIFICAR VISITA
            grupo.MapPut(
                "/visitas/{idVisita:int}",
                async (
                    int idVisita,
                    ModificarVisitaRequest request,
                    ExpedientesRepository repository,
                    CancellationToken ct) =>
                {
                    if (idVisita <= 0 ||
                        request.IdEmpleado <= 0 ||
                        request.Fecha == default ||
                        string.IsNullOrWhiteSpace(request.Motivo) ||
                        request.Motivo.Trim().Length > 255 ||
                        request.Notas?.Length > 500)
                    {
                        return Results.BadRequest(
                            new { mensaje = "Datos de visita inválidos." });
                    }

                    request.Motivo = request.Motivo.Trim();

                    try
                    {
                        await repository.ModificarVisitaAsync(
                            idVisita, request, ct);

                        return Results.NoContent();
                    }
                    catch (SqlException ex)
                        when (ex.Number >= 50000 &&
                              ex.Number < 51000)
                    {
                        return Results.BadRequest(
                            new { mensaje = ex.Message });
                    }
                });

            // CREAR VISITA COMPLETA CON TRATAMIENTOS
            grupo.MapPost(
                "/visitas/completa",
                async (
                    CrearVisitaCompletaRequest request,
                    ExpedientesRepository repository,
                    CancellationToken ct) =>
                {
                    if (request.IdPaciente <= 0 ||
                        request.IdEmpleado <= 0 ||
                        request.Fecha == default ||
                        string.IsNullOrWhiteSpace(request.Motivo) ||
                        request.Motivo.Trim().Length > 255 ||
                        request.Notas?.Length > 500 ||
                        request.Tratamientos == null ||
                        request.Tratamientos.Any(t =>
                            t.IdTratamiento <= 0 || t.Cantidad < 1) ||
                        request.Tratamientos
                            .Select(t => t.IdTratamiento)
                            .Distinct()
                            .Count() != request.Tratamientos.Count)
                    {
                        return Results.BadRequest(
                            new { mensaje = "Datos de visita o tratamientos inválidos." });
                    }

                    request.Motivo = request.Motivo.Trim();

                    try
                    {
                        int idVisita =
                            await repository.CrearVisitaCompletaAsync(
                                request, ct);

                        return Results.Created(
                            $"/api/expedientes/visitas/{idVisita}",
                            new { IdVisita = idVisita });
                    }
                    catch (SqlException ex)
                        when (ex.Number >= 50000 &&
                              ex.Number < 51000)
                    {
                        return Results.BadRequest(
                            new { mensaje = ex.Message });
                    }
                });


            // MODIFICAR VISITA COMPLETA CON TRATAMIENTOS
            grupo.MapPut(
                "/visitas/{idVisita:int}/completa",
                async (
                    int idVisita,
                    ModificarVisitaCompletaRequest request,
                    ExpedientesRepository repository,
                    CancellationToken ct) =>
                {
                    if (idVisita <= 0 ||
                        request.IdEmpleado <= 0 ||
                        request.Fecha == default ||
                        string.IsNullOrWhiteSpace(request.Motivo) ||
                        request.Motivo.Trim().Length > 255 ||
                        request.Notas?.Length > 500 ||
                        request.Tratamientos == null ||
                        request.Tratamientos.Any(t =>
                            t.IdTratamiento <= 0 || t.Cantidad < 1) ||
                        request.Tratamientos
                            .Select(t => t.IdTratamiento)
                            .Distinct()
                            .Count() != request.Tratamientos.Count)
                    {
                        return Results.BadRequest(
                            new { mensaje = "Datos de visita o tratamientos inválidos." });
                    }

                    request.Motivo = request.Motivo.Trim();

                    try
                    {
                        await repository.ModificarVisitaCompletaAsync(
                            idVisita, request, ct);

                        return Results.NoContent();
                    }
                    catch (SqlException ex)
                        when (ex.Number >= 50000 &&
                              ex.Number < 51000)
                    {
                        return Results.BadRequest(
                            new { mensaje = ex.Message });
                    }
                });
        }
    }
}
