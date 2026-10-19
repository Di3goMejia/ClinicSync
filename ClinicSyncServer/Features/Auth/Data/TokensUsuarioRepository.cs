using Microsoft.Data.SqlClient;
using System.Data;
using Dapper;
using ClinicSyncServer.Common.Data;

namespace ClinicSyncServer.Features.Auth.Data
{
    public class TokensUsuarioRepository(SqlConnectionFactory factory)
    {
        public async Task InsertarAsync(int idUsuario, byte[] tokenHash, string proposito, DateTime expira)
        {
            await using SqlConnection db = factory.Crear();
            await db.OpenAsync();
            await db.ExecuteAsync("api.usp_TokensUsuario_Insertar",
                new { IdUsuario = idUsuario, TokenHash = tokenHash, Proposito = proposito, FechaExpiracion = expira },
                commandType: CommandType.StoredProcedure);
        }
    }
}
