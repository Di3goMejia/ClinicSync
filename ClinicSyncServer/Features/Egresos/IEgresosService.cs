using ClinicSyncServer.Features.Egresos.Requests;
using ClinicSyncServer.Features.Egresos.Responses;

namespace ClinicSyncServer.Features.Egresos
{
    /// <summary>
    /// 
    /// </summary>
    public interface IEgresosService
    {
        /// <summary>
        /// Insertars the asynchronous.
        /// </summary>
        /// <param name="req">The req.</param>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<int> InsertarAsync(
            RegistrarEgresoRequest req,
            int idUsuario,
            CancellationToken ct = default);

        /// <summary>
        /// Obteners the asynchronous.
        /// </summary>
        /// <param name="idEgreso">The identifier egreso.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<EgresoListado?> ObtenerAsync(
            int idEgreso,
            CancellationToken ct = default);

        /// <summary>
        /// Actualizars the asynchronous.
        /// </summary>
        /// <param name="idEgreso">The identifier egreso.</param>
        /// <param name="req">The req.</param>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task ActualizarAsync(
            int idEgreso,
            ActualizarEgresoRequest req,
            int idUsuario,
            CancellationToken ct = default);

        /// <summary>
        /// Listars the asynchronous.
        /// </summary>
        /// <param name="desde">The desde.</param>
        /// <param name="hasta">The hasta.</param>
        /// <param name="activo">The activo.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<IEnumerable<EgresoListado>> ListarAsync(
            DateTime desde,
            DateTime hasta,
            bool? activo = null,
            CancellationToken ct = default);
    }
}
