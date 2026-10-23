using ClinicSyncServer.Features.Tratamientos.Data;
using ClinicSyncServer.Features.Tratamientos.Projections;
using ClinicSyncServer.Features.Tratamientos.Requests;

namespace ClinicSyncServer.Features.Tratamientos
{
    public class TratamientosService(TratamientosRepository repo) : ITratamientosService
    {
        public async Task<int> InsertarAsync(RegistrarTratamientoRequest req) =>
            await repo.InsertarAsync(req);

        public async Task<Tratamiento?> ObtenerPorIdAsync(int id) =>
            await repo.ObtenerPorIdAsync(id);

        public async Task<IEnumerable<Tratamiento>> ListarAsync(ListarTratamientoQuery query) =>
            await repo.ListarAsync(query);

        public async Task<Tratamiento?> ModificarAsync(int id, RegistrarTratamientoRequest req)
        {
            if (await repo.ObtenerPorIdAsync(id) is null) return null;

            await repo.ModificarAsync(id, req);
            return await repo.ObtenerPorIdAsync(id);
        }

        public async Task CambiarEstadoAsync(int id, bool activo) => 
            await repo.CambiarEstadoAsync(id, activo);
    }
}
