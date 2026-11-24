
using ClinicSyncServer.Features.Pacientes.Responses;

namespace ClinicSyncServer.Features.Pacientes
{
    /// <summary>
    /// 
    /// </summary>
    public interface IPacientesService
    {
        /// <summary>
        /// Listars the asynchronous.
        /// </summary>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<IEnumerable<PacienteListado>> ListarAsync(
            CancellationToken ct = default);

        /// <summary>
        /// Insertars the asynchronous.
        /// </summary>
        /// <param name="paciente">The paciente.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task InsertarAsync(
            RegistrarPacienteRequest paciente,
            CancellationToken ct = default);

        /// <summary>
        /// Modificars the asynchronous.
        /// </summary>
        /// <param name="idPaciente">The identifier paciente.</param>
        /// <param name="paciente">The paciente.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task ModificarAsync(
            int idPaciente,
            ModificarPacienteRequest paciente,
            CancellationToken ct = default);

        /// <summary>
        /// Cambiars the estado asynchronous.
        /// </summary>
        /// <param name="idPaciente">The identifier paciente.</param>
        /// <param name="activo">if set to <c>true</c> [activo].</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task CambiarEstadoAsync(
            int idPaciente,
            bool activo,
            CancellationToken ct = default);
    }
}
