
using System.Data;
using ClinicSyncServer.Common.Data;
using Dapper;

namespace ClinicSyncServer.Features.Notificaciones;

public static class NotificacionesEndpoints
{
    public static void MapNotificacionesEndpoints(
        this WebApplication app)
    {
        var grupo = app.MapGroup("/api/notificaciones")
            .RequireAuthorization();

        // LISTAR Y GENERAR NOTIFICACIONES
        grupo.MapGet("/",
            async (
                string? tipo,
                string? estado,
                DateTime? desde,
                DateTime? hasta,
                string? buscar,
                SqlConnectionFactory factory) =>
            {
                if (tipo is not null &&
                    tipo != "CITA" && tipo != "STOCK")
                {
                    return Results.BadRequest(
                        new { mensaje = "Tipo inválido." });
                }

                if (estado is not null &&
                    estado != "PENDIENTE" &&
                    estado != "LEIDA")
                {
                    return Results.BadRequest(
                        new { mensaje = "Estado inválido." });
                }

                if (desde > hasta)
                {
                    return Results.BadRequest(
                        new { mensaje = "Rango de fechas inválido." });
                }

                await using var db = factory.Crear();

                // Generar los recordatorios y alertas
                // que correspondan a la situación actual.
                await db.ExecuteAsync(
                    "api.usp_Notificaciones_Sincronizar",
                    commandType: CommandType.StoredProcedure);

                var datos = await db.QueryAsync<NotificacionDto>(
                    "api.usp_Notificaciones_Listar",
                    new
                    {
                        Tipo = tipo,
                        Estado = estado,
                        Desde = desde?.Date,
                        Hasta = hasta?.Date,
                        Buscar = buscar
                    },
                    commandType: CommandType.StoredProcedure);

                return Results.Ok(datos);
            });

        // MARCAR UNA NOTIFICACIÓN COMO LEÍDA
        grupo.MapPatch("/{id:long}/leida",
            async (long id, SqlConnectionFactory factory) =>
            {
                if (id <= 0)
                    return Results.BadRequest();

                await using var db = factory.Crear();

                var cantidad = await db.ExecuteScalarAsync<int>(
                    "api.usp_Notificaciones_MarcarLeida",
                    new { IdNotificacion = id },
                    commandType: CommandType.StoredProcedure);

                return cantidad == 0
                    ? Results.NotFound()
                    : Results.NoContent();
            });

        // MARCAR TODAS COMO LEÍDAS
        grupo.MapPatch("/leer-todas",
            async (SqlConnectionFactory factory) =>
            {
                await using var db = factory.Crear();

                await db.ExecuteAsync(
                    "api.usp_Notificaciones_MarcarTodasLeidas",
                    commandType: CommandType.StoredProcedure);

                return Results.NoContent();
            });

        // ARCHIVAR NOTIFICACIÓN
        grupo.MapPatch("/{id:long}/archivar",
            async (long id, SqlConnectionFactory factory) =>
            {
                if (id <= 0)
                    return Results.BadRequest();

                await using var db = factory.Crear();

                var cantidad = await db.ExecuteScalarAsync<int>(
                    "api.usp_Notificaciones_Archivar",
                    new { IdNotificacion = id },
                    commandType: CommandType.StoredProcedure);

                return cantidad == 0
                    ? Results.NotFound()
                    : Results.NoContent();
            });
    }
}

public sealed class NotificacionDto
{
    public long IdNotificacion { get; set; }
    public string Tipo { get; set; } = "";
    public int IdReferencia { get; set; }
    public string Titulo { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public DateTime FechaEvento { get; set; }
    public DateTime FechaCreacion { get; set; }
    public bool Leida { get; set; }
    public bool Vigente { get; set; }
    public bool Archivada { get; set; }
}
