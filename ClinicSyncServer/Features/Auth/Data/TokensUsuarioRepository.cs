using Microsoft.Data.SqlClient;
using System.Data;
using Dapper;
using ClinicSyncServer.Common.Data;

namespace ClinicSyncServer.Features.Auth.Data
{
    /// <summary>
    /// 
    /// </summary>
    public class TokensUsuarioRepository(SqlConnectionFactory factory)
    {
        /// <summary>
        /// Insertars the asynchronous.
        /// </summary>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="tokenHash">The token hash.</param>
        /// <param name="proposito">The proposito.</param>
        /// <param name="expira">The expira.</param>
        public async Task InsertarAsync(int idUsuario, byte[] tokenHash, string proposito, DateTime expira)
        {
            await using SqlConnection db = factory.Crear();
            await db.OpenAsync();
            await db.ExecuteAsync("api.usp_TokensUsuario_Insertar",
                new { IdUsuario = idUsuario, TokenHash = tokenHash, Proposito = proposito, FechaExpiracion = expira },
                commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Anulars the pendientes asynchronous.
        /// </summary>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="proposito">The proposito.</param>
        public async Task AnularPendientesAsync(int idUsuario, string proposito)
        {
            await using var db = factory.Crear();
            await db.OpenAsync();
            await db.ExecuteAsync(
                "UPDATE dbo.TokensUsuario SET FechaUso = SYSUTCDATETIME() WHERE IdUsuario = @IdUsuario AND Proposito = @Proposito AND FechaUso IS NULL",
                new { IdUsuario = idUsuario, Proposito = proposito });
        }

        /// <summary>
        /// Existes the token hash asynchronous.
        /// </summary>
        /// <param name="tokenHash">The token hash.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public async Task<bool> ExisteTokenHashAsync(byte[] tokenHash, CancellationToken ct = default)
        {
            await using SqlConnection db = factory.Crear();
            await db.OpenAsync(ct);

            var count = await db.ExecuteScalarAsync<int>(
                new CommandDefinition(
                    "SELECT COUNT(1) FROM dbo.TokensUsuario WHERE TokenHash = @TokenHash",
                    new { TokenHash = tokenHash },
                    cancellationToken: ct));

            return count > 0;
        }

        /// <summary>
        /// Validars the token asynchronous.
        /// </summary>
        /// <param name="tokenHash">The token hash.</param>
        /// <param name="proposito">The proposito.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public async Task<bool> ValidarTokenAsync(byte[] tokenHash, string proposito, CancellationToken ct = default)
        {
            await using SqlConnection db = factory.Crear();
            await db.OpenAsync(ct);

            var count = await db.ExecuteScalarAsync<int>(
                new CommandDefinition(
                    @"SELECT COUNT(1) 
                      FROM dbo.TokensUsuario 
                      WHERE TokenHash = @TokenHash 
                        AND Proposito = @Proposito 
                        AND FechaUso IS NULL 
                        AND FechaExpiracion > SYSUTCDATETIME()",
                    new { TokenHash = tokenHash, Proposito = proposito },
                    cancellationToken: ct));

            return count > 0;
        }
    }
}
