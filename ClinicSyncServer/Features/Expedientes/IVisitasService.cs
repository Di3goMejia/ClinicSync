using ClinicSyncServer.Features.Expedientes.Requests;
using ClinicSyncServer.Features.Expedientes.Responses;

namespace ClinicSyncServer.Features.Expedientes
{
    public interface IVisitasService
    {
        Task<IEnumerable<VisitaListado>> ListarAsync(int idPaciente, 
            CancellationToken ct = default);

        Task<VisitaListado?> ObtenerPorIdAsync(int idVisita,
            CancellationToken ct = default);

        Task<IEnumerable<TratamientoVisitaListado>> ListarTratamientosAsync(
            int idVisita, CancellationToken ct = default);

        Task<int> InsertarAsync(CrearVisitaRequest request,
            CancellationToken ct = default);

        Task ModificarAsync(int idVisita, ModificarVisitaRequest request, 
            CancellationToken ct = default);

        Task CambiarEstadoAsync(int idVisita, bool activo,
            CancellationToken ct = default);

        Task<int> InsertarCompletaAsync(CrearVisitaCompletaRequest request,
            CancellationToken ct = default);

        Task ModificarCompletaAsync(int idVisita,
            ModificarVisitaCompletaRequest request,
            CancellationToken ct = default);
    }
}
