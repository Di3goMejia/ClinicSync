using Microsoft.Data.SqlClient;
using System.Data;
using Dapper;
using ClinicSyncServer.Common.Data;
using ClinicSyncServer.Features.Sesiones.Projections;

namespace ClinicSyncServer.Features.Sesiones.Data
{
    public class SesionesRepository(SqlConnectionFactory factory)
    {
        public async Task<Guid> CrearAsync(int idUsuario, byte[] hashRefresh,
            string equipo, string? ip, DateTime expira)
        {
            await using SqlConnection db = factory.Crear();
            await db.OpenAsync();

            return await db.ExecuteScalarAsync<Guid>("api.usp_Sesiones_Crear",
                new
                {
                    IdUsuario = idUsuario,
                    Token = hashRefresh,
                    Equipo = equipo,
                    DireccionIp = ip,
                    Expira = expira
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<SesionRotada?> RotarAsync(byte[] hashActual, byte[] hashNuevo)
        {
            await using SqlConnection db = factory.Crear();
            await db.OpenAsync();
            return await db.QuerySingleOrDefaultAsync<SesionRotada>("api.usp_Sesiones_Rotar",
                new { TokenActual = hashActual, TokenNuevo = hashNuevo },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<bool> EsActivaAsync(Guid idSesion, int idUsuario)
        {
            await using SqlConnection db = factory.Crear();
            await db.OpenAsync();

            return await db.ExecuteScalarAsync<bool>("api.usp_Sesiones_EsActiva",
                new { IdSesion = idSesion, IdUsuario = idUsuario },
                commandType: CommandType.StoredProcedure);
        }

        public async Task CerrarAsync(Guid idSesion, string motivo)
        {
            await using SqlConnection db = factory.Crear();
            await db.OpenAsync();
            await db.ExecuteAsync("api.usp_Sesiones_Cerrar",
                new { IdSesion = idSesion, Motivo = motivo },
                commandType: CommandType.StoredProcedure);
        }
    }
}
