using ClinicSyncServer.Features.Egresos.Data;
using ClinicSyncServer.Features.Egresos.Requests;
using ClinicSyncServer.Features.Egresos.Responses;

namespace ClinicSyncServer.Features.Egresos
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="ClinicSyncServer.Features.Egresos.IEgresosService" />
    public class EgresosService(EgresosRepository repositorio) : IEgresosService
    {
        /// <summary>
        /// Insertars the asynchronous.
        /// </summary>
        /// <param name="req">The req.</param>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public Task<int> InsertarAsync(
            RegistrarEgresoRequest req,
            int idUsuario,
            CancellationToken ct = default)
            => repositorio.InsertarAsync(req, idUsuario, ct);

        /// <summary>
        /// Obteners the asynchronous.
        /// </summary>
        /// <param name="idEgreso">The identifier egreso.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public Task<EgresoListado?> ObtenerAsync(
            int idEgreso,
            CancellationToken ct = default)
            => repositorio.ObtenerAsync(idEgreso, ct);

        /// <summary>
        /// Actualizars the asynchronous.
        /// </summary>
        /// <param name="idEgreso">The identifier egreso.</param>
        /// <param name="req">The req.</param>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public Task ActualizarAsync(
            int idEgreso,
            ActualizarEgresoRequest req,
            int idUsuario,
            CancellationToken ct = default)
            => repositorio.ActualizarAsync(idEgreso, req, idUsuario, ct);

        /// <summary>
        /// Listars the asynchronous.
        /// </summary>
        /// <param name="desde">The desde.</param>
        /// <param name="hasta">The hasta.</param>
        /// <param name="activo">The activo.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public Task<IEnumerable<EgresoListado>> ListarAsync(
            DateTime desde,
            DateTime hasta,
            bool? activo = null,
            CancellationToken ct = default)
            => repositorio.ListarAsync(desde, hasta, activo, ct);
    }
}
