using ClinicSyncServer.Common.Data;
using ClinicSyncServer.Features.Egresos.Requests;
using ClinicSyncServer.Features.Egresos.Responses;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;

namespace ClinicSyncServer.Features.Egresos.Data
{
    public class EgresosRepository(SqlConnectionFactory factory)
    {
        // REGISTRAR EGRESO
        public async Task<int> InsertarAsync(
            RegistrarEgresoRequest req,
            int idUsuario,
            CancellationToken ct = default)
        {
            await using var db = factory.Crear();
            await db.OpenAsync(ct);

            try
            {
                string? materiales =
                    req.Materiales is { Count: > 0 }
                        ? JsonSerializer.Serialize(
                            req.Materiales.Select(m => new
                            {
                                m.IdMaterial,
                                m.Cantidad
                            }))
                        : null;

                var p = new DynamicParameters();

                p.Add("@IdUsuario", idUsuario, DbType.Int32);
                p.Add("@Tipo", req.Tipo, DbType.String, size: 20);
                p.Add("@Fecha", req.Fecha, DbType.DateTime2);
                p.Add("@Concepto", req.Concepto, DbType.String, size: 255);
                p.Add("@MedioPago", req.MedioPago, DbType.String, size: 20);
                p.Add("@Monto", req.Monto,
                    DbType.Decimal, precision: 10, scale: 2);
                p.Add("@Materiales", materiales, DbType.String, size: -1);
                p.Add("@IdEgreso",
                    dbType: DbType.Int32,
                    direction: ParameterDirection.Output);

                var cmd = new CommandDefinition(
                    "api.usp_Egresos_Insertar",
                    p,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: ct);

                await db.ExecuteAsync(cmd);

                return p.Get<int>("@IdEgreso");
            }

            catch (SqlException ex) when (SqlErrorTranslator.EsTraducible(ex))
            {
                throw SqlErrorTranslator.Traducir(ex);
            }
        }

        public async Task<EgresoListado?> ObtenerAsync(
            int idEgreso,
            CancellationToken ct = default)
        {
            await using var db = factory.Crear();
            await db.OpenAsync(ct);

            return await db.QuerySingleOrDefaultAsync<EgresoListado>(
                new CommandDefinition(
                    "api.usp_Egresos_Obtener",
                    new { IdEgreso = idEgreso },
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: ct));
        }
        public async Task ActualizarAsync(
            int idEgreso,
            ActualizarEgresoRequest req,
            int idUsuario,
            CancellationToken ct = default)
        {
            await using var db = factory.Crear();
            await db.OpenAsync(ct);

            await db.ExecuteAsync(new CommandDefinition(
                "api.usp_Egresos_Editar",
                new
                {
                    IdEgreso = idEgreso,
                    req.Fecha,
                    req.Concepto,
                    req.MedioPago,
                    req.Monto,
                    IdUsuario = idUsuario,

                    // ENVIAR EL ESTADO ACTIVO AL PROCEDIMIENTO
                    Activo = req.Activo
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: ct));
        }


        // LISTAR EGRESOS POR RANGO DE FECHAS
        public async Task<IEnumerable<EgresoListado>> ListarAsync(
            DateTime desde,
            DateTime hasta,
            bool? activo = null,
            CancellationToken ct = default)
        {
            await using var db = factory.Crear();
            await db.OpenAsync(ct);

            try
            {
                var p = new DynamicParameters();

                p.Add("@FechaDesde", desde, DbType.DateTime2);
                p.Add("@FechaHasta", hasta, DbType.DateTime2);
                p.Add("@Activo", activo, DbType.Boolean);

                var cmd = new CommandDefinition(
                    "api.usp_Egresos_Listar",
                    p,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: ct);

                return await db.QueryAsync<EgresoListado>(cmd);
            }
            catch (SqlException ex) when (SqlErrorTranslator.EsTraducible(ex))
            {
                throw SqlErrorTranslator.Traducir(ex);
            }
        }
    }
}