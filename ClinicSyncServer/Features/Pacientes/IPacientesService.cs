
using ClinicSyncServer.Features.Pacientes.Responses;

namespace ClinicSyncServer.Features.Pacientes
{
    public interface IPacientesService
    {
        Task<IEnumerable<PacienteListado>> ListarAsync(
            CancellationToken ct = default);

        Task InsertarAsync(
            RegistrarPacienteRequest paciente,
            CancellationToken ct = default);

        Task ModificarAsync(
            int idPaciente,
            ModificarPacienteRequest paciente,
            CancellationToken ct = default);

        Task CambiarEstadoAsync(
            int idPaciente,
            bool activo,
            CancellationToken ct = default);
    }
}
