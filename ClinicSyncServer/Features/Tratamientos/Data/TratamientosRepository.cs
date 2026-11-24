using ClinicSyncServer.Common.Data;
using ClinicSyncServer.Features.Tratamientos.Requests;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ClinicSyncServer.Features.Tratamientos.Data
{
    /// <summary>
    /// 
    /// </summary>
    public class TratamientosRepository(SqlConnectionFactory factory)
    {
        /// <summary>
        /// Insertars the asynchronous.
        /// </summary>
        /// <param name="req">The req.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public async Task<int> InsertarAsync(RegistrarTratamientoRequest req,
            CancellationToken ct = default)
        {
            await using var db = factory.Crear();
            await db.OpenAsync(ct);

            try
            {
                var parametros = new DynamicParameters();
                parametros.Add("@Nombre", req.Nombre, DbType.String, size: 100);
                parametros.Add("@Precio", req.Precio, DbType.Decimal, precision: 10, scale: 2);
                parametros.Add("@Descripcion", req.Descripcion, DbType.String, size: 200);

                var comando = new CommandDefinition(
                    commandText: "api.usp_Tratamientos_Insertar",
                    parameters: parametros,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: ct
                );

                return await db.QuerySingleAsync<int>(comando);
            }
            catch (SqlException ex) when (SqlErrorTranslator.EsTraducible(ex))
            {
                throw SqlErrorTranslator.Traducir(ex);
            }
        }

        /// <summary>
        /// Obteners the por identifier asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public async Task<Tratamiento?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
        {
            await using var db = factory.Crear();
            await db.OpenAsync(ct);

            try
            {
                var parametros = new CommandDefinition(
                    commandText: "api.usp_Tratamientos_ObtenerPorId",
                    commandType: CommandType.StoredProcedure,
                    parameters: new { IdTratamiento = id },
                    cancellationToken: ct
                );

                return await db.QuerySingleOrDefaultAsync<Tratamiento>(parametros);
            }
            catch (SqlException ex) when (SqlErrorTranslator.EsTraducible(ex))
            {
                throw SqlErrorTranslator.Traducir(ex);
            }
        }

        /// <summary>
        /// Listars the asynchronous.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public async Task<IEnumerable<Tratamiento>> ListarAsync(ListarTratamientoQuery query,
            CancellationToken ct = default)
        {
            await using var db = factory.Crear();
            await db.OpenAsync(ct);

            try
            {
                var parametros = new DynamicParameters();
                parametros.Add("@Nombre", query.Buscar, DbType.String, size: 100);
                parametros.Add("@Activo", query.Activo, DbType.Boolean);

                var comando = new CommandDefinition(
                    commandText: "api.usp_Tratamientos_Listar",
                    commandType: CommandType.StoredProcedure,
                    parameters: parametros,
                    cancellationToken: ct
                );

                return await db.QueryAsync<Tratamiento>(comando);
            }
            catch (SqlException ex) when (SqlErrorTranslator.EsTraducible(ex))
            {
                throw SqlErrorTranslator.Traducir(ex);
            }
        }

        /// <summary>
        /// Modificars the asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="req">The req.</param>
        /// <param name="ct">The ct.</param>
        public async Task ModificarAsync(int id, RegistrarTratamientoRequest req, 
            CancellationToken ct = default)
        {
            await using var db = factory.Crear();
            await db.OpenAsync(ct);

            try
            {
                var parametros = new DynamicParameters();
                parametros.Add("@IdTratamiento", id, DbType.Int32);
                parametros.Add("@Nombre", req.Nombre, DbType.String, size: 100);
                parametros.Add("@Precio", req.Precio, DbType.Decimal, precision: 10, scale: 2);
                parametros.Add("@Descripcion", req.Descripcion, DbType.String, size: 200);

                var comando = new CommandDefinition(
                    commandText: "api.usp_Tratamientos_Modificar",
                    parameters: parametros,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: ct
                );

                await db.ExecuteAsync(comando);
            }
            catch (SqlException ex) when (SqlErrorTranslator.EsTraducible(ex))
            {
                throw SqlErrorTranslator.Traducir(ex);
            }
        }

        /// <summary>
        /// Cambiars the estado asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="activo">if set to <c>true</c> [activo].</param>
        /// <param name="ct">The ct.</param>
        public async Task CambiarEstadoAsync(int id, bool activo, CancellationToken ct = default)
        {
            await using var db = factory.Crear();
            await db.OpenAsync(ct);

            try
            {
                var parametros = new DynamicParameters();
                parametros.Add("@IdTratamiento", id, DbType.Int32);
                parametros.Add("@Activo", activo, DbType.Boolean);

                var comando = new CommandDefinition(
                    commandText: "api.usp_Tratamientos_CambiarEstado",
                    commandType: CommandType.StoredProcedure,
                    parameters: parametros,
                    cancellationToken: ct
                );

                await db.ExecuteAsync(comando);
            }
            catch (SqlException ex) when (SqlErrorTranslator.EsTraducible(ex))
            {
                throw SqlErrorTranslator.Traducir(ex);
            }
        }
    }
}
