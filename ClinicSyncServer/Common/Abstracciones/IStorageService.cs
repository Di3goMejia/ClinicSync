namespace ClinicSyncServer.Common.Abstracciones
{
    /// <summary>
    /// 
    /// </summary>
    public interface IStorageService
    {
        /// <summary>
        /// Subirs the asynchronous.
        /// </summary>
        /// <param name="clave">The clave.</param>
        /// <param name="contenido">The contenido.</param>
        /// <param name="tipoContenido">The tipo contenido.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task SubirAsync(string clave, Stream contenido, string tipoContenido, CancellationToken ct = default);

        /// <summary>
        /// Abrirs the asynchronous.
        /// </summary>
        /// <param name="clave">The clave.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<ContenidoAlmacenado?> AbrirAsync(string clave, CancellationToken ct = default);

        /// <summary>
        /// Eliminars the asynchronous.
        /// </summary>
        /// <param name="clave">The clave.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task EliminarAsync(string clave, CancellationToken ct = default);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.IEquatable&lt;ClinicSyncServer.Common.Abstracciones.ContenidoAlmacenado&gt;" />
    public sealed record ContenidoAlmacenado(Stream Stream, string TipoContenido, IDisposable Propietario);
}
