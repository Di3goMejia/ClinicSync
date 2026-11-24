using ClinicSyncServer.Features.Sesiones.Projections;

namespace ClinicSyncServer.Features.Sesiones
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISesionService
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
        Task<Guid> CrearAsync(int idUsuario, byte[] hashRefresh, string equipo,
            string? ip, DateTime expira, CancellationToken ct = default);

        /// <summary>
        /// Rotars the asynchronous.
        /// </summary>
        /// <param name="hashActual">The hash actual.</param>
        /// <param name="hashNuevo">The hash nuevo.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<SesionRotada?> RotarAsync(byte[] hashActual, byte[] hashNuevo,
            CancellationToken ct = default);

        /// <summary>
        /// Cerrars the asynchronous.
        /// </summary>
        /// <param name="idSesion">The identifier sesion.</param>
        /// <param name="motivo">The motivo.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task CerrarAsync(Guid idSesion, string motivo, CancellationToken ct = default);

        /// <summary>
        /// Eses the activa asynchronous.
        /// </summary>
        /// <param name="idSesion">The identifier sesion.</param>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<bool> EsActivaAsync(Guid idSesion, int idUsuario, CancellationToken ct = default);

    }
}
