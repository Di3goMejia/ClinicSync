using ClinicSyncServer.Features.Expedientes.Data;
using ClinicSyncServer.Features.Expedientes.Requests;
using ClinicSyncServer.Features.Expedientes.Responses;
using Dapper;
using System.Data;
using System.Text.Json;

namespace ClinicSyncServer.Features.Expedientes
{
    public class VisitasService(VisitasRepository repo) : IVisitasService
    {
        public async Task<IEnumerable<VisitaListado>> ListarAsync(
            int idPaciente, CancellationToken ct = default) =>
                await repo.ListarAsync(idPaciente, ct);

        public async Task<VisitaListado?> ObtenerPorIdAsync(
            int idVisita, CancellationToken ct = default) =>
                await repo.ObtenerPorIdAsync(idVisita, ct);

        public async Task<IEnumerable<TratamientoVisitaListado>> ListarTratamientosAsync(
            int idVisita, CancellationToken ct = default) =>
                await repo.ListarTratamientosAsync(idVisita, ct);

        public async Task<int> InsertarAsync(CrearVisitaRequest request,
            CancellationToken ct = default) =>
                await repo.InsertarAsync(request, ct);

        public async Task ModificarAsync(int idVisita,
            ModificarVisitaRequest request, CancellationToken ct = default) =>
                await repo.ModificarAsync(idVisita, request, ct);

        public async Task CambiarEstadoAsync(int idVisita, bool activo, 
            CancellationToken ct = default) =>
                await repo.CambiarEstadoAsync(idVisita, activo, ct);

        public async Task<int> InsertarCompletaAsync(
            CrearVisitaCompletaRequest request,
            CancellationToken ct = default) =>
                await repo.InsertarCompletaAsync(request, ct);

        public async Task ModificarCompletaAsync(int idVisita,
            ModificarVisitaCompletaRequest request,
            CancellationToken ct = default) =>
                await repo.ModificarCompletaAsync(idVisita, request, ct);
    }
}
