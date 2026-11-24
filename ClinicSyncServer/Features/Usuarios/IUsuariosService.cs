using ClinicSyncServer.Common.Abstracciones;
using ClinicSyncServer.Features.Usuarios.Projections;
using ClinicSyncServer.Features.Usuarios.Requests;
using ClinicSyncServer.Features.Usuarios.Responses;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Http;

namespace ClinicSyncServer.Features.Usuarios
{
    /// <summary>
    /// 
    /// </summary>
    public interface IUsuariosService
    {
        /// <summary>
        /// Crears the asynchronous.
        /// </summary>
        /// <param name="req">The req.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<CrearUsuarioResponse> CrearAsync(CrearUsuarioRequest req, CancellationToken ct = default);

        /// <summary>
        /// Obteners the asynchronous.
        /// </summary>
        /// <param name="idSolicitado">The identifier solicitado.</param>
        /// <param name="idSolicitante">The identifier solicitante.</param>
        /// <param name="puedeVerTodos">if set to <c>true</c> [puede ver todos].</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<UsuarioDetalleResponse?> ObtenerAsync(
            int idSolicitado,
            int idSolicitante,
            bool puedeVerTodos,
            CancellationToken ct = default);

        /// <summary>
        /// Listars the asynchronous.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<UsuarioListado>> ListarAsync();
        /// <summary>
        /// Modificars the asynchronous.
        /// </summary>
        /// <param name="req">The req.</param>
        /// <param name="idSolicitante">The identifier solicitante.</param>
        /// <param name="puedeEditarTodos">if set to <c>true</c> [puede editar todos].</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task ModificarAsync(ModificarUsuarioRequest req, int idSolicitante, bool puedeEditarTodos, CancellationToken ct = default);
        /// <summary>
        /// Modificars the rol asynchronous.
        /// </summary>
        /// <param name="req">The req.</param>
        /// <param name="idSolicitante">The identifier solicitante.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task ModificarRolAsync(ModificarRolUsuarioRequest req, int idSolicitante, CancellationToken ct = default);
        /// <summary>
        /// Subirs the imagen perfil asynchronous.
        /// </summary>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="archivo">The archivo.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task SubirImagenPerfilAsync(int idUsuario, IFormFile archivo, CancellationToken ct = default);
        /// <summary>
        /// Obteners the imagen perfil asynchronous.
        /// </summary>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<ContenidoAlmacenado?> ObtenerImagenPerfilAsync(int idUsuario, CancellationToken ct = default);
        /// <summary>
        /// Reenviars the activacion asynchronous.
        /// </summary>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task ReenviarActivacionAsync(int idUsuario, CancellationToken ct = default);
        /// <summary>
        /// Desactivars the asynchronous.
        /// </summary>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="idAdministrador">The identifier administrador.</param>
        /// <param name="password">The password.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task DesactivarAsync(int idUsuario, int idAdministrador, string password, CancellationToken ct = default);
        /// <summary>
        /// Reactivars the asynchronous.
        /// </summary>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="idAdministrador">The identifier administrador.</param>
        /// <param name="password">The password.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task ReactivarAsync(int idUsuario, int idAdministrador, string password, CancellationToken ct = default);
        /// <summary>
        /// Restablecers the password asynchronous.
        /// </summary>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="idAdministrador">The identifier administrador.</param>
        /// <param name="password">The password.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task RestablecerPasswordAsync(int idUsuario, int idAdministrador, string password, CancellationToken ct = default);
        /// <summary>
        /// Eliminars the imagen perfil asynchronous.
        /// </summary>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task EliminarImagenPerfilAsync(int idUsuario, CancellationToken ct = default);
    }
}
