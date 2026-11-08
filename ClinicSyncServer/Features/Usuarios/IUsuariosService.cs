using ClinicSyncServer.Features.Usuarios.Projections;
using ClinicSyncServer.Features.Usuarios.Requests;
using ClinicSyncServer.Features.Usuarios.Responses;
using Microsoft.Data.SqlClient;
using System.Data;

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
        Task ModificarAsync(ModificarUsuarioRequest req, CancellationToken ct = default);
    }
}
