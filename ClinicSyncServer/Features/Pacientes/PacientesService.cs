
using ClinicSyncServer.Features.Pacientes.Data;
using ClinicSyncServer.Features.Pacientes.Responses;

namespace ClinicSyncServer.Features.Pacientes
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="ClinicSyncServer.Features.Pacientes.IPacientesService" />
    public sealed class PacientesService : IPacientesService
    {
        /// <summary>
        /// The repo
        /// </summary>
        private readonly PacientesRepository repo;

        /// <summary>
        /// Initializes a new instance of the <see cref="PacientesService"/> class.
        /// </summary>
        /// <param name="repo">The repo.</param>
        public PacientesService(PacientesRepository repo)
        {
            this.repo = repo;
        }

        /// <summary>
        /// Listars the asynchronous.
        /// </summary>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public Task<IEnumerable<PacienteListado>> ListarAsync(
            CancellationToken ct = default)
        {
            return repo.ListarAsync(ct);
        }

        /// <summary>
        /// Insertars the asynchronous.
        /// </summary>
        /// <param name="paciente">The paciente.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public Task InsertarAsync(
            RegistrarPacienteRequest paciente,
            CancellationToken ct = default)
        {
            return repo.InsertarAsync(paciente, ct);
        }

        /// <summary>
        /// Modificars the asynchronous.
        /// </summary>
        /// <param name="idPaciente">The identifier paciente.</param>
        /// <param name="paciente">The paciente.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public Task ModificarAsync(
            int idPaciente,
            ModificarPacienteRequest paciente,
            CancellationToken ct = default)
        {
            return repo.ModificarAsync(
                idPaciente, paciente, ct);
        }

        /// <summary>
        /// Cambiars the estado asynchronous.
        /// </summary>
        /// <param name="idPaciente">The identifier paciente.</param>
        /// <param name="activo">if set to <c>true</c> [activo].</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
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
