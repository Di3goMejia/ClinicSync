using ClinicSyncServer.Features.Permisos.Projections;

namespace ClinicSyncServer.Features.Permisos
{
    public interface IPermisosService
    {
        Task<PermisosUsuario?> ObtenerPorUsuarioAsync(int idUsuario, CancellationToken ct = default);
    }
}
