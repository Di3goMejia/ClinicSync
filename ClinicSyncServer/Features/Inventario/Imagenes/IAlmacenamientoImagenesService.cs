using Microsoft.AspNetCore.Http;

namespace ClinicSyncServer.Features.Inventario.Imagenes
{
    public interface IAlmacenamientoImagenesService
    {
        Task<string> GuardarAsync(
            IFormFile archivo,
            CancellationToken ct = default);

        string ObtenerRuta(string nombreArchivo);
    }
}
