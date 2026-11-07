using ClinicSyncServer.Features.Agenda.Requests;
using ClinicSyncServer.Features.Agenda.Responses;

namespace ClinicSyncServer.Features.Agenda
{
    public interface IAgendaService
    {
        Task<IEnumerable<EventoAgendaResponse>> ListarAsync(
            ListarAgendaQuery query,
            CancellationToken ct = default);

        Task<IEnumerable<ProximaCitaResponse>> ProximasAsync(
            ProximasCitasQuery query,
            CancellationToken ct = default);

        Task<EventoAgendaResponse?> ObtenerPorIdAsync(
            int idEvento,
            CancellationToken ct = default);

        Task<DisponibilidadResponse> VerificarDisponibilidadAsync(
            DisponibilidadQuery query,
            int idUsuario,
            CancellationToken ct = default);

        // MODIFICAR UNA CITA
        Task ModificarAsync(
            int idEvento,
            CitaRequest req,
            CancellationToken ct = default);

        // CANCELAR UNA CITA
        Task CancelarAsync(
            int idEvento,
            CancellationToken ct = default);

        Task<int> InsertarAsync(
            CitaRequest req,
            int idUsuario,
            CancellationToken ct = default);

    }
}