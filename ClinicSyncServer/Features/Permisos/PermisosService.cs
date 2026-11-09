using ClinicSyncServer.Features.Permisos.Data;
using ClinicSyncServer.Features.Permisos.Projections;

namespace ClinicSyncServer.Features.Permisos
{
    public class PermisosService(PermisosRepository permisos) : IPermisosService
    {
        public Task<PermisosUsuario?> ObtenerPorUsuarioAsync(int idUsuario, CancellationToken ct = default) =>
            permisos.ObtenerPorUsuarioAsync(idUsuario);

        public Task<IReadOnlyList<PermisoCatalogo>> ListarAsync(CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<PermisoCatalogo>>(
                Common.Seguridad.Permisos.Todos.Select(
                    codigo => new PermisoCatalogo(codigo, codigo.Split('_')[0], codigo)).ToList());
    }
}
