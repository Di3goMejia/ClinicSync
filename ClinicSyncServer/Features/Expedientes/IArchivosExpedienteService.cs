using ClinicSyncServer.Common.Abstracciones;
using ClinicSyncServer.Features.Expedientes.Projections;
using ClinicSyncServer.Features.Expedientes.Responses;

namespace ClinicSyncServer.Features.Expedientes
{
    public interface IArchivosExpedienteService
    {
        Task<ArchivoRespuesta?> SubirAsync(int idUsuario, int idVisita, IFormFile archivo,
            string? descripcion = null,
            CancellationToken ct = default);

        Task<ArchivoRespuesta?> SubirExpedienteAsync(
            int idUsuario,
            int idExpediente,
            IFormFile archivo,
            string? descripcion = null,
            CancellationToken ct = default);

        Task<ArchivoExpediente?> ObtenerPorIdAsync(Guid idArchivo, CancellationToken ct = default);

        Task<ContenidoAlmacenado?> AbrirContenidoAsync(Guid idArchivo, CancellationToken ct = default);

        Task<IEnumerable<ArchivoExpediente>> ListarAsync(int idPaciente, 
            int? idVisita = null, CancellationToken ct = default);

        Task ModificarAsync(Guid idArchivo, string nombre,
            string? descripcion = null, CancellationToken ct = default);

        Task CambiarEstadoAsync(Guid idArchivo, bool activo,
            CancellationToken ct = default);
    }
}
