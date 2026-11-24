using ClinicSyncServer.Features.Expedientes.Data;
using ClinicSyncServer.Features.Expedientes.Responses;

namespace ClinicSyncServer.Features.Expedientes
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="ClinicSyncServer.Features.Expedientes.IExpedientesService" />
    public class ExpedientesService(ExpedientesRepository repo) : IExpedientesService
    {
        /// <summary>
        /// Obteners the por identifier asynchronous.
        /// </summary>
        /// <param name="idExpediente">The identifier expediente.</param>
        /// <param name="idPaciente">The identifier paciente.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public async Task<ExpedienteDetalle?> 
            ObtenerPorIdAsync(int? idExpediente = null, int? idPaciente = null, CancellationToken ct = default) =>
                await repo.ObtenerPorIdAsync(idExpediente, idPaciente, ct);

        /// <summary>
        /// Modificars the asynchronous.
        /// </summary>
        /// <param name="idPaciente">The identifier paciente.</param>
        /// <param name="notas">The notas.</param>
        /// <param name="ct">The ct.</param>
        public async Task ModificarAsync(int idPaciente, string? notas = null,
            CancellationToken ct = default) =>
                await repo.ModificarAsync(idPaciente, notas, ct);
    }
}
