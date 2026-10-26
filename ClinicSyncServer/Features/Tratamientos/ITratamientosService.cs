using ClinicSyncServer.Features.Tratamientos.Requests;

namespace ClinicSyncServer.Features.Tratamientos
{
    public interface ITratamientosService
    {
        Task<int> InsertarAsync(RegistrarTratamientoRequest req, CancellationToken ct = default);
        Task<Tratamiento?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
        Task<IEnumerable<Tratamiento>> ListarAsync(ListarTratamientoQuery query, CancellationToken ct = default);
        Task<Tratamiento?> ModificarAsync(int id, RegistrarTratamientoRequest req, CancellationToken ct = default);
        Task CambiarEstadoAsync(int id, bool activo, CancellationToken ct = default);
    }
}
