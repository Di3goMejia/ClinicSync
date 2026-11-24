using ClinicSyncServer.Features.Agenda.Requests;
using ClinicSyncServer.Features.Agenda.Responses;

namespace ClinicSyncServer.Features.Agenda
{
    /// <summary>
    /// 
    /// </summary>
    public interface IAgendaService
    {
        /// <summary>
        /// Listars the asynchronous.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<IEnumerable<EventoAgendaResponse>> ListarAsync(
            ListarAgendaQuery query,
            CancellationToken ct = default);

        /// <summary>
        /// Proximases the asynchronous.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<IEnumerable<ProximaCitaResponse>> ProximasAsync(
            ProximasCitasQuery query,
            CancellationToken ct = default);

        /// <summary>
        /// Obteners the por identifier asynchronous.
        /// </summary>
        /// <param name="idEvento">The identifier evento.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<EventoAgendaResponse?> ObtenerPorIdAsync(
            int idEvento,
            CancellationToken ct = default);

        /// <summary>
        /// Verificars the disponibilidad asynchronous.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<DisponibilidadResponse> VerificarDisponibilidadAsync(
            DisponibilidadQuery query,
            int idUsuario,
            CancellationToken ct = default);

        // MODIFICAR UNA CITA
        /// <summary>
        /// Modificars the asynchronous.
        /// </summary>
        /// <param name="idEvento">The identifier evento.</param>
        /// <param name="req">The req.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task ModificarAsync(
            int idEvento,
            CitaRequest req,
            CancellationToken ct = default);

        // CANCELAR UNA CITA
        /// <summary>
        /// Cancelars the asynchronous.
        /// </summary>
        /// <param name="idEvento">The identifier evento.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task CancelarAsync(
            int idEvento,
            CancellationToken ct = default);

        /// <summary>
        /// Insertars the asynchronous.
        /// </summary>
        /// <param name="req">The req.</param>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<int> InsertarAsync(
            CitaRequest req,
            int idUsuario,
            CancellationToken ct = default);

    }
}