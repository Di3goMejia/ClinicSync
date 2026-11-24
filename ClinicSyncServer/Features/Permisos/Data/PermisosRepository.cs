using Microsoft.Data.SqlClient;
using System.Data;
using Dapper;
using ClinicSyncServer.Common.Data;
using ClinicSyncServer.Features.Permisos.Projections;

namespace ClinicSyncServer.Features.Permisos.Data
{
    /// <summary>
    /// 
    /// </summary>
    public class PermisosRepository(SqlConnectionFactory factory)
    {
        /// <summary>
        /// Obteners the por usuario asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public async Task<PermisosUsuario?> ObtenerPorUsuarioAsync(int id, CancellationToken ct = default)
        {
            await using SqlConnection db = factory.Crear();
            await db.OpenAsync(ct);

            var filas = await db.QueryAsync<PermisoFila>(
                new CommandDefinition(
                    "api.usp_Permisos_ObtenerPorUsuario",
                    new { IdUsuario = id },
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: ct)
            );

            if (!filas.Any()) return null;

            var permisos = filas
                .Where(f => f.PermisoCodigo is not null)
                .Select(f => f.PermisoCodigo)
                .Distinct()
                .ToList();

            return new PermisosUsuario(filas.First().RolCodigo, permisos!);
        }

        /// <summary>
        /// Listars the asynchronous.
        /// </summary>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public async Task<IEnumerable<PermisoCatalogo>> ListarAsync(CancellationToken ct = default)
        {
            await using SqlConnection db = factory.Crear();
            await db.OpenAsync(ct);

            return await db.QueryAsync<PermisoCatalogo>(
                new CommandDefinition(
                    "api.usp_Permisos_Listar",
                    null,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: ct)
            );
        }
    }
}
