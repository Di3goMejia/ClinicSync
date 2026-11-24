using System.Security.Claims;
using System.Data;
using Dapper;
using ClinicSyncServer.Common.Data;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace ClinicSyncServer.Features.Finanzas
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.IEquatable&lt;ClinicSyncServer.Features.Finanzas.MaterialConsumoRequest&gt;" />
    public record MaterialConsumoRequest(
        int IdMaterial,
        string Nombre,
        decimal Cantidad);

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.IEquatable&lt;ClinicSyncServer.Features.Finanzas.CrearIngresoRequest&gt;" />
    public record CrearIngresoRequest(
        int IdPaciente,
        DateTime Fecha,
        List<int> IdsTratamientosVisita,
        decimal Descuento,
        string MedioPago,
        List<MaterialConsumoRequest> Materiales);

    /// <summary>
    /// 
    /// </summary>
    public static class FinanzasEndpoints
    {

        /// <summary>
        /// 
        /// </summary>
        public sealed class IngresoDetalleDto
        {
            /// <summary>
            /// Gets or sets the identifier ingreso.
            /// </summary>
            /// <value>
            /// The identifier ingreso.
            /// </value>
            public int IdIngreso { get; set; }
            /// <summary>
            /// Gets or sets the identifier paciente.
            /// </summary>
            /// <value>
            /// The identifier paciente.
            /// </value>
            public int IdPaciente { get; set; }
            /// <summary>
            /// Gets or sets the paciente.
            /// </summary>
            /// <value>
            /// The paciente.
            /// </value>
            public string Paciente { get; set; } = "";
            /// <summary>
            /// Gets or sets the fecha.
            /// </summary>
            /// <value>
            /// The fecha.
            /// </value>
            public DateTime Fecha { get; set; }
            /// <summary>
            /// Gets or sets the subtotal.
            /// </summary>
            /// <value>
            /// The subtotal.
            /// </value>
            public decimal Subtotal { get; set; }
            /// <summary>
            /// Gets or sets the descuento.
            /// </summary>
            /// <value>
            /// The descuento.
            /// </value>
            public decimal Descuento { get; set; }
            /// <summary>
            /// Gets or sets the total.
            /// </summary>
            /// <value>
            /// The total.
            /// </value>
            public decimal Total { get; set; }
            /// <summary>
            /// Gets or sets the medio pago.
            /// </summary>
            /// <value>
            /// The medio pago.
            /// </value>
            public string MedioPago { get; set; } = "";
            /// <summary>
            /// Gets or sets a value indicating whether this <see cref="IngresoDetalleDto"/> is activo.
            /// </summary>
            /// <value>
            ///   <c>true</c> if activo; otherwise, <c>false</c>.
            /// </value>
            public bool Activo { get; set; }

            /// <summary>
            /// Gets or sets the materiales.
            /// </summary>
            /// <value>
            /// The materiales.
            /// </value>
            public List<MaterialDetalleDto> Materiales { get; set; } = new();

            /// <summary>
            /// Gets or sets the tratamientos.
            /// </summary>
            /// <value>
            /// The tratamientos.
            /// </value>
            public List<TratamientoIngresoDto> Tratamientos
            { get; set; } = new();
        }

        /// <summary>
        /// 
        /// </summary>
        public sealed class MaterialDetalleDto
        {
            /// <summary>
            /// Gets or sets the identifier material.
            /// </summary>
            /// <value>
            /// The identifier material.
            /// </value>
            public int IdMaterial { get; set; }
            /// <summary>
            /// Gets or sets the nombre.
            /// </summary>
            /// <value>
            /// The nombre.
            /// </value>
            public string Nombre { get; set; } = "";
            /// <summary>
            /// Gets or sets the cantidad.
            /// </summary>
            /// <value>
            /// The cantidad.
            /// </value>
            public decimal Cantidad { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public sealed class TratamientoIngresoDto
        {
            /// <summary>
            /// Gets or sets the identifier tratamiento visita.
            /// </summary>
            /// <value>
            /// The identifier tratamiento visita.
            /// </value>
            public int IdTratamientoVisita { get; set; }

            /// <summary>
            /// Gets or sets the identifier tratamiento.
            /// </summary>
            /// <value>
            /// The identifier tratamiento.
            /// </value>
            public int IdTratamiento { get; set; }

            /// <summary>
            /// Gets or sets the nombre.
            /// </summary>
            /// <value>
            /// The nombre.
            /// </value>
            public string Nombre { get; set; } = "";

            /// <summary>
            /// Gets or sets the precio.
            /// </summary>
            /// <value>
            /// The precio.
            /// </value>
            public decimal Precio { get; set; }

            /// <summary>
            /// Gets or sets the cantidad.
            /// </summary>
            /// <value>
            /// The cantidad.
            /// </value>
            public int Cantidad { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public sealed record EditarIngresoDto(
            int IdPaciente,
            DateTime Fecha,
            decimal Descuento,
            string MedioPago,
            bool Activo,
            List<int> IdsTratamientos,
            List<MaterialConsumoRequest>? Materiales = null
        );

        /// <summary>
        /// 
        /// </summary>
        public sealed class ResumenDto
        {
            /// <summary>
            /// Gets or sets the total ingresos.
            /// </summary>
            /// <value>
            /// The total ingresos.
            /// </value>
            public decimal TotalIngresos { get; set; }
            /// <summary>
            /// Gets or sets the total egresos.
            /// </summary>
            /// <value>
            /// The total egresos.
            /// </value>
            public decimal TotalEgresos { get; set; }
            /// <summary>
            /// Gets or sets the saldo neto.
            /// </summary>
            /// <value>
            /// The saldo neto.
            /// </value>
            public decimal SaldoNeto { get; set; }
            /// <summary>
            /// Gets or sets the grafico.
            /// </summary>
            /// <value>
            /// The grafico.
            /// </value>
            public GraficoResumenDto? Grafico { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public sealed class GraficoResumenDto
        {
            /// <summary>
            /// Gets or sets the periodo.
            /// </summary>
            /// <value>
            /// The periodo.
            /// </value>
            public string Periodo { get; set; } = "";
            /// <summary>
            /// Gets or sets the agrupacion.
            /// </summary>
            /// <value>
            /// The agrupacion.
            /// </value>
            public string Agrupacion { get; set; } = "";
            /// <summary>
            /// Gets or sets the desde.
            /// </summary>
            /// <value>
            /// The desde.
            /// </value>
            public string Desde { get; set; } = "";
            /// <summary>
            /// Gets or sets the hasta.
            /// </summary>
            /// <value>
            /// The hasta.
            /// </value>
            public string Hasta { get; set; } = "";

            /// <summary>
            /// Gets or sets the etiquetas.
            /// </summary>
            /// <value>
            /// The etiquetas.
            /// </value>
            public IReadOnlyList<string> Etiquetas { get; set; } = [];
            /// <summary>
            /// Gets or sets the ingresos.
            /// </summary>
            /// <value>
            /// The ingresos.
            /// </value>
            public IReadOnlyList<decimal> Ingresos { get; set; } = [];
            /// <summary>
            /// Gets or sets the egresos.
            /// </summary>
            /// <value>
            /// The egresos.
            /// </value>
            public IReadOnlyList<decimal> Egresos { get; set; } = [];
        }

        /// <summary>
        /// 
        /// </summary>
        public sealed record PeriodoResumen(string Etiqueta, decimal Ingresos, decimal Egresos);

        /// <summary>
        /// 
        /// </summary>
        public sealed class MovimientoResumenDto
        {
            /// <summary>
            /// Gets or sets the fecha.
            /// </summary>
            /// <value>
            /// The fecha.
            /// </value>
            public DateTime Fecha { get; set; }
            /// <summary>
            /// Gets or sets the concepto.
            /// </summary>
            /// <value>
            /// The concepto.
            /// </value>
            public string Concepto { get; set; } = "";
            /// <summary>
            /// Gets or sets the medio pago.
            /// </summary>
            /// <value>
            /// The medio pago.
            /// </value>
            public string MedioPago { get; set; } = "";
            /// <summary>
            /// Gets or sets the monto.
            /// </summary>
            /// <value>
            /// The monto.
            /// </value>
            public decimal Monto { get; set; }
            /// <summary>
            /// Gets or sets the tipo.
            /// </summary>
            /// <value>
            /// The tipo.
            /// </value>
            public string Tipo { get; set; } = "";
        }

        /// <summary>
        /// Generars the etiquetas.
        /// </summary>
        /// <param name="desde">The desde.</param>
        /// <param name="hasta">The hasta.</param>
        /// <param name="agrupacion">The agrupacion.</param>
        /// <returns></returns>
        private static List<string> GenerarEtiquetas(DateOnly desde, DateOnly hasta, Agrupacion agrupacion)
        {
            var lista = new List<string>();

            switch (agrupacion)
            {
                case Agrupacion.Hora:
                    for (var h = 0; h < 24; h++)
                        lista.Add($"{desde:yyyy-MM-dd}T{h:D2}");
                    break;

                case Agrupacion.Dia:
                    for (var d = desde; d <= hasta; d = d.AddDays(1))
                        lista.Add(d.ToString("yyyy-MM-dd"));
                    break;

                case Agrupacion.Semana:
                    var lunes = desde.AddDays(-(((int)desde.DayOfWeek + 6) % 7));
                    for (var s = lunes; s <= hasta; s = s.AddDays(7))
                        lista.Add(s.ToString("yyyy-MM-dd"));
                    break;

                case Agrupacion.Mes:
                    for (var m = new DateOnly(desde.Year, desde.Month, 1); m <= hasta; m = m.AddMonths(1))
                        lista.Add(m.ToString("yyyy-MM"));
                    break;
            }

            return lista;
        }

        public static void MapFinanzasEndpoints(
            this WebApplication app)
        {
            var grupo = app.MapGroup("/api/finanzas")
                .RequireAuthorization();

            grupo.MapGet("/tratamientos-disponibles",
                async (
                    int idPaciente,
                    DateTime fecha,
                    SqlConnectionFactory factory) =>
                {
                    await using var db = factory.Crear();

                    var datos = await db.QueryAsync(
                        "api.usp_Finanzas_TratamientosDisponibles",
                        new
                        {
                            IdPaciente = idPaciente,
                            Fecha = fecha.Date
                        },
                        commandType: CommandType.StoredProcedure);

                    return Results.Ok(datos);
                });

            grupo.MapGet("/materiales",
                async (SqlConnectionFactory factory) =>
                {
                    await using var db = factory.Crear();

                    var datos = await db.QueryAsync(
                        "api.usp_Finanzas_Materiales",
                        commandType: CommandType.StoredProcedure);

                    return Results.Ok(datos);
                });

            grupo.MapGet("/ingresos",
                async (SqlConnectionFactory factory) =>
                {
                    await using var db = factory.Crear();

                    var datos = await db.QueryAsync(
                        "api.usp_Finanzas_Ingresos_Listar",
                        commandType: CommandType.StoredProcedure);

                    return Results.Ok(datos);
                });

            grupo.MapPost("/ingresos",
                async (
                    CrearIngresoRequest request,
                    ClaimsPrincipal usuario,
                    SqlConnectionFactory factory) =>
                {

                    string? identificador =
                        usuario.FindFirst("sub")?.Value;


                    if (request.IdsTratamientosVisita is null ||
                        request.IdsTratamientosVisita.Count == 0 ||
                        request.IdsTratamientosVisita.Any(id => id <= 0) ||
                        request.IdsTratamientosVisita.Distinct().Count()
                            != request.IdsTratamientosVisita.Count)
                    {
                        return Results.BadRequest(new
                        {
                            mensaje = "Seleccione tratamientos válidos " +
                                      "del expediente del paciente."
                        });
                    }


                    if (!int.TryParse(identificador, out int idUsuario))
                    {
                        Console.WriteLine(
                            "ERROR: El claim sub no contiene un IdUsuario válido.");

                        return Results.Unauthorized();
                    }

                    Console.WriteLine(
                        $"Usuario autenticado correctamente. IdUsuario: {idUsuario}");


                    if (request.IdPaciente <= 0 ||
                        request.Fecha > DateTime.Now ||
                        request.Descuento < 0 ||
                        request.IdsTratamientosVisita is null ||
                        request.IdsTratamientosVisita.Count == 0 ||
                        request.Materiales is null)
                    {
                        return Results.BadRequest(
                            "Los datos del ingreso son inválidos.");
                    }

                    await using var db = factory.Crear();

                    try
                    {
                        int idIngreso = await db.ExecuteScalarAsync<int>(
                            "api.usp_Finanzas_Ingreso_CrearDesdeVisita",
                            new
                            {
                                request.IdPaciente,
                                request.Fecha,
                                request.MedioPago,
                                request.Descuento,
                                IdUsuario = idUsuario,
                                TratamientosJson = JsonSerializer.Serialize(
                                    request.IdsTratamientosVisita),
                                MaterialesJson = JsonSerializer.Serialize(
                                    request.Materiales)
                            },
                            commandType: CommandType.StoredProcedure);

                        return Results.Created(
                            $"/api/finanzas/ingresos/{idIngreso}",
                            new { IdIngreso = idIngreso });
                    }


                    catch (Microsoft.Data.SqlClient.SqlException ex)
                    {
                        Console.WriteLine("========== ERROR SQL INGRESOS ==========");
                        Console.WriteLine($"Número: {ex.Number}");
                        Console.WriteLine($"Mensaje: {ex.Message}");
                        Console.WriteLine(ex.ToString());
                        Console.WriteLine("========================================");

                        return Results.BadRequest(new
                        {
                            mensaje = ex.Message
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("========== ERROR INGRESOS ==========");
                        Console.WriteLine(ex.ToString());
                        Console.WriteLine("====================================");

                        return Results.Problem(
                            title: "No se pudo registrar el ingreso.",
                            statusCode: 500);
                    }


                });

            grupo.MapGet("/tratamientos-catalogo",
                async (SqlConnectionFactory factory) =>
                {
                    await using var db = factory.Crear();
                    var tratamientos = await db.QueryAsync<TratamientoIngresoDto>(
                        /// <summary>
                        /// Initializes a new instance of the <see cref=".$Program"/> class.
                        /// </summary>
                        """
                        SELECT IdTratamiento, Nombre, Precio, 1 AS Cantidad
                        FROM dbo.Tratamientos
                        WHERE Activo = 1
                        ORDER BY Nombre
                        """);
                    return Results.Ok(tratamientos);
                });

            // OBTENER INGRESO PARA EDICIÓN
            grupo.MapGet("/ingresos/{idIngreso:int}",
                async (int idIngreso, SqlConnectionFactory factory) =>
                {
                    await using var db = factory.Crear();

                    using var resultado = await db.QueryMultipleAsync(
                        "api.usp_Finanzas_Ingreso_Obtener",
                        new { IdIngreso = idIngreso },
                        commandType: CommandType.StoredProcedure);

                    var ingreso =
                        await resultado.ReadFirstOrDefaultAsync<IngresoDetalleDto>();

                    if (ingreso == null)
                        return Results.NotFound(
                            new { mensaje = "Ingreso no encontrado." });

                    ingreso.Tratamientos =
                        (await resultado.ReadAsync<TratamientoIngresoDto>())
                        .ToList();

                    ingreso.Materiales =
                        (await resultado.ReadAsync<MaterialDetalleDto>()).ToList();

                    return Results.Ok(ingreso);
                });

            // GUARDAR EDICIÓN
            grupo.MapPut("/ingresos/{idIngreso:int}",
                async (
                    int idIngreso,
                    EditarIngresoDto request,
                    ClaimsPrincipal usuario,
                    SqlConnectionFactory factory,
                    ILogger<Program> logger) =>
                {
                    // En ClinicSync el IdUsuario numérico
                    // está almacenado en el claim "sub".
                    string? identificador =
                        usuario.FindFirst("sub")?.Value;

                    if (!int.TryParse(identificador, out int idUsuario))
                        return Results.Unauthorized();

                    if (idIngreso <= 0 ||
                        request.IdPaciente <= 0 ||
                        request.IdsTratamientos == null ||
                        request.IdsTratamientos.Count == 0 ||
                        request.IdsTratamientos.Any(x => x <= 0) ||
                        request.IdsTratamientos.Distinct().Count() !=
                            request.IdsTratamientos.Count ||
                        request.Descuento < 0 ||
                        request.Fecha > DateTime.Now ||
                        (request.MedioPago != "Efectivo" &&
                         request.MedioPago != "Transferencia"))
                    {
                        return Results.BadRequest(
                            new { mensaje = "Revise los datos del formulario." });
                    }

                    try
                    {
                        await using var db = factory.Crear();


                        int esIngresoDeVisita = await db.ExecuteScalarAsync<int>(
                            @"SELECT CASE WHEN EXISTS (
                            SELECT 1
                            FROM dbo.DetalleIngresos
                            WHERE IdIngreso = @IdIngreso
                             ) THEN 1 ELSE 0 END",
                            new { IdIngreso = idIngreso });

                        string procedimiento = esIngresoDeVisita == 1
                            ? "api.usp_Finanzas_Ingreso_EditarDesdeVisita"
                            : "api.usp_Finanzas_Ingreso_Editar";

                        await db.ExecuteAsync(
                            procedimiento,
                            new
                            {
                                IdIngreso = idIngreso,
                                request.IdPaciente,
                                request.Fecha,
                                request.MedioPago,
                                request.Descuento,
                                request.Activo,
                                IdUsuario = idUsuario,
                                TratamientosJson = JsonSerializer.Serialize(
                                    request.IdsTratamientos),
                                MaterialesJson = request.Materiales is null
                                    ? null
                                    : JsonSerializer.Serialize(request.Materiales)
                            },
                            commandType: CommandType.StoredProcedure);


                        return Results.Ok(new
                        {
                            mensaje = "Ingreso actualizado correctamente."
                        });
                    }
                    catch (Microsoft.Data.SqlClient.SqlException ex)
                        when ((ex.Number >= 51200 && ex.Number <= 51299) ||
                              (ex.Number >= 51401 && ex.Number <= 51416))
                    {
                        return Results.BadRequest(
                            new { mensaje = ex.Message });
                    }

                    catch (Exception ex)
                    {
                        logger.LogError(
                            ex,
                            "Error al actualizar el ingreso {IdIngreso}",
                            idIngreso);

                        return Results.Problem(
                            title: "No se pudo actualizar el ingreso.",
                            statusCode: 500);
                    }
                });

            // DATOS PARA EL GRÁFICO FINANCIERO
            grupo.MapGet("/graficos", async (
                [FromQuery] Periodo periodo,
                [FromQuery] DateOnly? desde,
                [FromQuery] DateOnly? hasta,
                SqlConnectionFactory factory
            ) =>
            {
                var rango = RangoFinanciero.Resolver(periodo, desde, hasta, DateTime.UtcNow);

                await using var db = factory.Crear();

                var filas = await db.QueryAsync<PeriodoResumen>(
                    "api.usp_Finanzas_ResumenPorPeriodo",
                    new
                    {
                        rango.Desde,
                        Hasta = rango.Hasta!.Value,
                        Agrupacion = rango.Agrupacion.ToString().ToUpperInvariant()
                    },
                    commandType: CommandType.StoredProcedure
                );

                var inicio = rango.Desde ?? filas
                    .Select(f => DateOnly.Parse(f.Etiqueta))
                    .DefaultIfEmpty(rango.Hasta!.Value)
                    .Min();

                var etiquetas = GenerarEtiquetas(inicio, rango.Hasta!.Value, rango.Agrupacion);
                var indice = filas.ToDictionary(f => f.Etiqueta);

                var totalIngresos = filas.Sum(p => p.Ingresos);
                var totalEgresos = filas.Sum(p => p.Egresos);
                var saldoNeto = totalIngresos - totalEgresos;

                return Results.Ok(
                    new ResumenDto()
                    {
                        TotalIngresos = totalIngresos,
                        TotalEgresos = totalEgresos,
                        SaldoNeto = saldoNeto,
                        Grafico = new()
                        {
                            Periodo = periodo.ToString().ToLowerInvariant(),
                            Agrupacion = rango.Agrupacion.ToString().ToLowerInvariant(),
                            Desde = inicio.ToString("yyyy-MM-dd"),
                            Hasta = rango.Hasta!.Value.ToString("yyyy-MM-dd"),
                            Etiquetas = etiquetas,
                            Ingresos = [.. etiquetas.Select(e => indice.TryGetValue(e, out var f) ? f.Ingresos : 0m)],
                            Egresos = [.. etiquetas.Select(e => indice.TryGetValue(e, out var f) ? f.Egresos : 0m)]
                        }
                    }
                );
            });

            grupo.MapGet("/resumen", async (
                [FromQuery] Periodo periodo,
                [FromQuery] DateOnly? desde,
                [FromQuery] DateOnly? hasta,
                SqlConnectionFactory factory
            ) =>
            {
                var rango = RangoFinanciero.Resolver(periodo, desde, hasta, DateTime.UtcNow);

                await using var db = factory.Crear();

                var movimientos = await db.QueryAsync<MovimientoResumenDto>(
                    "api.usp_Finanzas_ResumenMovimiento",
                    new
                    {
                        Desde = rango.Desde,
                        Hasta = rango.Hasta!.Value
                    },
                    commandType: CommandType.StoredProcedure);

                return Results.Ok(movimientos);
            });
        }
    }
}

