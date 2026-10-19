using ClinicSyncServer.Features.Usuarios.Requests;
using ClinicSyncServer.Features.Usuarios.Responses;

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
    }
}
