using ClinicSyncServer.Features.Inventario.Requests;

namespace ClinicSyncServer.Features.Inventario
{
    /// <summary>
    /// 
    /// </summary>
    public interface IInventarioService
    {
        /// <summary>
        /// Insertars the asynchronous.
        /// </summary>
        /// <param name="req">The req.</param>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<int> InsertarAsync(RegistrarMaterialRequest req,int idUsuario,CancellationToken ct = default);

        /// <summary>
        /// Listars the asynchronous.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<IEnumerable<Material>> ListarAsync(ListarMaterialesQuery query,CancellationToken ct = default);

        /// <summary>
        /// Obteners the por identifier asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<Material?> ObtenerPorIdAsync(int id,CancellationToken ct = default);

        /// <summary>
        /// Modificars the asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="req">The req.</param>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<Material?> ModificarAsync(int id,MaterialRequest req,int idUsuario,CancellationToken ct = default);

        /// <summary>
        /// Cambiars the estado asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="activo">if set to <c>true</c> [activo].</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<bool> CambiarEstadoAsync(int id,bool activo,CancellationToken ct = default);
    }
}
