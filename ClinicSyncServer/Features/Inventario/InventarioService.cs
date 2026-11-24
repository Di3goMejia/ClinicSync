using ClinicSyncServer.Features.Inventario.Data;
using ClinicSyncServer.Features.Inventario.Requests;

namespace ClinicSyncServer.Features.Inventario
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="ClinicSyncServer.Features.Inventario.IInventarioService" />
    public class InventarioService(InventarioRepository repo)
        : IInventarioService
    {
        /// <summary>
        /// Insertars the asynchronous.
        /// </summary>
        /// <param name="req">The req.</param>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public Task<int> InsertarAsync(
            RegistrarMaterialRequest req,
            int idUsuario,
            CancellationToken ct = default)
        {
            return repo.InsertarAsync(req, idUsuario, ct);
        }

        /// <summary>
        /// Listars the asynchronous.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public Task<IEnumerable<Material>> ListarAsync(
            ListarMaterialesQuery query,
            CancellationToken ct = default)
        {
            return repo.ListarAsync(query, ct);
        }

        /// <summary>
        /// Obteners the por identifier asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public Task<Material?> ObtenerPorIdAsync(
            int id,
            CancellationToken ct = default)
        {
            return repo.ObtenerPorIdAsync(id, ct);
        }

        /// <summary>
        /// Modificars the asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="req">The req.</param>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public async Task<Material?> ModificarAsync(
            int id,
            MaterialRequest req,
            int idUsuario,
            CancellationToken ct = default)
        {
            if (await repo.ObtenerPorIdAsync(id, ct) is null)
            {
                return null;
            }

            await repo.ModificarAsync(id, req, idUsuario, ct);

            return await repo.ObtenerPorIdAsync(id, ct);
        }

        /// <summary>
        /// Cambiars the estado asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="activo">if set to <c>true</c> [activo].</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public async Task<bool> CambiarEstadoAsync(
            int id,
            bool activo,
            CancellationToken ct = default)
        {
            if (await repo.ObtenerPorIdAsync(id, ct) is null)
            {
                return false;
            }

            await repo.CambiarEstadoAsync(id, activo, ct);

            return true;
        }
    }
}
