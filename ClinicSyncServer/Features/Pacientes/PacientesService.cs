
using ClinicSyncServer.Features.Pacientes.Data;
using ClinicSyncServer.Features.Pacientes.Responses;

namespace ClinicSyncServer.Features.Pacientes
{
    public sealed class PacientesService : IPacientesService
    {
        private readonly PacientesRepository repo;

        public PacientesService(PacientesRepository repo)
        {
            this.repo = repo;
        }

        public Task<IEnumerable<PacienteListado>> ListarAsync(
            CancellationToken ct = default)
        {
            return repo.ListarAsync(ct);
        }

        public Task InsertarAsync(
            RegistrarPacienteRequest paciente,
            CancellationToken ct = default)
        {
            return repo.InsertarAsync(paciente, ct);
        }

        public Task ModificarAsync(
            int idPaciente,
            ModificarPacienteRequest paciente,
            CancellationToken ct = default)
        {
            return repo.ModificarAsync(
                idPaciente, paciente, ct);
        }

        public Task CambiarEstadoAsync(
            int idPaciente,
            bool activo,
            CancellationToken ct = default)
        {
            return repo.CambiarEstadoAsync(
                idPaciente, activo, ct);
        }
    }
}
