using ClinicSyncServer.Common.Abstracciones;
using ClinicSyncServer.Features.Expedientes.Projections;
using ClinicSyncServer.Features.Expedientes.Responses;

namespace ClinicSyncServer.Features.Expedientes
{
    /// <summary>
    /// 
    /// </summary>
    public interface IArchivosExpedienteService
    {
        /// <summary>
        /// Subirs the asynchronous.
        /// </summary>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="idVisita">The identifier visita.</param>
        /// <param name="archivo">The archivo.</param>
        /// <param name="descripcion">The descripcion.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<ArchivoRespuesta?> SubirAsync(int idUsuario, int idVisita, IFormFile archivo,
            string? descripcion = null,
            CancellationToken ct = default);

        /// <summary>
        /// Subirs the expediente asynchronous.
        /// </summary>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="idExpediente">The identifier expediente.</param>
        /// <param name="archivo">The archivo.</param>
        /// <param name="descripcion">The descripcion.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<ArchivoRespuesta?> SubirExpedienteAsync(
            int idUsuario,
            int idExpediente,
            IFormFile archivo,
            string? descripcion = null,
            CancellationToken ct = default);

        /// <summary>
        /// Obteners the por identifier asynchronous.
        /// </summary>
        /// <param name="idArchivo">The identifier archivo.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<ArchivoExpediente?> ObtenerPorIdAsync(Guid idArchivo, CancellationToken ct = default);

        /// <summary>
        /// Abrirs the contenido asynchronous.
        /// </summary>
        /// <param name="idArchivo">The identifier archivo.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<ContenidoAlmacenado?> AbrirContenidoAsync(Guid idArchivo, CancellationToken ct = default);

        /// <summary>
        /// Listars the asynchronous.
        /// </summary>
        /// <param name="idPaciente">The identifier paciente.</param>
        /// <param name="idVisita">The identifier visita.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<IEnumerable<ArchivoExpediente>> ListarAsync(int idPaciente, 
            int? idVisita = null, CancellationToken ct = default);

        /// <summary>
        /// Modificars the asynchronous.
        /// </summary>
        /// <param name="idArchivo">The identifier archivo.</param>
        /// <param name="nombre">The nombre.</param>
        /// <param name="descripcion">The descripcion.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task ModificarAsync(Guid idArchivo, string nombre,
            string? descripcion = null, CancellationToken ct = default);

        /// <summary>
        /// Cambiars the estado asynchronous.
        /// </summary>
        /// <param name="idArchivo">The identifier archivo.</param>
        /// <param name="activo">if set to <c>true</c> [activo].</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task CambiarEstadoAsync(Guid idArchivo, bool activo,
            CancellationToken ct = default);
    }
}
