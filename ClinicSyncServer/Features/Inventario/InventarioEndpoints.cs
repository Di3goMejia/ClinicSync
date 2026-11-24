
using System.IO;
using System.Security.Claims;
using ClinicSyncServer.Features.Inventario.Imagenes;
using ClinicSyncServer.Features.Inventario.Requests;
using ClinicSyncServer.Features.Inventario.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PermisosSeguridad = ClinicSyncServer.Common.Seguridad.Permisos;

namespace ClinicSyncServer.Features.Inventario
{
    /// <summary>
    /// 
    /// </summary>
    public static class InventarioEndpoints
    {
        public static void MapInventarioEndpoints(this WebApplication app)
        {
            var materiales = app
                .MapGroup("/api/inventario/materiales")
                .RequireAuthorization();

            // LISTAR MATERIALES
            materiales.MapGet(
                "/",
                async (
                    [AsParameters] ListarMaterialesQuery query,
                    IInventarioService svc,
                    CancellationToken ct) =>
                {
                    return Results.Ok(
                        await svc.ListarAsync(query, ct));
                })
                .WithName("ListarMateriales")
                .RequireAuthorization(
                    PermisosSeguridad.InventarioLeer);

            // OBTENER MATERIAL POR ID
            materiales.MapGet(
                "/{id:int}",
                async (
                    int id,
                    IInventarioService svc,
                    CancellationToken ct) =>
                {
                    if (id <= 0)
                        return Results.BadRequest();

                    var material =
                        await svc.ObtenerPorIdAsync(id, ct);

                    return material is null
                        ? Results.NotFound()
                        : Results.Ok(material);
                })
                .WithName("ObtenerMaterial")
                .RequireAuthorization(
                    PermisosSeguridad.InventarioLeer);

            // REGISTRAR MATERIAL
            materiales.MapPost(
                "/",
                async (
                    [FromBody] RegistrarMaterialRequest req,
                    ClaimsPrincipal usuario,
                    IInventarioService svc,
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
                        $"/api/inventario/materiales/{id}",
                        new RegistrarMaterialResponse(id));
                })
                .WithName("CrearMaterial")
                .RequireAuthorization(
                    PermisosSeguridad.InventarioCrear);

            // MODIFICAR MATERIAL
            materiales.MapPut(
                "/{id:int}",
                async (
                    int id,
                    [FromBody] MaterialRequest req,
                    ClaimsPrincipal usuario,
                    IInventarioService svc,
                    CancellationToken ct) =>
                {
                    if (id <= 0)
                        return Results.BadRequest();

                    if (!int.TryParse(
                        usuario.FindFirstValue("sub"),
                        out int idUsuario))
                    {
                        return Results.Unauthorized();
                    }

                    var material =
                        await svc.ModificarAsync(
                            id,
                            req,
                            idUsuario,
                            ct);

                    return material is null
                        ? Results.NotFound()
                        : Results.Ok(material);
                })
                .WithName("ModificarMaterial")
                .RequireAuthorization(
                    PermisosSeguridad.InventarioModificar);

            // CAMBIAR ESTADO DEL MATERIAL
            materiales.MapPatch(
                "/{id:int}/estado",
                async (
                    int id,
                    [FromBody] CambiarEstadoMaterialRequest req,
                    IInventarioService svc,
                    CancellationToken ct) =>
                {
                    if (id <= 0)
                        return Results.BadRequest();

                    var cambiado =
                        await svc.CambiarEstadoAsync(
                            id,
                            req.Activo,
                            ct);

                    return cambiado
                        ? Results.NoContent()
                        : Results.NotFound();
                })
                .WithName("CambiarEstadoMaterial")
                .RequireAuthorization(
                    PermisosSeguridad.InventarioEstado);

            // SUBIR IMAGEN DE MATERIAL
            materiales.MapPost(
                "/imagenes",
                async (
                    HttpRequest request,
                    IAlmacenamientoImagenesService almacenamiento,
                    CancellationToken ct) =>
                {
                    try
                    {
                        if (!request.HasFormContentType)
                        {
                            return Results.BadRequest(new
                            {
                                Detail =
                                    "La solicitud debe utilizar multipart/form-data."
                            });
                        }

                        var formulario =
                            await request.ReadFormAsync(ct);

                        var archivo =
                            formulario.Files.GetFile("archivo");

                        if (archivo is null || archivo.Length == 0)
                        {
                            return Results.BadRequest(new
                            {
                                Detail =
                                    "No se recibió ningún archivo. " +
                                    "El campo esperado es 'archivo'."
                            });
                        }

                        string referencia =
                            await almacenamiento.GuardarAsync(
                                archivo,
                                ct);

                        return Results.Ok(new
                        {
                            Imagen = referencia
                        });
                    }
                    catch (ArgumentException ex)
                    {
                        return Results.BadRequest(new
                        {
                            Detail = ex.Message
                        });
                    }
                })
                .Accepts<IFormFile>("multipart/form-data")
                .DisableAntiforgery()
                .WithName("SubirImagenMaterial")
                .RequireAuthorization(
                    PermisosSeguridad.InventarioCrear);

            // OBTENER IMAGEN DE MATERIAL
            materiales.MapGet(
                "/imagenes/{nombreArchivo}",
                (
                    string nombreArchivo,
                    IAlmacenamientoImagenesService almacenamiento) =>
                {
                    try
                    {
                        string ruta =
                            almacenamiento.ObtenerRuta(
                                nombreArchivo);

                        if (!File.Exists(ruta))
                            return Results.NotFound();

                        string extension =
                            Path.GetExtension(ruta)
                                .ToLowerInvariant();

                        string tipoMime = extension switch
                        {
                            ".jpg" => "image/jpeg",
                            ".png" => "image/png",
                            ".webp" => "image/webp",
                            _ => "application/octet-stream"
                        };

                        return Results.File(
                            ruta,
                            contentType: tipoMime,
                            enableRangeProcessing: true);
                    }
                    catch (ArgumentException)
                    {
                        return Results.BadRequest();
                    }
                })
                .WithName("ObtenerImagenMaterial")
                .RequireAuthorization(
                    PermisosSeguridad.InventarioLeer);
        }
    }
}
