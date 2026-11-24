using Microsoft.AspNetCore.Http;

namespace ClinicSyncServer.Features.Inventario.Imagenes
{
    /// <summary>
    /// 
    /// </summary>
    public interface IAlmacenamientoImagenesService
    {
        /// <summary>
        /// Guardars the asynchronous.
        /// </summary>
        /// <param name="archivo">The archivo.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<string> GuardarAsync(
            IFormFile archivo,
            CancellationToken ct = default);

        /// <summary>
        /// Obteners the ruta.
        /// </summary>
        /// <param name="nombreArchivo">The nombre archivo.</param>
        /// <returns></returns>
        string ObtenerRuta(string nombreArchivo);
    }
}
