using ClinicSyncServer.Features.Expedientes.Requests;
using ClinicSyncServer.Features.Expedientes.Responses;

namespace ClinicSyncServer.Features.Expedientes
{
    /// <summary>
    /// 
    /// </summary>
    public interface IVisitasService
    {
        /// <summary>
        /// Listars the asynchronous.
        /// </summary>
        /// <param name="idPaciente">The identifier paciente.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<IEnumerable<VisitaListado>> ListarAsync(int idPaciente, 
            CancellationToken ct = default);

        /// <summary>
        /// Obteners the por identifier asynchronous.
        /// </summary>
        /// <param name="idVisita">The identifier visita.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<VisitaListado?> ObtenerPorIdAsync(int idVisita,
            CancellationToken ct = default);

        /// <summary>
        /// Listars the tratamientos asynchronous.
        /// </summary>
        /// <param name="idVisita">The identifier visita.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<IEnumerable<TratamientoVisitaListado>> ListarTratamientosAsync(
            int idVisita, CancellationToken ct = default);

        /// <summary>
        /// Insertars the asynchronous.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<int> InsertarAsync(CrearVisitaRequest request,
            CancellationToken ct = default);

        /// <summary>
        /// Modificars the asynchronous.
        /// </summary>
        /// <param name="idVisita">The identifier visita.</param>
        /// <param name="request">The request.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task ModificarAsync(int idVisita, ModificarVisitaRequest request, 
            CancellationToken ct = default);

        /// <summary>
        /// Cambiars the estado asynchronous.
        /// </summary>
        /// <param name="idVisita">The identifier visita.</param>
        /// <param name="activo">if set to <c>true</c> [activo].</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task CambiarEstadoAsync(int idVisita, bool activo,
            CancellationToken ct = default);

        /// <summary>
        /// Insertars the completa asynchronous.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<int> InsertarCompletaAsync(CrearVisitaCompletaRequest request,
            CancellationToken ct = default);

        /// <summary>
        /// Modificars the completa asynchronous.
        /// </summary>
        /// <param name="idVisita">The identifier visita.</param>
        /// <param name="request">The request.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task ModificarCompletaAsync(int idVisita,
            ModificarVisitaCompletaRequest request,
            CancellationToken ct = default);
    }
}
