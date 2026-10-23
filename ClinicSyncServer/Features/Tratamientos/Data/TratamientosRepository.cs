using ClinicSyncServer.Common.Data;
using ClinicSyncServer.Features.Tratamientos.Projections;
using ClinicSyncServer.Features.Tratamientos.Requests;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ClinicSyncServer.Features.Tratamientos.Data
{
    public class TratamientosRepository(SqlConnectionFactory factory)
    {
        public async Task<int> InsertarAsync(RegistrarTratamientoRequest req)
        {
            await using SqlConnection db = factory.Crear();
            await db.OpenAsync();

            try
            {
                return await db.ExecuteScalarAsync<int>(
                    "api.usp_Tratamientos_Insertar",
                    req,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (SqlException ex) when (SqlErrorTranslator.EsTraducible(ex))
            {
                throw SqlErrorTranslator.Traducir(ex);
            }
        }

        public async Task<Tratamiento?> ObtenerPorIdAsync(int id)
        {
            await using SqlConnection db = factory.Crear();
            await db.OpenAsync();

            return await db.QuerySingleOrDefaultAsync<Tratamiento>(
                "api.usp_Tratamientos_ObtenerPorId",
                new { IdTratamiento = id },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<Tratamiento>> ListarAsync(ListarTratamientoQuery query)
        {
            await using SqlConnection db = factory.Crear();
            await db.OpenAsync();

            return await db.QueryAsync<Tratamiento>(
                "api.usp_Tratamientos_Listar",
                new { Nombre = query.Buscar, query.Activo },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task ModificarAsync(int id, RegistrarTratamientoRequest req)
        {
            await using SqlConnection db = factory.Crear();
            await db.OpenAsync();

            try
            {
                await db.ExecuteAsync(
                    "api.usp_Tratamientos_Modificar",
                    new { IdTratamiento = id, req.Nombre, req.Precio, req.Descripcion },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (SqlException ex) when (SqlErrorTranslator.EsTraducible(ex))
            {
                throw SqlErrorTranslator.Traducir(ex);
            }
        }
        
        public async Task CambiarEstadoAsync(int id, bool activo)
        {
            await using SqlConnection db = factory.Crear();
            await db.OpenAsync();

            try
            {
                await db.ExecuteAsync(
                    "api.usp_Tratamientos_CambiarEstado",
                    new { IdTratamiento = id, Activo = activo },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (SqlException ex) when (SqlErrorTranslator.EsTraducible(ex))
            {
                throw SqlErrorTranslator.Traducir(ex);
            }
        }
    }
}
