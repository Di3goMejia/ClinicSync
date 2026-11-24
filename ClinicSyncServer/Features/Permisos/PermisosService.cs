using ClinicSyncServer.Features.Permisos.Data;
using ClinicSyncServer.Features.Permisos.Projections;

namespace ClinicSyncServer.Features.Permisos
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="ClinicSyncServer.Features.Permisos.IPermisosService" />
    public class PermisosService(PermisosRepository permisos) : IPermisosService
    {
        /// <summary>
        /// Obteners the por usuario asynchronous.
        /// </summary>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public Task<PermisosUsuario?> ObtenerPorUsuarioAsync(int idUsuario, CancellationToken ct = default) =>
            permisos.ObtenerPorUsuarioAsync(idUsuario, ct);

        /// <summary>
        /// Listars the asynchronous.
        /// </summary>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public Task<IEnumerable<PermisoCatalogo>> ListarAsync(CancellationToken ct = default) =>
            permisos.ListarAsync(ct);
    }
}
