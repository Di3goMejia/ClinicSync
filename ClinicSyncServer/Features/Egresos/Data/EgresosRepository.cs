using ClinicSyncServer.Common.Data;
using ClinicSyncServer.Features.Egresos.Requests;
using ClinicSyncServer.Features.Egresos.Responses;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

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
                var p = new DynamicParameters();

                p.Add("@IdUsuario", idUsuario, DbType.Int32);
                p.Add("@Tipo", req.Tipo, DbType.String, size: 20);
                p.Add("@Fecha", req.Fecha, DbType.Date);
                p.Add("@Concepto", req.Concepto, DbType.String, size: 255);
                p.Add("@MedioPago", req.MedioPago, DbType.String, size: 20);
                p.Add("@Monto", req.Monto,
                    DbType.Decimal, precision: 10, scale: 2);

                var cmd = new CommandDefinition(
                    "api.usp_Egresos_Insertar",
                    p,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: ct);

                return await db.QuerySingleAsync<int>(cmd);
            }
            catch (SqlException ex) when (SqlErrorTranslator.EsTraducible(ex))
            {
                throw SqlErrorTranslator.Traducir(ex);
            }
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

                p.Add("@FechaDesde", desde, DbType.Date);
                p.Add("@FechaHasta", hasta, DbType.Date);
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