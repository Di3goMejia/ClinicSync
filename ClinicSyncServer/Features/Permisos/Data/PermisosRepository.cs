using Microsoft.Data.SqlClient;
using System.Data;
using Dapper;
using ClinicSyncServer.Common.Data;
using ClinicSyncServer.Features.Permisos.Projections;

namespace ClinicSyncServer.Features.Permisos.Data
{
    public class PermisosRepository(SqlConnectionFactory factory)
    {
        public async Task<PermisosUsuario?> ObtenerPorUsuarioAsync(int id)
        {
            await using SqlConnection db = factory.Crear();
            await db.OpenAsync();

            var filas = (await db.QueryAsync<PermisoFila>(
                "api.usp_Permisos_ObtenerPorUsuario",
                new { IdUsuario = id },
                commandType: CommandType.StoredProcedure)
            ).ToList();

            if (filas.Count == 0) 
                return null;

            var permisos = filas
                .Where(f => f.PermisoCodigo is not null)
                .Select(f => f.PermisoCodigo)
                .Distinct()
                .ToList();

            return new PermisosUsuario(filas[0].RolCodigo, permisos!);
        }
    }
}
