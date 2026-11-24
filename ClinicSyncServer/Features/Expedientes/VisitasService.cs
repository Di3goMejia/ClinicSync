using ClinicSyncServer.Features.Expedientes.Data;
using ClinicSyncServer.Features.Expedientes.Requests;
using ClinicSyncServer.Features.Expedientes.Responses;
using Dapper;
using System.Data;
using System.Text.Json;

namespace ClinicSyncServer.Features.Expedientes
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="ClinicSyncServer.Features.Expedientes.IVisitasService" />
    public class VisitasService(VisitasRepository repo) : IVisitasService
    {
        /// <summary>
        /// Listars the asynchronous.
        /// </summary>
        /// <param name="idPaciente">The identifier paciente.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public async Task<IEnumerable<VisitaListado>> ListarAsync(
            int idPaciente, CancellationToken ct = default) =>
                await repo.ListarAsync(idPaciente, ct);

        /// <summary>
        /// Obteners the por identifier asynchronous.
        /// </summary>
        /// <param name="idVisita">The identifier visita.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public async Task<VisitaListado?> ObtenerPorIdAsync(
            int idVisita, CancellationToken ct = default) =>
                await repo.ObtenerPorIdAsync(idVisita, ct);

        /// <summary>
        /// Listars the tratamientos asynchronous.
        /// </summary>
        /// <param name="idVisita">The identifier visita.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public async Task<IEnumerable<TratamientoVisitaListado>> ListarTratamientosAsync(
            int idVisita, CancellationToken ct = default) =>
                await repo.ListarTratamientosAsync(idVisita, ct);

        /// <summary>
        /// Insertars the asynchronous.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public async Task<int> InsertarAsync(CrearVisitaRequest request,
            CancellationToken ct = default) =>
                await repo.InsertarAsync(request, ct);

        /// <summary>
        /// Modificars the asynchronous.
        /// </summary>
        /// <param name="idVisita">The identifier visita.</param>
        /// <param name="request">The request.</param>
        /// <param name="ct">The ct.</param>
        public async Task ModificarAsync(int idVisita,
            ModificarVisitaRequest request, CancellationToken ct = default) =>
                await repo.ModificarAsync(idVisita, request, ct);

        /// <summary>
        /// Cambiars the estado asynchronous.
        /// </summary>
        /// <param name="idVisita">The identifier visita.</param>
        /// <param name="activo">if set to <c>true</c> [activo].</param>
        /// <param name="ct">The ct.</param>
        public async Task CambiarEstadoAsync(int idVisita, bool activo, 
            CancellationToken ct = default) =>
                await repo.CambiarEstadoAsync(idVisita, activo, ct);

        /// <summary>
        /// Insertars the completa asynchronous.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public async Task<int> InsertarCompletaAsync(
            CrearVisitaCompletaRequest request,
            CancellationToken ct = default) =>
                await repo.InsertarCompletaAsync(request, ct);

        /// <summary>
        /// Modificars the completa asynchronous.
        /// </summary>
        /// <param name="idVisita">The identifier visita.</param>
        /// <param name="request">The request.</param>
        /// <param name="ct">The ct.</param>
        public async Task ModificarCompletaAsync(int idVisita,
            ModificarVisitaCompletaRequest request,
            CancellationToken ct = default) =>
                await repo.ModificarCompletaAsync(idVisita, request, ct);
    }
}
