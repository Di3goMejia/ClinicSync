using ClinicSyncServer.Features.Sesiones.Data;
using ClinicSyncServer.Features.Sesiones.Projections;

namespace ClinicSyncServer.Features.Sesiones
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="ClinicSyncServer.Features.Sesiones.ISesionService" />
    public class SesionService(SesionesRepository repo) : ISesionService
    {
        /// <summary>
        /// Crears the asynchronous.
        /// </summary>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="hashRefresh">The hash refresh.</param>
        /// <param name="equipo">The equipo.</param>
        /// <param name="ip">The ip.</param>
        /// <param name="expira">The expira.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public Task<Guid> CrearAsync(int idUsuario, byte[] hashRefresh, string equipo,
            string? ip, DateTime expira, CancellationToken ct = default) =>
            repo.CrearAsync(idUsuario, hashRefresh, equipo, ip, expira);

        /// <summary>
        /// Rotars the asynchronous.
        /// </summary>
        /// <param name="hashActual">The hash actual.</param>
        /// <param name="hashNuevo">The hash nuevo.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public Task<SesionRotada?> RotarAsync(byte[] hashActual, byte[] hashNuevo,
            CancellationToken ct = default) =>
            repo.RotarAsync(hashActual, hashNuevo);

        /// <summary>
        /// Cerrars the asynchronous.
        /// </summary>
        /// <param name="idSesion">The identifier sesion.</param>
        /// <param name="motivo">The motivo.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public Task CerrarAsync(Guid idSesion, string motivo, CancellationToken ct = default) =>
            repo.CerrarAsync(idSesion, motivo);

        /// <summary>
        /// Eses the activa asynchronous.
        /// </summary>
        /// <param name="idSesion">The identifier sesion.</param>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public Task<bool> EsActivaAsync(Guid idSesion, int idUsuario, CancellationToken ct = default) =>
            repo.EsActivaAsync(idSesion, idUsuario);
    }
}
