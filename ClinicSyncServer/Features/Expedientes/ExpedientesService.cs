using ClinicSyncServer.Features.Expedientes.Data;
using ClinicSyncServer.Features.Expedientes.Responses;

namespace ClinicSyncServer.Features.Expedientes
{
    public class ExpedientesService(ExpedientesRepository repo) : IExpedientesService
    {
        public async Task<ExpedienteDetalle?> 
            ObtenerPorIdAsync(int? idExpediente = null, int? idPaciente = null, CancellationToken ct = default) =>
                await repo.ObtenerPorIdAsync(idExpediente, idPaciente, ct);

        public async Task ModificarAsync(int idPaciente, string? notas = null,
            CancellationToken ct = default) =>
                await repo.ModificarAsync(idPaciente, notas, ct);
    }
}
