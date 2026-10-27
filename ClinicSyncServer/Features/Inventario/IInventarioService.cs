using ClinicSyncServer.Features.Inventario.Requests;

namespace ClinicSyncServer.Features.Inventario
{
    public interface IInventarioService
    {
        Task<int> InsertarAsync(RegistrarMaterialRequest req, int idUsuario, CancellationToken ct = default);
        Task<IEnumerable<Material>> ListarAsync(ListarMaterialesQuery query, CancellationToken ct = default);
        Task<Material?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
        Task<Material?> ModificarAsync(int id, MaterialRequest req, CancellationToken ct = default);
        Task<bool> CambiarEstadoAsync(int id, bool activo, CancellationToken ct = default);
    }
}
