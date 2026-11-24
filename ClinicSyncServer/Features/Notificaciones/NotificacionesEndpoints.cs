
using System.Data;
using ClinicSyncServer.Common.Data;
using Dapper;

namespace ClinicSyncServer.Features.Notificaciones;

/// <summary>
/// 
/// </summary>
public static class NotificacionesEndpoints
{
    /// <summary>
    /// Maps the notificaciones endpoints.
    /// </summary>
    /// <param name="app">The application.</param>
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

/// <summary>
/// 
/// </summary>
public sealed class NotificacionDto
{
    /// <summary>
    /// Gets or sets the identifier notificacion.
    /// </summary>
    /// <value>
    /// The identifier notificacion.
    /// </value>
    public long IdNotificacion { get; set; }
    /// <summary>
    /// Gets or sets the tipo.
    /// </summary>
    /// <value>
    /// The tipo.
    /// </value>
    public string Tipo { get; set; } = "";
    /// <summary>
    /// Gets or sets the identifier referencia.
    /// </summary>
    /// <value>
    /// The identifier referencia.
    /// </value>
    public int IdReferencia { get; set; }
    /// <summary>
    /// Gets or sets the titulo.
    /// </summary>
    /// <value>
    /// The titulo.
    /// </value>
    public string Titulo { get; set; } = "";
    /// <summary>
    /// Gets or sets the descripcion.
    /// </summary>
    /// <value>
    /// The descripcion.
    /// </value>
    public string Descripcion { get; set; } = "";
    /// <summary>
    /// Gets or sets the fecha evento.
    /// </summary>
    /// <value>
    /// The fecha evento.
    /// </value>
    public DateTime FechaEvento { get; set; }
    /// <summary>
    /// Gets or sets the fecha creacion.
    /// </summary>
    /// <value>
    /// The fecha creacion.
    /// </value>
    public DateTime FechaCreacion { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="NotificacionDto"/> is leida.
    /// </summary>
    /// <value>
    ///   <c>true</c> if leida; otherwise, <c>false</c>.
    /// </value>
    public bool Leida { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="NotificacionDto"/> is vigente.
    /// </summary>
    /// <value>
    ///   <c>true</c> if vigente; otherwise, <c>false</c>.
    /// </value>
    public bool Vigente { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="NotificacionDto"/> is archivada.
    /// </summary>
    /// <value>
    ///   <c>true</c> if archivada; otherwise, <c>false</c>.
    /// </value>
    public bool Archivada { get; set; }
}
