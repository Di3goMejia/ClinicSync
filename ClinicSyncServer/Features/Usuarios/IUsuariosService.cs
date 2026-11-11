using ClinicSyncServer.Common.Abstracciones;
using ClinicSyncServer.Features.Usuarios.Projections;
using ClinicSyncServer.Features.Usuarios.Requests;
using ClinicSyncServer.Features.Usuarios.Responses;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Http;

namespace ClinicSyncServer.Features.Usuarios
{
    public interface IUsuariosService
    {
        Task<CrearUsuarioResponse> CrearAsync(CrearUsuarioRequest req, CancellationToken ct = default);

        Task<UsuarioDetalleResponse?> ObtenerAsync(
            int idSolicitado,
            int idSolicitante,
            bool puedeVerTodos,
            CancellationToken ct = default);

        Task<IEnumerable<UsuarioListado>> ListarAsync();
        Task ModificarAsync(ModificarUsuarioRequest req, int idSolicitante, bool puedeEditarTodos, CancellationToken ct = default);
        Task ModificarRolAsync(ModificarRolUsuarioRequest req, int idSolicitante, CancellationToken ct = default);
        Task SubirImagenPerfilAsync(int idUsuario, IFormFile archivo, CancellationToken ct = default);
        Task<ContenidoAlmacenado?> ObtenerImagenPerfilAsync(int idUsuario, CancellationToken ct = default);
        Task ReenviarActivacionAsync(int idUsuario, CancellationToken ct = default);
        Task DesactivarAsync(int idUsuario, int idAdministrador, string password, CancellationToken ct = default);
        Task ReactivarAsync(int idUsuario, int idAdministrador, string password, CancellationToken ct = default);
        Task RestablecerPasswordAsync(int idUsuario, int idAdministrador, string password, CancellationToken ct = default);
        Task EliminarImagenPerfilAsync(int idUsuario, CancellationToken ct = default);
    }
}
