using ClinicSyncServer.Features.Expedientes.Responses;

namespace ClinicSyncServer.Features.Expedientes
{
    public interface IExpedientesService
    {
        Task<ExpedienteDetalle?> ObtenerPorIdAsync(int? idExpediente = null, 
            int? idPaciente = null, CancellationToken ct = default);
        Task ModificarAsync(int idPaciente, string? notas = null, CancellationToken ct = default);
    }
}
