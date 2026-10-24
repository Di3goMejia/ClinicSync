using ClinicSyncServer.Common.Data;
using ClinicSyncServer.Features.Tratamientos.Responses;
using Dapper;
using System.Data;

namespace ClinicSyncServer.Features.Tratamientos.Data
{
    public class TratamientosRepository(SqlConnectionFactory factory)
    {
        public async Task<IEnumerable<TratamientoListado>> ListarAsync(
            CancellationToken ct = default)
        {
            await using var db = factory.Crear();
            await db.OpenAsync(ct);

            var parametros = new CommandDefinition(
                commandText: "api.usp_Tratamientos_Listar",
                commandType: CommandType.StoredProcedure,
                cancellationToken: ct
            );

            return await db.QueryAsync<TratamientoListado>(parametros);
        }

        public async Task<int> RegistrarAsync(
            string nombre,
            decimal precio,
            string? descripcion = null,
            CancellationToken ct = default)
        {
            await using var db = factory.Crear();
            await db.OpenAsync(ct);

            var parametros = new DynamicParameters();

            parametros.Add(
                "@Nombre",
                nombre,
                DbType.String,
                size: 100);

            parametros.Add(
                "@Precio",
                precio,
                DbType.Decimal,
                precision: 10,
                scale: 2);

            parametros.Add(
                "@Descripcion",
                descripcion,
                DbType.String,
                size: 200);

            var comando = new CommandDefinition(
                commandText: "api.usp_Tratamientos_Insertar",
                parameters: parametros,
                commandType: CommandType.StoredProcedure,
                cancellationToken: ct
            );

            return await db.QuerySingleAsync<int>(comando);
        }
    }
}
