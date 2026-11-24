using ClinicSyncServer.Features.Tratamientos.Data;
using ClinicSyncServer.Features.Tratamientos.Requests;

namespace ClinicSyncServer.Features.Tratamientos
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="ClinicSyncServer.Features.Tratamientos.ITratamientosService" />
    public class TratamientoService(
        TratamientosRepository repo) : ITratamientosService
    {
        /// <summary>
        /// Insertars the asynchronous.
        /// </summary>
        /// <param name="req">The req.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public async Task<int> InsertarAsync(RegistrarTratamientoRequest req, CancellationToken ct = default) =>
            await repo.InsertarAsync(req, ct);

        /// <summary>
        /// Obteners the por identifier asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public async Task<Tratamiento?> ObtenerPorIdAsync(int id, CancellationToken ct = default) =>
            await repo.ObtenerPorIdAsync(id, ct);

        /// <summary>
        /// Listars the asynchronous.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public async Task<IEnumerable<Tratamiento>> ListarAsync(ListarTratamientoQuery query, CancellationToken ct = default) =>
            await repo.ListarAsync(query, ct);

        /// <summary>
        /// Modificars the asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="req">The req.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public async Task<Tratamiento?> ModificarAsync(int id, RegistrarTratamientoRequest req, CancellationToken ct = default)
        {
            if (await repo.ObtenerPorIdAsync(id, ct) is null) return null;

            await repo.ModificarAsync(id, req, ct);
            return await repo.ObtenerPorIdAsync(id, ct);
        }

        /// <summary>
        /// Cambiars the estado asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="activo">if set to <c>true</c> [activo].</param>
        /// <param name="ct">The ct.</param>
        public async Task CambiarEstadoAsync(int id, bool activo, CancellationToken ct = default) =>
            await repo.CambiarEstadoAsync(id, activo, ct);
    }
}
