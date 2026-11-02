using System.Security.Claims;
using System.Data;
using Dapper;
using ClinicSyncServer.Common.Data;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace ClinicSyncServer.Features.Finanzas
{
    public record MaterialConsumoRequest(
        int IdMaterial,
        string Nombre,
        decimal Cantidad);

    public record CrearIngresoRequest(
        int IdPaciente,
        DateTime Fecha,
        List<int> IdsTratamientosVisita,
        decimal Descuento,
        string MedioPago,
        List<MaterialConsumoRequest> Materiales);

    public static class FinanzasEndpoints
    {

        public sealed class IngresoDetalleDto
        {
            public int IdIngreso { get; set; }
            public int IdPaciente { get; set; }
            public string Paciente { get; set; } = "";
            public DateTime Fecha { get; set; }
            public decimal Subtotal { get; set; }
            public decimal Descuento { get; set; }
            public decimal Total { get; set; }
            public string MedioPago { get; set; } = "";
            public bool Activo { get; set; }

            public List<TratamientoIngresoDto> Tratamientos
            { get; set; } = new();
        }

        public sealed class TratamientoIngresoDto
        {
            public int IdTratamiento { get; set; }
            public string Nombre { get; set; } = "";
            public decimal Precio { get; set; }
            public int Cantidad { get; set; }
        }

        public sealed record EditarIngresoDto(
            int IdPaciente,
            DateTime Fecha,
            decimal Descuento,
            string MedioPago,
            bool Activo,
            List<int> IdsTratamientos
        );

        public sealed class ResumenDto
        {
            public decimal TotalIngresos { get; set; }
            public decimal TotalEgresos { get; set; }
            public decimal SaldoNeto { get; set; }
            public GraficoResumenDto? Grafico { get; set; }
        }

        public sealed class GraficoResumenDto
        {
            public string Periodo { get; set; } = "";
            public string Agrupacion { get; set; } = "";
            public string Desde { get; set; } = "";
            public string Hasta { get; set; } = "";

            public IReadOnlyList<string> Etiquetas { get; set; } = [];
            public IReadOnlyList<decimal> Ingresos { get; set; } = [];
            public IReadOnlyList<decimal> Egresos { get; set; } = [];
        }

        public sealed record PeriodoResumen(string Etiqueta, decimal Ingresos, decimal Egresos);

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

                    if (!int.TryParse(identificador, out int idUsuario))
                    {
                        Console.WriteLine(
                            "ERROR: El claim sub no contiene un IdUsuario válido.");

                        return Results.Unauthorized();
                    }

                    Console.WriteLine(
                        $"Usuario autenticado correctamente. IdUsuario: {idUsuario}");


                    if (request.IdPaciente <= 0 ||
                        request.Fecha.Date > DateTime.Today ||
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
                            "api.usp_Finanzas_Ingreso_Crear",
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
                        request.Fecha.Date > DateTime.Today ||
                        (request.MedioPago != "Efectivo" &&
                         request.MedioPago != "Transferencia"))
                    {
                        return Results.BadRequest(
                            new { mensaje = "Revise los datos del formulario." });
                    }

                    try
                    {
                        await using var db = factory.Crear();

                        await db.ExecuteAsync(
                            "api.usp_Finanzas_Ingreso_Editar",
                            new
                            {
                                IdIngreso = idIngreso,
                                request.IdPaciente,
                                request.Fecha,
                                request.MedioPago,
                                request.Descuento,
                                request.Activo,
                                IdUsuario = idUsuario,
                                TratamientosJson =
                                    System.Text.Json.JsonSerializer.Serialize(
                                        request.IdsTratamientos)
                            },
                            commandType: CommandType.StoredProcedure);

                        return Results.Ok(new
                        {
                            mensaje = "Ingreso actualizado correctamente."
                        });
                    }
                    catch (Microsoft.Data.SqlClient.SqlException ex)
                        when (ex.Number >= 51200 && ex.Number <= 51299)
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
                    new { 
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
        }
    }
}

