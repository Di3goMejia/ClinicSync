using ClinicSyncServer.Features.Tratamientos.Requests;

namespace ClinicSyncServer.Features.Tratamientos
{
    /// <summary>
    /// 
    /// </summary>
    public interface ITratamientosService
    {
        /// <summary>
        /// Insertars the asynchronous.
        /// </summary>
        /// <param name="req">The req.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<int> InsertarAsync(RegistrarTratamientoRequest req, CancellationToken ct = default);
        /// <summary>
        /// Obteners the por identifier asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<Tratamiento?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
        /// <summary>
        /// Listars the asynchronous.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<IEnumerable<Tratamiento>> ListarAsync(ListarTratamientoQuery query, CancellationToken ct = default);
        /// <summary>
        /// Modificars the asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="req">The req.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<Tratamiento?> ModificarAsync(int id, RegistrarTratamientoRequest req, CancellationToken ct = default);
        /// <summary>
        /// Cambiars the estado asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="activo">if set to <c>true</c> [activo].</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task CambiarEstadoAsync(int id, bool activo, CancellationToken ct = default);
    }
}
