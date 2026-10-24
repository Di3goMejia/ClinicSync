
using ClinicSyncServer.Features.Tratamientos.Responses;

namespace ClinicSyncServer.Features.Tratamientos
{
    public interface ITratamientosService
    {
        Task<IEnumerable<TratamientoListado>> ListarAsync(
            CancellationToken ct = default);

        Task<int> RegistrarAsync(
            string nombre,
            decimal precio,
            string? descripcion = null,
            CancellationToken ct = default);
    }
}
