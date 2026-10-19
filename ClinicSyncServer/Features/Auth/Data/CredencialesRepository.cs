using ClinicSyncServer.Common.Data;
using ClinicSyncServer.Features.Auth.Projections;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ClinicSyncServer.Features.Auth.Data
{
    public class CredencialesRepository(SqlConnectionFactory factory)
    {
        public async Task<UsuarioAuth?> ObtenerParaLoginAsync(string identificador)
        {
            await using SqlConnection db = factory.Crear();
            await db.OpenAsync();

            return await db.QuerySingleOrDefaultAsync<UsuarioAuth>(
                "api.usp_Usuarios_ObtenerParaLogin",
                new { Identificador = identificador },
                commandType: CommandType.StoredProcedure);
        }

        public async Task RegistrarLoginExitosoAsync(int idUsuario)
        {
            await using SqlConnection db = factory.Crear();
            await db.OpenAsync();

            await db.ExecuteAsync("api.usp_Usuarios_RegistrarLoginExitoso",
                new { IdUsuario = idUsuario },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<UsuarioContacto?> ObtenerContactoAsync(string identificador)
        {
            await using SqlConnection db = factory.Crear();
            await db.OpenAsync();

            return await db.QuerySingleOrDefaultAsync<UsuarioContacto>(
                "api.usp_Usuarios_ContactoPorIdentificador",
                new { Identificador = identificador },
                commandType: CommandType.StoredProcedure);
        }

        public async Task EstablecerPasswordAsync(byte[] tokenHash, string proposito, string passwordHash)
        {
            await using SqlConnection db = factory.Crear();
            await db.OpenAsync();

            try
            {
                await db.ExecuteAsync("api.usp_Usuarios_EstablecerPassword",
                    new { TokenHash = tokenHash, Proposito = proposito, PasswordHash = passwordHash },
                    commandType: CommandType.StoredProcedure);
            }
            catch (SqlException ex) when (SqlErrorTranslator.EsTraducible(ex))
            {
                // Traduce el 50011 (enlace inválido o expirado) a ReglaNegocioException
                throw SqlErrorTranslator.Traducir(ex);
            }
        }
    }
}