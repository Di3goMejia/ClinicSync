using ClinicSyncServer.Features.Expedientes.Responses;

namespace ClinicSyncServer.Features.Expedientes
{
    /// <summary>
    /// 
    /// </summary>
    public interface IExpedientesService
    {
        /// <summary>
        /// Obteners the por identifier asynchronous.
        /// </summary>
        /// <param name="idExpediente">The identifier expediente.</param>
        /// <param name="idPaciente">The identifier paciente.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<ExpedienteDetalle?> ObtenerPorIdAsync(int? idExpediente = null, 
            int? idPaciente = null, CancellationToken ct = default);
        /// <summary>
        /// Modificars the asynchronous.
        /// </summary>
        /// <param name="idPaciente">The identifier paciente.</param>
        /// <param name="notas">The notas.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task ModificarAsync(int idPaciente, string? notas = null, CancellationToken ct = default);
    }
}
