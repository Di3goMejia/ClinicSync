using ClinicSyncServer.Common.Data;
using ClinicSyncServer.Features.Auth.Projections;
using ClinicSyncServer.Features.Usuarios.Projections;
using ClinicSyncServer.Features.Usuarios.Requests;
using ClinicSyncServer.Features.Usuarios.Responses;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ClinicSyncServer.Features.Usuarios.Data
{
    public class UsuariosRepository(SqlConnectionFactory factory)
    {
        public async Task<UsuarioAuth?> ObtenerParaLoginAsync(string identificador)
        {
            await using var db = factory.Crear();
            await db.OpenAsync();

            return await db.QuerySingleOrDefaultAsync<UsuarioAuth>(
                "api.usp_Usuarios_ObtenerParaLogin",
                new { Identificador = identificador },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task RegistrarLoginExitosoAsync(int idUsuario)
        {
            await using var db = factory.Crear();
            await db.OpenAsync();

            await db.ExecuteAsync(
                "api.usp_Usuarios_RegistrarLoginExitoso",
                new { IdUsuario = idUsuario},
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task EstablecerPasswordAsync(byte[] tokenHash, string tipo, string clave)
        {
            await using SqlConnection db = factory.Crear();
            await db.OpenAsync();

            await db.ExecuteAsync("api.usp_Usuarios_EstablecerPassword",
                new { TokenHash = tokenHash, TipoToken = tipo, Clave = clave },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<CrearUsuarioResponse> CrearConEmpleadoAsync(CrearUsuarioRequest req, byte[] tokenHash, DateTime expira)
        {
            await using var db = factory.Crear();
            await db.OpenAsync();

            try
            {
                return await db.QuerySingleAsync<CrearUsuarioResponse>(
                    "api.usp_Usuarios_Crear",
                    new
                    {
                        req.Identidad,
                        req.Nombres,
                        req.Apellidos,
                        req.FechaNacimiento,
                        req.Genero,
                        req.Telefono,
                        req.IdRol,
                        req.NombreUsuario,
                        req.CorreoLogin,
                        req.CorreoContacto,
                        TokenHash = tokenHash,
                        Expira = expira
                    },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (SqlException ex) when (SqlErrorTranslator.EsTraducible(ex))
            {
                throw SqlErrorTranslator.Traducir(ex);
            }
        }

        public async Task<IEnumerable<UsuarioListado>> ListarAsync()
        {
            await using SqlConnection db = factory.Crear();
            await db.OpenAsync();

            return await db.QueryAsync<UsuarioListado>(
                "api.usp_Usuarios_Listar",
                commandType: CommandType.StoredProcedure);
        }

        public async Task<UsuarioDetalle?> ObtenerPorIdAsync(int id)
        {
            await using SqlConnection db = factory.Crear();
            await db.OpenAsync();

            return await db.QuerySingleOrDefaultAsync<UsuarioDetalle>(
                "api.usp_Usuarios_ObtenerPorId",
                new { Id = id },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task ModificarAsync(ModificarUsuarioRequest req)
        {
            await using SqlConnection db = factory.Crear();
            await db.OpenAsync();

            try
            {
                await db.ExecuteAsync(
                    "api.usp_Usuarios_Modificar",
                    req,
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
