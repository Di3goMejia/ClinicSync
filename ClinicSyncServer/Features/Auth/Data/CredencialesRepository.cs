using ClinicSyncServer.Common.Data;
using ClinicSyncServer.Features.Auth.Projections;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ClinicSyncServer.Features.Auth.Data
{
    /// <summary>
    /// 
    /// </summary>
    public class CredencialesRepository(SqlConnectionFactory factory)
    {
        /// <summary>
        /// Obteners the para login asynchronous.
        /// </summary>
        /// <param name="identificador">The identificador.</param>
        /// <returns></returns>
        public async Task<UsuarioAuth?> ObtenerParaLoginAsync(string identificador)
        {
            await using SqlConnection db = factory.Crear();
            await db.OpenAsync();

            return await db.QuerySingleOrDefaultAsync<UsuarioAuth>(
                "api.usp_Usuarios_ObtenerParaLogin",
                new { Identificador = identificador },
                commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Registrars the login exitoso asynchronous.
        /// </summary>
        /// <param name="idUsuario">The identifier usuario.</param>
        public async Task RegistrarLoginExitosoAsync(int idUsuario)
        {
            await using SqlConnection db = factory.Crear();
            await db.OpenAsync();

            await db.ExecuteAsync("api.usp_Usuarios_RegistrarLoginExitoso",
                new { IdUsuario = idUsuario },
                commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Obteners the contacto asynchronous.
        /// </summary>
        /// <param name="identificador">The identificador.</param>
        /// <returns></returns>
        public async Task<UsuarioContacto?> ObtenerContactoAsync(string identificador)
        {
            await using SqlConnection db = factory.Crear();
            await db.OpenAsync();

            return await db.QuerySingleOrDefaultAsync<UsuarioContacto>(
                "api.usp_Usuarios_ContactoPorIdentificador",
                new { Identificador = identificador },
                commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Establecers the password asynchronous.
        /// </summary>
        /// <param name="tokenHash">The token hash.</param>
        /// <param name="proposito">The proposito.</param>
        /// <param name="passwordHash">The password hash.</param>
        public async Task EstablecerPasswordAsync(byte[] tokenHash, string proposito, string passwordHash)
        {
            await using SqlConnection db = factory.Crear();
            await db.OpenAsync();

            try
            {
                await db.ExecuteAsync("api.usp_Usuarios_EstablecerPassword",
                    new
                    {
                        TokenHash = tokenHash,
                        Proposito = proposito,
                        ClaveHash = passwordHash
                    },
                    commandType: CommandType.StoredProcedure);
            }
            catch (SqlException ex) when (SqlErrorTranslator.EsTraducible(ex))
            {
                // Traduce el 50011 (enlace inválido o expirado) a ReglaNegocioException
                throw SqlErrorTranslator.Traducir(ex);
            }
        }

        // OBTENER HASH DE CONTRASEÑA DEL USUARIO
        /// <summary>
        /// Obteners the clave asynchronous.
        /// </summary>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <returns></returns>
        public async Task<string?> ObtenerClaveAsync(int idUsuario)
        {
            await using SqlConnection db = factory.Crear();
            await db.OpenAsync();

            return await db.QuerySingleOrDefaultAsync<string>(
                "api.usp_Usuarios_ObtenerClave",
                new { IdUsuario = idUsuario },
                commandType: CommandType.StoredProcedure);
        }

        // CAMBIAR CONTRASEÑA DESDE PERFIL
        /// <summary>
        /// Cambiars the mi password asynchronous.
        /// </summary>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="idSesionActual">The identifier sesion actual.</param>
        /// <param name="claveHash">The clave hash.</param>
        public async Task CambiarMiPasswordAsync(
            int idUsuario,
            Guid idSesionActual,
            string claveHash)
        {
            await using SqlConnection db = factory.Crear();
            await db.OpenAsync();

            await db.ExecuteAsync(
                "api.usp_Usuarios_CambiarMiPassword",
                new
                {
                    IdUsuario = idUsuario,
                    IdSesionActual = idSesionActual,
                    ClaveHash = claveHash
                },
                commandType: CommandType.StoredProcedure);
        }
    }
}