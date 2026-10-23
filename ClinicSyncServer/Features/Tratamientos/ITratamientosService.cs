using ClinicSyncServer.Features.Tratamientos.Projections;
using ClinicSyncServer.Features.Tratamientos.Requests;

namespace ClinicSyncServer.Features.Tratamientos
{
    public interface ITratamientosService
    {
        Task<int> InsertarAsync(RegistrarTratamientoRequest req);
        Task<Tratamiento?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<Tratamiento>> ListarAsync(ListarTratamientoQuery query);
        Task<Tratamiento?> ModificarAsync(int id, RegistrarTratamientoRequest req);
        Task CambiarEstadoAsync(int id, bool activo);
    }
}
