
using ClinicSyncServer.Common;
using ClinicSyncServer.Common.Data;
using ClinicSyncServer.Features.Expedientes.Data;
using ClinicSyncServer.Features.Expedientes.Requests;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using PermisosSeguridad = ClinicSyncServer.Common.Seguridad.Permisos;

namespace ClinicSyncServer.Features.Expedientes
{
    /// <summary>
    /// 
    /// </summary>
    public static class ArchivosExpedienteEndpoints
    {
        public static void MapArchivosExpedienteEndpoints(
            this WebApplication app)
        {
            var grupo = app.MapGroup("/api/")
                .RequireAuthorization();

            // LISTAR ARCHIVOS GENERALES DEL EXPEDIENTE
            grupo.MapGet(
                "/expedientes/{idExpediente:int}/archivos",
                async (
                    int idExpediente,
                    IExpedientesService expedientes,
                    IArchivosExpedienteService archivos,
                    CancellationToken ct) =>
                {
                    var expediente = await expedientes.ObtenerPorIdAsync(
                        idExpediente,
                        null,
                        ct);

                    if (expediente is null)
                        return Results.NotFound();

                    var registros = await archivos.ListarAsync(
                        expediente.IdPaciente,
                        null,
                        ct);

                    return Results.Ok(
                        registros.Where(r =>
                            r.Activo && r.IdVisita is null));
                })
                .RequireAuthorization(PermisosSeguridad.ArchivosLeer);

            // SUBIR ARCHIVO GENERAL AL EXPEDIENTE
            grupo.MapPost(
                "/expedientes/{idExpediente:int}/archivos",
                async (
                    int idExpediente,
                    IFormFile archivo,
                    [FromForm] string? descripcion,
                    ClaimsPrincipal usuario,
                    IArchivosExpedienteService archivos,
                    CancellationToken ct) =>
                {
                    if (!int.TryParse(
                        usuario.FindFirstValue("sub"),
                        out int idUsuario))
                    {
                        return Results.Unauthorized();
                    }

                    var subido = await archivos.SubirExpedienteAsync(
                        idUsuario,
                        idExpediente,
                        archivo,
                        descripcion,
                        ct);

                    return subido is null
                        ? Results.BadRequest()
                        : Results.Created(
                            $"/api/archivos/{subido.IdArchivo}",
                            subido);
                })
                .RequireAuthorization(PermisosSeguridad.ArchivosSubir)
                .DisableAntiforgery();

            // LISTAR ARCHIVOS ACTIVOS DE UNA VISITA
            grupo.MapGet(
                "/visitas/{idVisita:int}/archivos",
                async (
                    int idVisita,
                    IVisitasService visitas,
                    IExpedientesService expedientes,
                    IArchivosExpedienteService archivos) =>
                {
                    var visita = await visitas.ObtenerPorIdAsync(idVisita);
                    if (visita is null) 
                        return Results.NotFound();

                    var expediente = await expedientes.ObtenerPorIdAsync(visita.IdExpediente, null);
                    if (expediente is null)
                        return Results.NotFound();

                    var registros = await archivos.ListarAsync(
                        expediente.IdPaciente,
                        idVisita);

                    return Results.Ok(
                        registros.Where(r => r.Activo));
                }
            ).RequireAuthorization(PermisosSeguridad.ArchivosLeer);

            // SUBIR ARCHIVO A UNA VISITA
            grupo.MapPost(
                "/visitas/{idVisita:int}/archivos",
                async (
                    int idVisita,
                    IFormFile archivo,
                    [FromForm] string? descripcion,
                    ClaimsPrincipal usuario,
                    IArchivosExpedienteService archivos) =>
                {
                    // OBTENER USUARIO AUTENTICADO
                    if (!int.TryParse(
                        usuario.FindFirstValue("sub"),
                        out int idUsuario))
                    {
                        return Results.Unauthorized();
                    }

                    var subido = await archivos.SubirAsync(idUsuario, idVisita, archivo, descripcion);
                    if (subido is null) 
                        return Results.BadRequest();

                    return Results.Created($"/api/archivos/{subido.IdArchivo}", subido);
                })
                .RequireAuthorization(PermisosSeguridad.ArchivosSubir)
                .DisableAntiforgery();

            // DESCARGAR ARCHIVO
            grupo.MapGet("/archivos/{idArchivo:guid}/contenido",
                async (Guid idArchivo, HttpContext ctx, ClaimsPrincipal user, IArchivosExpedienteService svc, CancellationToken ct) =>
                {
                    var idActor = int.Parse(user.FindFirstValue("sub")!);
                    var contenido = await svc.AbrirContenidoAsync(idArchivo, ct);

                    if (contenido is null) return Results.NotFound();

                    using (contenido.Propietario)
                    {
                        ctx.Response.ContentType = contenido.TipoContenido;
                        ctx.Response.Headers.CacheControl = "private, no-store";
                        await contenido.Stream.CopyToAsync(ctx.Response.Body, ct);
                    }

                    return Results.Empty;
                })
                .RequireAuthorization(PermisosSeguridad.ArchivosLeer);

            // DESACTIVAR ARCHIVO
            grupo.MapPut(
                "/archivos/{idArchivo:guid}/estado",
                async (
                    Guid idArchivo,
                    [FromBody] CambiarEstadoArchivoRequest request,
                    IArchivosExpedienteService svc,
                    ClaimsPrincipal usuario) =>
                {
                    if (request.Activo)
                        throw new ReglaNegocioException("Solo se permite desactivar el archivo.", 0);

                    await svc.CambiarEstadoAsync(idArchivo, request.Activo);
                    return Results.NoContent();
                })
                .RequireAuthorization(
                    PermisosSeguridad.ArchivosDesactivar);
        }
    }
}
