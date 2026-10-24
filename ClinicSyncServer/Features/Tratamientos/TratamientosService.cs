using ClinicSyncServer.Features.Tratamientos.Data;
using ClinicSyncServer.Features.Tratamientos.Responses;

namespace ClinicSyncServer.Features.Tratamientos
{
    public class TratamientoService(
        TratamientosRepository repository) : ITratamientosService
    {
        public Task<IEnumerable<TratamientoListado>> ListarAsync(
            CancellationToken ct = default)
        {
            return repository.ListarAsync(ct);
        }

        public Task<int> RegistrarAsync(
            string nombre,
            decimal precio,
            string? descripcion = null,
            CancellationToken ct = default)
        {
            return repository.RegistrarAsync(
                nombre,
                precio,
                descripcion,
                ct);
        }
    }
}
