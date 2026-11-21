using ClinicSyncServer.Features.Permisos.Data;
using ClinicSyncServer.Features.Permisos.Projections;

namespace ClinicSyncServer.Features.Permisos
{
    public class PermisosService(PermisosRepository permisos) : IPermisosService
    {
        public Task<PermisosUsuario?> ObtenerPorUsuarioAsync(int idUsuario, CancellationToken ct = default) =>
            permisos.ObtenerPorUsuarioAsync(idUsuario, ct);

        public Task<IEnumerable<PermisoCatalogo>> ListarAsync(CancellationToken ct = default) =>
            permisos.ListarAsync(ct);
    }
}
