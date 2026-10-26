using ClinicSyncServer.Features.Tratamientos.Data;
using ClinicSyncServer.Features.Tratamientos.Requests;

namespace ClinicSyncServer.Features.Tratamientos
{
    public class TratamientoService(
        TratamientosRepository repo) : ITratamientosService
    {
        public async Task<int> InsertarAsync(RegistrarTratamientoRequest req, CancellationToken ct = default) =>
            await repo.InsertarAsync(req, ct);

        public async Task<Tratamiento?> ObtenerPorIdAsync(int id, CancellationToken ct = default) =>
            await repo.ObtenerPorIdAsync(id, ct);

        public async Task<IEnumerable<Tratamiento>> ListarAsync(ListarTratamientoQuery query, CancellationToken ct = default) =>
            await repo.ListarAsync(query, ct);

        public async Task<Tratamiento?> ModificarAsync(int id, RegistrarTratamientoRequest req, CancellationToken ct = default)
        {
            if (await repo.ObtenerPorIdAsync(id, ct) is null) return null;

            await repo.ModificarAsync(id, req, ct);
            return await repo.ObtenerPorIdAsync(id, ct);
        }

        public async Task CambiarEstadoAsync(int id, bool activo, CancellationToken ct = default) =>
            await repo.CambiarEstadoAsync(id, activo, ct);
    }
}
