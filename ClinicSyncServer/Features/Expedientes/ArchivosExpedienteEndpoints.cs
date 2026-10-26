
using ClinicSyncServer.Common.Data;
using ClinicSyncServer.Features.Expedientes.Data;
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
    public static class ArchivosExpedienteEndpoints
    {
        private const long TamanoMaximo = 10 * 1024 * 1024;

        public static void MapArchivosExpedienteEndpoints(
            this WebApplication app)
        {
            var grupo = app.MapGroup("/api/expedientes")
                .RequireAuthorization();

            // LISTAR ARCHIVOS ACTIVOS DE UNA VISITA
            grupo.MapGet(
                "/visitas/{idVisita:int}/archivos",
                async (
                    int idVisita,
                    ClaimsPrincipal usuario,
                    ExpedientesRepository expedientes,
                    SqlConnectionFactory factory) =>
                {
                    if (idVisita <= 0)
                        return Results.BadRequest();

                    var visita =
                        await expedientes.ObtenerVisitaAsync(idVisita);

                    if (visita == null)
                        return Results.NotFound();

                    await using var db = factory.Crear();
                    await db.OpenAsync();

                    var archivos = await db.QueryAsync(
                        @"SELECT
                            IdArchivo,
                            NombreArchivo,
                            FechaSubida
                          FROM dbo.ArchivosExpediente
                          WHERE IdVisita = @IdVisita
                            AND Activo = 1
                          ORDER BY FechaSubida DESC",
                        new
                        {
                            IdVisita = idVisita
                        });

                    return Results.Ok(archivos);
                })
                .RequireAuthorization(
                    PermisosSeguridad.ArchivosLeer);

            // SUBIR ARCHIVO A UNA VISITA
            grupo.MapPost(
                "/visitas/{idVisita:int}/archivos",
                async (
                    int idVisita,
                    IFormFile archivo,
                    ClaimsPrincipal usuario,
                    ExpedientesRepository expedientes,
                    SqlConnectionFactory factory,
                    IWebHostEnvironment entorno) =>
                {
                    if (idVisita <= 0)
                        return Results.BadRequest();

                    // OBTENER USUARIO AUTENTICADO
                    if (!int.TryParse(
                        usuario.FindFirstValue("sub"),
                        out int idUsuario))
                    {
                        return Results.Unauthorized();
                    }

                    // COMPROBAR QUE LA VISITA EXISTA
                    var visita =
                        await expedientes.ObtenerVisitaAsync(idVisita);

                    if (visita == null)
                        return Results.NotFound();

                    // VALIDAR TAMAÑO
                    if (archivo.Length == 0 ||
                        archivo.Length > TamanoMaximo)
                    {
                        return Results.BadRequest(new
                        {
                            mensaje =
                                "El archivo está vacío o supera los 10 MB."
                        });
                    }

                    // VALIDAR EXTENSIÓN
                    string extension =
                        Path.GetExtension(archivo.FileName)
                            .ToLowerInvariant();

                    if (extension != ".jpg" &&
                        extension != ".jpeg" &&
                        extension != ".png" &&
                        extension != ".pdf")
                    {
                        return Results.BadRequest(new
                        {
                            mensaje =
                                "Solo se permiten archivos JPG, PNG y PDF."
                        });
                    }

                    // LEER CONTENIDO
                    byte[] contenido;

                    using (var memoria = new MemoryStream())
                    {
                        await archivo.CopyToAsync(memoria);

                        contenido = memoria.ToArray();
                    }

                    // VALIDAR CONTENIDO REAL
                    if (!TipoValido(contenido, extension))
                    {
                        return Results.BadRequest(new
                        {
                            mensaje =
                                "El contenido no corresponde al tipo de archivo."
                        });
                    }

                    string tipoMime = extension switch
                    {
                        ".png" => "image/png",
                        ".pdf" => "application/pdf",
                        _ => "image/jpeg"
                    };

                    // GENERAR HASH SHA256
                    byte[] hash =
                        SHA256.HashData(contenido);

                    // CARPETA PRIVADA DEL SERVIDOR
                    string carpeta = Path.Combine(
                        entorno.ContentRootPath,
                        "ArchivosPrivados",
                        "Expedientes");

                    Directory.CreateDirectory(carpeta);

                    // NOMBRE ÚNICO
                    string nombreFisico =
                        Guid.NewGuid().ToString("N") +
                        extension;

                    string rutaFisica =
                        Path.Combine(carpeta, nombreFisico);

                    await using var db = factory.Crear();
                    await db.OpenAsync();

                    // OBTENER PACIENTE DE LA VISITA
                    int? idPaciente =
                        await db.QueryFirstOrDefaultAsync<int?>(
                            @"SELECT IdPaciente
                              FROM dbo.Expedientes
                              WHERE IdExpediente = @IdExpediente",
                            new
                            {
                                visita.IdExpediente
                            });

                    if (idPaciente == null)
                        return Results.NotFound();

                    try
                    {
                        // GUARDAR ARCHIVO FÍSICO
                        await File.WriteAllBytesAsync(
                            rutaFisica,
                            contenido);

                        // REGISTRAR EN SQL SERVER
                        await db.ExecuteAsync(
                            "api.usp_ArchivosExpediente_Insertar",
                            new
                            {
                                IdPaciente = idPaciente.Value,
                                IdVisita = idVisita,
                                IdUsuario = idUsuario,
                                NombreArchivo =
                                    Path.GetFileName(archivo.FileName),
                                Ruta = rutaFisica,
                                Descripcion = (string?)null,
                                HashArchivo = hash,
                                TipoMime = tipoMime,
                                TamanoBytes = contenido.Length
                            },
                            commandType:
                                CommandType.StoredProcedure);

                        return Results.Ok(new
                        {
                            mensaje =
                                "El archivo se subió exitosamente."
                        });
                    }
                    catch (Exception ex)
                    {
                        if (File.Exists(rutaFisica))
                            File.Delete(rutaFisica);

                        Console.WriteLine(
                            "ERROR AL SUBIR ARCHIVO: " + ex.ToString()
                        );

                        throw;
                    }

                })
                .DisableAntiforgery()
                .RequireAuthorization(
                    PermisosSeguridad.ArchivosSubir);

            // DESACTIVAR ARCHIVO
            grupo.MapPut(
                "/archivos/{idArchivo:int}/estado",
                async (
                    int idArchivo,
                    [FromBody] CambiarEstadoArchivoRequest request,
                    ClaimsPrincipal usuario,
                    SqlConnectionFactory factory) =>
                {
                    if (idArchivo <= 0)
                        return Results.BadRequest();

                    // SOLO PERMITIR DESACTIVACIÓN
                    if (request.Activo)
                    {
                        return Results.BadRequest(new
                        {
                            mensaje =
                                "Esta operación solamente permite desactivar archivos."
                        });
                    }

                    await using var db = factory.Crear();
                    await db.OpenAsync();

                    // DESACTIVACIÓN LÓGICA
                    int filas = await db.ExecuteAsync(
                        @"UPDATE dbo.ArchivosExpediente
                          SET Activo = 0
                          WHERE IdArchivo = @IdArchivo
                            AND Activo = 1",
                        new
                        {
                            IdArchivo = idArchivo
                        });

                    if (filas == 0)
                        return Results.NotFound();

                    return Results.NoContent();
                })
                .RequireAuthorization(
                    PermisosSeguridad.ArchivosDesactivar);
        }

        // VALIDAR FIRMA DEL ARCHIVO
        private static bool TipoValido(
            byte[] datos,
            string extension)
        {
            if (extension == ".pdf")
            {
                return datos.Length >= 5 &&
                    datos[0] == 0x25 &&
                    datos[1] == 0x50 &&
                    datos[2] == 0x44 &&
                    datos[3] == 0x46 &&
                    datos[4] == 0x2D;
            }

            if (extension == ".png")
            {
                byte[] firma =
                {
                    137, 80, 78, 71, 13, 10, 26, 10
                };

                return datos.Length >= firma.Length &&
                    datos.Take(8).SequenceEqual(firma);
            }

            // JPG / JPEG
            return datos.Length >= 4 &&
                datos[0] == 0xFF &&
                datos[1] == 0xD8 &&
                datos[2] == 0xFF;
        }
    }

    // MODELO PARA CAMBIAR ESTADO
    public class CambiarEstadoArchivoRequest
    {
        public bool Activo { get; set; }
    }
}
