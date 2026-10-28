namespace ClinicSyncServer.Common.Abstracciones
{
    public interface IStorageService
    {
        Task SubirAsync(string clave, Stream contenido, string tipoContenido, CancellationToken ct = default);

        Task<ContenidoAlmacenado?> AbrirAsync(string clave, CancellationToken ct = default);

        Task EliminarAsync(string clave, CancellationToken ct = default);
    }

    public sealed record ContenidoAlmacenado(Stream Stream, string TipoContenido, IDisposable Propietario);
}
