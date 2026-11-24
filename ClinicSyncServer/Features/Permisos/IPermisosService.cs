using ClinicSyncServer.Features.Permisos.Projections;

namespace ClinicSyncServer.Features.Permisos
{
    /// <summary>
    /// 
    /// </summary>
    public interface IPermisosService
    {
        /// <summary>
        /// Obteners the por usuario asynchronous.
        /// </summary>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<PermisosUsuario?> ObtenerPorUsuarioAsync(int idUsuario, CancellationToken ct = default);
        /// <summary>
        /// Listars the asynchronous.
        /// </summary>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<IEnumerable<PermisoCatalogo>> ListarAsync(CancellationToken ct = default);
    }
}
