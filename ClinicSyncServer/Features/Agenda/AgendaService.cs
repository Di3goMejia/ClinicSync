using ClinicSyncServer.Common;
using ClinicSyncServer.Features.Agenda.Data;
using ClinicSyncServer.Features.Agenda.Requests;
using ClinicSyncServer.Features.Agenda.Responses;

namespace ClinicSyncServer.Features.Agenda
{
    public sealed class AgendaService(AgendaRepository repo) : IAgendaService
    {
        // El token trae el IdUsuario; la agenda trabaja con el IdEmpleado.
        private async Task<int> ObtenerIdEmpleadoAsync(
            int idUsuario,
            CancellationToken ct)
        {
            var idEmpleado = await repo.ObtenerIdEmpleadoAsync(idUsuario, ct);

            return idEmpleado
                ?? throw new NoAutorizadoException(
                    "El usuario no está activo o no tiene un empleado asociado.");
        }

        public Task<IEnumerable<EventoAgendaResponse>> ListarAsync(
            ListarAgendaQuery query,
            CancellationToken ct = default)
        {
            return repo.ListarAsync(query, ct);
        }

        public Task<IEnumerable<ProximaCitaResponse>> ProximasAsync(
            ProximasCitasQuery query,
            CancellationToken ct = default)
        {
            return repo.ProximasAsync(query, ct);
        }

        public Task<EventoAgendaResponse?> ObtenerPorIdAsync(
            int idEvento,
            CancellationToken ct = default)
        {
            return repo.ObtenerPorIdAsync(idEvento, ct);
        }

        public async Task<DisponibilidadResponse> VerificarDisponibilidadAsync(
            DisponibilidadQuery query,
            int idUsuario,
            CancellationToken ct = default)
        {
            int idEmpleado = await ObtenerIdEmpleadoAsync(idUsuario, ct);

            return await repo.VerificarDisponibilidadAsync(query, idEmpleado, ct);
        }

        public async Task<int> InsertarAsync(
            CitaRequest req,
            int idUsuario,
            CancellationToken ct = default)
        {
            int idEmpleado = await ObtenerIdEmpleadoAsync(idUsuario, ct);

            return await repo.InsertarAsync(req, idEmpleado, ct);
        }

       
    }
}