using Microsoft.AspNetCore.Http;

namespace ClinicSyncServer.Features.Inventario.Imagenes
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="ClinicSyncServer.Features.Inventario.Imagenes.IAlmacenamientoImagenesService" />
    public class AlmacenamientoImagenesService
        : IAlmacenamientoImagenesService
    {
        /// <summary>
        /// The tamano maximo
        /// </summary>
        private const long TamanoMaximo = 5 * 1024 * 1024;

        /// <summary>
        /// The carpeta
        /// </summary>
        private readonly string carpeta;

        /// <summary>
        /// Initializes a new instance of the <see cref="AlmacenamientoImagenesService"/> class.
        /// </summary>
        /// <param name="environment">The environment.</param>
        public AlmacenamientoImagenesService(
            IWebHostEnvironment environment)
        {
            carpeta = Path.Combine(
                environment.ContentRootPath,
                "App_Data",
                "Imagenes",
                "Materiales");

            Directory.CreateDirectory(carpeta);
        }

        /// <summary>
        /// Guardars the asynchronous.
        /// </summary>
        /// <param name="archivo">The archivo.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">
        /// Debes seleccionar una imagen.
        /// or
        /// La imagen no puede superar los 5 MB.
        /// or
        /// El archivo debe ser una imagen PNG, JPEG o WebP válida.
        /// </exception>
        public async Task<string> GuardarAsync(
            IFormFile archivo,
            CancellationToken ct = default)
        {
            if (archivo is null || archivo.Length == 0)
                throw new ArgumentException(
                    "Debes seleccionar una imagen.");

            if (archivo.Length > TamanoMaximo)
                throw new ArgumentException(
                    "La imagen no puede superar los 5 MB.");

            await using var entrada = archivo.OpenReadStream();

            byte[] firma = new byte[12];
            int leidos = await entrada.ReadAsync(
                firma.AsMemory(0, firma.Length), ct);

            string? extension = ObtenerExtension(
                firma, leidos);

            if (extension is null)
                throw new ArgumentException(
                    "El archivo debe ser una imagen PNG, JPEG o WebP válida.");

            string nombre = $"{Guid.NewGuid():N}{extension}";
            string destino = Path.Combine(carpeta, nombre);

            entrada.Position = 0;

            await using (var salida = new FileStream(
                destino,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                81920,
                FileOptions.Asynchronous))
            {
                await entrada.CopyToAsync(salida, ct);
            }

            return $"/api/inventario/materiales/imagenes/{nombre}";
        }

        /// <summary>
        /// Obteners the ruta.
        /// </summary>
        /// <param name="nombreArchivo">The nombre archivo.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Nombre de archivo no válido.</exception>
        public string ObtenerRuta(string nombreArchivo)
        {
            string nombreSeguro = Path.GetFileName(nombreArchivo);

            if (string.IsNullOrWhiteSpace(nombreSeguro) ||
                nombreSeguro != nombreArchivo)
            {
                throw new ArgumentException(
                    "Nombre de archivo no válido.");
            }

            return Path.Combine(carpeta, nombreSeguro);
        }

        /// <summary>
        /// Obteners the extension.
        /// </summary>
        /// <param name="firma">The firma.</param>
        /// <param name="longitud">The longitud.</param>
        /// <returns></returns>
        private static string? ObtenerExtension(
            byte[] firma, int longitud)
        {
            // JPEG
            if (longitud >= 3 &&
                firma[0] == 0xFF &&
                firma[1] == 0xD8 &&
                firma[2] == 0xFF)
                return ".jpg";

            // PNG
            if (longitud >= 8 &&
                firma[0] == 0x89 &&
                firma[1] == 0x50 &&
                firma[2] == 0x4E &&
                firma[3] == 0x47 &&
                firma[4] == 0x0D &&
                firma[5] == 0x0A &&
                firma[6] == 0x1A &&
                firma[7] == 0x0A)
                return ".png";

            // WebP: RIFF....WEBP
            if (longitud >= 12 &&
                firma[0] == 0x52 &&
                firma[1] == 0x49 &&
                firma[2] == 0x46 &&
                firma[3] == 0x46 &&
                firma[8] == 0x57 &&
                firma[9] == 0x45 &&
                firma[10] == 0x42 &&
                firma[11] == 0x50)
                return ".webp";

            return null;
        }
    }
}
