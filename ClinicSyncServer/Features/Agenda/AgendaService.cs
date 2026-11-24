using ClinicSyncServer.Common;
using ClinicSyncServer.Features.Agenda.Data;
using ClinicSyncServer.Features.Agenda.Requests;
using ClinicSyncServer.Features.Agenda.Responses;

namespace ClinicSyncServer.Features.Agenda
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="ClinicSyncServer.Features.Agenda.IAgendaService" />
    public sealed class AgendaService(AgendaRepository repo) : IAgendaService
    {
        // El token trae el IdUsuario; la agenda trabaja con el IdEmpleado.
        /// <summary>
        /// Obteners the identifier empleado asynchronous.
        /// </summary>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        /// <exception cref="NoAutorizadoException">El usuario no está activo o no tiene un empleado asociado.</exception>
        private async Task<int> ObtenerIdEmpleadoAsync(
            int idUsuario,
            CancellationToken ct)
        {
            var idEmpleado = await repo.ObtenerIdEmpleadoAsync(idUsuario, ct);

            return idEmpleado
                ?? throw new NoAutorizadoException(
                    "El usuario no está activo o no tiene un empleado asociado.");
        }

        /// <summary>
        /// Listars the asynchronous.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public Task<IEnumerable<EventoAgendaResponse>> ListarAsync(
            ListarAgendaQuery query,
            CancellationToken ct = default)
        {
            return repo.ListarAsync(query, ct);
        }

        /// <summary>
        /// Proximases the asynchronous.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public Task<IEnumerable<ProximaCitaResponse>> ProximasAsync(
            ProximasCitasQuery query,
            CancellationToken ct = default)
        {
            return repo.ProximasAsync(query, ct);
        }

        /// <summary>
        /// Obteners the por identifier asynchronous.
        /// </summary>
        /// <param name="idEvento">The identifier evento.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public Task<EventoAgendaResponse?> ObtenerPorIdAsync(
            int idEvento,
            CancellationToken ct = default)
        {
            return repo.ObtenerPorIdAsync(idEvento, ct);
        }

        /// <summary>
        /// Verificars the disponibilidad asynchronous.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public async Task<DisponibilidadResponse> VerificarDisponibilidadAsync(
            DisponibilidadQuery query,
            int idUsuario,
            CancellationToken ct = default)
        {
            int idEmpleado = await ObtenerIdEmpleadoAsync(idUsuario, ct);

            return await repo.VerificarDisponibilidadAsync(query, idEmpleado, ct);
        }

        /// <summary>
        /// Insertars the asynchronous.
        /// </summary>
        /// <param name="req">The req.</param>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public async Task<int> InsertarAsync(
            CitaRequest req,
            int idUsuario,
            CancellationToken ct = default)
        {
            int idEmpleado = await ObtenerIdEmpleadoAsync(idUsuario, ct);

            return await repo.InsertarAsync(req, idEmpleado, ct);
        }

        // MODIFICAR UNA CITA
        /// <summary>
        /// Modificars the asynchronous.
        /// </summary>
        /// <param name="idEvento">The identifier evento.</param>
        /// <param name="req">The req.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public Task ModificarAsync(
            int idEvento,
            CitaRequest req,
            CancellationToken ct = default)
        {
            return repo.ModificarAsync(idEvento, req, ct);
        }

        // CANCELAR UNA CITA
        /// <summary>
        /// Cancelars the asynchronous.
        /// </summary>
        /// <param name="idEvento">The identifier evento.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public Task CancelarAsync(
            int idEvento,
            CancellationToken ct = default)
        {
            return repo.CancelarAsync(idEvento, ct);
        }


    }
}