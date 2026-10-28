using ClinicSyncServer.Common;
using ClinicSyncServer.Common.Abstracciones;
using ClinicSyncServer.Features.Expedientes.Data;
using ClinicSyncServer.Features.Expedientes.Projections;
using ClinicSyncServer.Features.Expedientes.Responses;
using Dapper;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace ClinicSyncServer.Features.Expedientes
{
    public class ArchivosExpedienteService(
        ArchivosExpedienteRepository repo,
        IVisitasService visitas,
        IExpedientesService expedientes,
        IStorageService almacen) : IArchivosExpedienteService
    {
        private const long TamanoMaximo = 10 * 1024 * 1024; // 10 MiB
        private readonly IReadOnlyList<string> ExtensionesPermitidas = [
            ".png", ".jpg", ".jpeg", ".pdf"
        ];
        private readonly Dictionary<string, string> MimesExtension = new Dictionary<string, string>
        {
            {".png", "image/png" },
            {".pdf", "application/pdf" },
            {".jpg", "image/jpeg" },
            {".jpeg", "image/jpeg" }
        };

        private static bool TipoValido(
            byte[] datos,
            string extension)
        {
            byte[] firma = extension switch
            {
                ".pdf" => [0x25, 0x50, 0x44, 0x46, 0x2D],
                ".png" => [137, 80, 78, 71, 13, 10, 26, 10],
                ".jpg" or ".jpeg" => [0xff, 0xd8, 0xff],
                _ => throw new InvalidOperationException("La extensión no es soportada.")
            };

            return datos.Length >= firma.Length && datos.Take(firma.Length).SequenceEqual(firma);
        }

        private static string CalcularHash(byte[] contenido)
        {
            byte[] hashBinary = SHA256.HashData(contenido);

            var resultado = new StringBuilder(hashBinary.Length * 2);
            foreach (byte elem in hashBinary)
                resultado.Append(elem.ToString("x2"));

            return resultado.ToString();
        }

        public async Task<ArchivoRespuesta?> SubirAsync(int idUsuario, int idVisita, IFormFile archivo,
            string? descripcion = null,
            CancellationToken ct = default)
        {
            var visita = await visitas.ObtenerPorIdAsync(idVisita, ct) ??
                throw new ReglaNegocioException("La visita no existe.", 0);

            return await SubirInternoAsync(
                idUsuario,
                visita.IdExpediente,
                idVisita,
                archivo,
                descripcion,
                ct);
        }

        public async Task<ArchivoRespuesta?> SubirExpedienteAsync(
            int idUsuario,
            int idExpediente,
            IFormFile archivo,
            string? descripcion = null,
            CancellationToken ct = default)
        {
            var expediente = await expedientes.ObtenerPorIdAsync(
                idExpediente,
                null,
                ct) ?? throw new ReglaNegocioException(
                    "El expediente no existe.", 0);

            return await SubirInternoAsync(
                idUsuario,
                expediente.IdExpediente,
                null,
                archivo,
                descripcion,
                ct);
        }

        private async Task<ArchivoRespuesta?> SubirInternoAsync(
            int idUsuario,
            int idExpediente,
            int? idVisita,
            IFormFile archivo,
            string? descripcion,
            CancellationToken ct)
        {
            var expediente = await expedientes.ObtenerPorIdAsync(
                idExpediente,
                null,
                ct) ?? throw new ReglaNegocioException(
                    "El expediente no existe.", 0);

            // VALIDAR TAMAÑO
            if (archivo.Length > TamanoMaximo)
                throw new ReglaNegocioException("El archivo supera el máximo permitido (10 MiB).", 0);

            // VALIDAR EXTENSIÓN
            string extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            if (!ExtensionesPermitidas.Contains(extension))
                throw new ReglaNegocioException("Extensión no permitida. Se permiten .jpg, .jpeg, .png y .pdf.", 0);

            // LEER CONTENIDO
            byte[] contenido;
            using var mem = new MemoryStream();

            await archivo.CopyToAsync(mem, ct);
            contenido = mem.ToArray();
            mem.Position = 0;

            // VALIDAR CONTENIDO REAL
            if (!TipoValido(contenido, extension))
                throw new ReglaNegocioException("El contenido no corresponde al tipo de archivo", 0);

            string tipoMime = MimesExtension[extension];

            // GENERAR HASH SHA256
            var hash = CalcularHash(contenido);

            var idArchivo = Guid.NewGuid();
            var clave = idVisita.HasValue
                ? $"visitas/{idVisita}/{idArchivo}"
                : $"expedientes/{idExpediente}/{idArchivo}";

            await almacen.SubirAsync(clave, mem, tipoMime, ct);


            try
            {
                await repo.InsertarAsync(
                    idArchivo, expediente.IdPaciente, idVisita,
                    idUsuario, archivo.FileName, tipoMime, descripcion,
                    contenido.LongLength, clave, hash, ct);
            }
            catch
            {
                await almacen.EliminarAsync(clave, CancellationToken.None);
                throw;
            }

            var arc = await repo.ObtenerPorIdAsync(idArchivo, ct);
            if (arc is not null)
                return new ArchivoRespuesta(
                    arc.IdArchivo,
                    arc.IdUsuario,
                    arc.NombreArchivo,
                    arc.ClaveAlmacen,
                    arc.Descripcion,
                    arc.TipoMime,
                    arc.TamanoBytes,
                    arc.FechaSubida,
                    arc.Activo
                );

            return null;
        }

        public async Task<ArchivoExpediente?> ObtenerPorIdAsync(Guid idArchivo, CancellationToken ct = default) =>
            await repo.ObtenerPorIdAsync(idArchivo, ct);

        public async Task<ContenidoAlmacenado?> AbrirContenidoAsync(Guid idArchivo, CancellationToken ct = default)
        {
            var fila = await repo.ObtenerPorIdAsync(idArchivo, ct);
            if (fila is null || !fila.Activo) return null;

            if (fila.IdVisita.HasValue)
            {
                var visita = await visitas.ObtenerPorIdAsync(
                    fila.IdVisita.Value,
                    ct);

                if (visita is null || !visita.Activo)
                    return null;
            }

            return await almacen.AbrirAsync(fila.ClaveAlmacen, ct);
        }   

        public async Task<IEnumerable<ArchivoExpediente>> ListarAsync(
            int idPaciente, int? idVisita = null, CancellationToken ct = default) =>
                await repo.ListarAsync(idPaciente, idVisita, ct);

        public async Task ModificarAsync(Guid idArchivo, string nombre,
            string? descripcion = null, CancellationToken ct = default) =>
                await repo.ModificarAsync(idArchivo, nombre, descripcion, ct);

        public async Task CambiarEstadoAsync(Guid idArchivo, bool activo,
            CancellationToken ct = default) =>
                await repo.CambiarEstadoAsync(idArchivo, activo, ct);
    }
}
