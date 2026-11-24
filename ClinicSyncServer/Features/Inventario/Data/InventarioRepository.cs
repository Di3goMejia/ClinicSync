
using ClinicSyncServer.Common.Data;
using ClinicSyncServer.Features.Inventario.Requests;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ClinicSyncServer.Features.Inventario.Data
{
    /// <summary>
    /// 
    /// </summary>
    public class InventarioRepository(SqlConnectionFactory factory)
    {
        /// <summary>
        /// Insertars the asynchronous.
        /// </summary>
        /// <param name="req">The req.</param>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public async Task<int> InsertarAsync(
            RegistrarMaterialRequest req,
            int idUsuario,
            CancellationToken ct = default)
        {
            await using var db = factory.Crear();
            await db.OpenAsync(ct);

            try
            {
                var p = new DynamicParameters();

                p.Add("@Codigo", req.Codigo, DbType.String, size: 30);
                p.Add("@Nombre", req.Nombre, DbType.String, size: 50);
                p.Add("@UnidadMedida", req.UnidadMedida, DbType.String, size: 20);
                p.Add("@StockMinimo", req.StockMinimo, DbType.Decimal, precision: 12, scale: 2);
                p.Add("@CantidadInicial", req.CantidadInicial, DbType.Decimal, precision: 12, scale: 2);
                p.Add("@PrecioCompra", req.PrecioCompra, DbType.Decimal, precision: 10, scale: 2);
                p.Add("@IdUsuario", idUsuario, DbType.Int32);
                p.Add("@Imagen", req.Imagen, DbType.String, size: 255);
                p.Add("@Descripcion", req.Descripcion, DbType.String, size: 200);
                p.Add("@Activo", true, DbType.Boolean);

                var cmd = new CommandDefinition(
                    "api.usp_Materiales_Insertar",
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

        /// <summary>
        /// Listars the asynchronous.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public async Task<IEnumerable<Material>> ListarAsync(
            ListarMaterialesQuery query,
            CancellationToken ct = default)
        {
            await using var db = factory.Crear();
            await db.OpenAsync(ct);

            try
            {
                var p = new DynamicParameters();
                p.Add("@Busqueda", query.Buscar, DbType.String, size: 50);
                p.Add("@Activo", query.Activo, DbType.Boolean);

                var cmd = new CommandDefinition(
                    "api.usp_Materiales_Listar",
                    p,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: ct);

                return await db.QueryAsync<Material>(cmd);
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
        public async Task<Material?> ObtenerPorIdAsync(
            int id,
            CancellationToken ct = default)
        {
            await using var db = factory.Crear();
            await db.OpenAsync(ct);

            try
            {
                var cmd = new CommandDefinition(
                    "api.usp_Materiales_ObtenerPorId",
                    new { IdMaterial = id },
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: ct);

                return await db.QuerySingleOrDefaultAsync<Material>(cmd);
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
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="ct">The ct.</param>
        public async Task ModificarAsync(
            int id,
            MaterialRequest req,
            int idUsuario,
            CancellationToken ct = default)
        {
            await using var db = factory.Crear();
            await db.OpenAsync(ct);

            try
            {
                var p = new DynamicParameters();

                p.Add("@IdMaterial", id, DbType.Int32);
                p.Add("@Codigo", req.Codigo, DbType.String, size: 30);
                p.Add("@Nombre", req.Nombre, DbType.String, size: 50);
                p.Add("@UnidadMedida", req.UnidadMedida, DbType.String, size: 20);

                p.Add("@StockMinimo", req.StockMinimo,
                    DbType.Decimal, precision: 12, scale: 2);

                p.Add("@PrecioCompra", req.PrecioCompra,
                    DbType.Decimal, precision: 10, scale: 2);

                p.Add("@Imagen", req.Imagen, DbType.String, size: 255);
                p.Add("@Descripcion", req.Descripcion, DbType.String, size: 200);

                // No cambiar el estado desde esta operación.
                p.Add("@Activo", dbType: DbType.Boolean, value: null);

                // Stock actual y stock original al abrir el formulario.
                p.Add("@StockActual", req.StockActual,
                    DbType.Decimal, precision: 12, scale: 2);

                p.Add("@StockEsperado", req.StockEsperado,
                    DbType.Decimal, precision: 12, scale: 2);

                p.Add("@Motivo", req.Motivo, DbType.String, size: 200);
                p.Add("@IdUsuario", idUsuario, DbType.Int32);

                var cmd = new CommandDefinition(
                    "api.usp_Materiales_Modificar",
                    p,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: ct);

                await db.ExecuteAsync(cmd);
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
        public async Task CambiarEstadoAsync(
            int id,
            bool activo,
            CancellationToken ct = default)
        {
            await using var db = factory.Crear();
            await db.OpenAsync(ct);

            try
            {
                var cmd = new CommandDefinition(
                    "api.usp_Materiales_CambiarEstado",
                    new { IdMaterial = id, Activo = activo },
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: ct);

                await db.ExecuteAsync(cmd);
            }
            catch (SqlException ex) when (SqlErrorTranslator.EsTraducible(ex))
            {
                throw SqlErrorTranslator.Traducir(ex);
            }
        }
    }
}
