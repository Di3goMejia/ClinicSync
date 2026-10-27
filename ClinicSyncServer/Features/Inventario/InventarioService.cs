using ClinicSyncServer.Features.Inventario.Data;
using ClinicSyncServer.Features.Inventario.Requests;

namespace ClinicSyncServer.Features.Inventario
{
    public class InventarioService(InventarioRepository repo)
        : IInventarioService
    {
        public Task<int> InsertarAsync(
            RegistrarMaterialRequest req,
            int idUsuario,
            CancellationToken ct = default)
        {
            return repo.InsertarAsync(req, idUsuario, ct);
        }

        public Task<IEnumerable<Material>> ListarAsync(
            ListarMaterialesQuery query,
            CancellationToken ct = default)
        {
            return repo.ListarAsync(query, ct);
        }

        public Task<Material?> ObtenerPorIdAsync(
            int id,
            CancellationToken ct = default)
        {
            return repo.ObtenerPorIdAsync(id, ct);
        }

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
