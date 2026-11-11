using ClinicSyncServer.Common.Abstracciones;
using ClinicSyncServer.Common;
using ClinicSyncServer.Features.Usuarios.Data;
using ClinicSyncServer.Features.Usuarios.Projections;
using ClinicSyncServer.Features.Usuarios.Requests;
using ClinicSyncServer.Features.Usuarios.Responses;
using ClinicSyncServer.Features.Auth;
using ClinicSyncServer.Features.Auth.Data;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace ClinicSyncServer.Features.Usuarios
{
    public class UsuariosService(
        UsuariosRepository repo,
        IEmailSender correo,
        IStorageService almacen,
        IPasswordHasher hasher,
        TokensUsuarioRepository tokens,
        IAuthService auth) : IUsuariosService
    {
        private static readonly TimeSpan VigenciaActivacion = TimeSpan.FromHours(24); 
        private const string PropositoActivacion = "PRIMER_LOGIN";

        public async Task<CrearUsuarioResponse> CrearAsync(CrearUsuarioRequest req, CancellationToken ct = default)
        {
            // Generar el token de activación de 6 dígitos.
            var (token, tokenHash) = await GenerarTokenActivacionAsync(ct);
            var expira = DateTime.UtcNow.Add(VigenciaActivacion);

            // Crear el usuario.
            var resp = await repo.CrearConEmpleadoAsync(req, tokenHash, expira);

            // Enviar el correo.
            await correo.EnviarActivacionAsync(req.CorreoLogin, req.Nombres, token, ct);
            return resp;
        }

        public async Task EliminarImagenPerfilAsync(
            int idUsuario,
            CancellationToken ct = default)
        {
            // Desvincular la fotografía del usuario.
            // NO eliminar el archivo físico.
            await repo.EliminarImagenPerfilAsync(idUsuario, ct);
        }

        public async Task<UsuarioDetalleResponse?> ObtenerAsync(int idSolicitado, int idSolicitante, bool puedeVerTodos, CancellationToken ct = default)
        {
            // Una persona sin el permiso de leer usuarios solo puede consultar la información de su
            // propio usuario.
            if (!puedeVerTodos && idSolicitado != idSolicitante)
                return null;

            var u = await repo.ObtenerPorIdAsync(idSolicitado);
            if (u is null) return null;

            return new UsuarioDetalleResponse(u.IdUsuario, u.IdEmpleado, u.Identidad, 
                u.Nombres, u.Apellidos, u.FechaNacimiento, u.Genero, u.Telefono, 
                u.CorreoContacto, u.ImagenPerfil, u.NombreUsuario, u.CorreoLogin, 
                u.IdRol, u.Estado, u.FechaRegistro);
        }

        public async Task<IEnumerable<UsuarioListado>> ListarAsync()
        {
            return await repo.ListarAsync();
        }

        public async Task ModificarAsync(ModificarUsuarioRequest req, int idSolicitante, bool puedeEditarTodos, CancellationToken ct = default)
        {
            // Una persona sin el permiso de editar usuarios solo puede editar la información de su propio usuario.
            if (!puedeEditarTodos && req.IdUsuario != idSolicitante)
                throw new KeyNotFoundException("El usuario no existe.");

            await repo.ModificarAsync(req);
        }

        public async Task ModificarRolAsync(ModificarRolUsuarioRequest req, int idSolicitante, CancellationToken ct = default)
        {
            if (req.IdUsuario == idSolicitante)
                throw new ReglaNegocioException("No se puede modificar el rol del usuario activo.", 0);

            await repo.ModificarRolAsync(req);
        }

        public async Task SubirImagenPerfilAsync(int idUsuario, IFormFile archivo, CancellationToken ct = default)
        {
            const long tamanoMaximo = 5 * 1024 * 1024;
            if (archivo.Length == 0 || archivo.Length > tamanoMaximo)
                throw new ReglaNegocioException("La imagen debe tener contenido y no superar los 5 MB.", 0);

            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            var tipoMime = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => throw new ReglaNegocioException("Solo se permiten imágenes JPG, JPEG o PNG.", 0)
            };

            await using var entrada = archivo.OpenReadStream();
            using var memoria = new MemoryStream();
            await entrada.CopyToAsync(memoria, ct);
            var contenido = memoria.ToArray();

            if (!TipoImagenValido(contenido, extension))
                throw new ReglaNegocioException("El contenido no corresponde al tipo de imagen.", 0);

            var clave = $"usuarios/{idUsuario}/perfil/{Guid.NewGuid():N}{extension}";
            var claveAnterior = await repo.ObtenerImagenPerfilAsync(idUsuario);

            await using var subida = new MemoryStream(contenido);
            await almacen.SubirAsync(clave, subida, tipoMime, ct);

            try
            {
                await repo.CambiarImagenPerfilAsync(idUsuario, clave, ct);
            }
            catch
            {
                await almacen.EliminarAsync(clave, CancellationToken.None);
                throw;
            }

        }

        public async Task<ContenidoAlmacenado?> ObtenerImagenPerfilAsync(int idUsuario, CancellationToken ct = default)
        {
            var clave = await repo.ObtenerImagenPerfilAsync(idUsuario);
            return string.IsNullOrWhiteSpace(clave)
                ? null
                : await almacen.AbrirAsync(clave, ct);
        }

        private static bool TipoImagenValido(byte[] contenido, string extension)
        {
            byte[] firma = extension switch
            {
                ".png" => [137, 80, 78, 71, 13, 10, 26, 10],
                ".jpg" or ".jpeg" => [0xff, 0xd8, 0xff],
                _ => []
            };

            return contenido.Length >= firma.Length && contenido.AsSpan(0, firma.Length).SequenceEqual(firma);
        }

        private async Task<(string Token, byte[] TokenHash)> GenerarTokenActivacionAsync(CancellationToken ct = default)
        {
            string token;
            byte[] tokenHash;
            do
            {
                token = RandomNumberGenerator.GetInt32(100000, 1000000).ToString("D6");
                tokenHash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            } while (await tokens.ExisteTokenHashAsync(tokenHash, ct));

            return (token, tokenHash);
        }

        public async Task ReenviarActivacionAsync(int idUsuario, CancellationToken ct = default)
        {
            var usuario = await repo.ObtenerPorIdAsync(idUsuario)
                ?? throw new KeyNotFoundException("El usuario no existe.");

            if (!usuario.Estado.Equals("PENDIENTE", StringComparison.OrdinalIgnoreCase))
                throw new ReglaNegocioException("Solo se puede reenviar la activación a cuentas pendientes.", 0);

            var (token, tokenHash) = await GenerarTokenActivacionAsync(ct);
            await tokens.AnularPendientesAsync(idUsuario, PropositoActivacion);
            await tokens.InsertarAsync(idUsuario, tokenHash, PropositoActivacion,
                DateTime.UtcNow.Add(VigenciaActivacion));
            await correo.EnviarActivacionAsync(usuario.CorreoLogin, usuario.Nombres, token, ct);
        }

        public async Task DesactivarAsync(int idUsuario, int idAdministrador, string password, CancellationToken ct = default)
        {
            if (idUsuario == idAdministrador)
            {
                throw new ReglaNegocioException("No se puede desactivar el usuario de la sesión actual.", 0);
            }
            
            await VerificarAdministradorAsync(idAdministrador, password, ct);
            var usuario = await repo.ObtenerPorIdAsync(idUsuario)
                ?? throw new KeyNotFoundException("El usuario no existe.");

            await repo.CambiarEstadoAsync(idUsuario, activo: false, ct);
            await correo.EnviarDesactivacionAsync(usuario.CorreoLogin, usuario.Nombres, ct);
        }

        public async Task ReactivarAsync(int idUsuario, int idAdministrador, string password, CancellationToken ct = default)
        {
            await VerificarAdministradorAsync(idAdministrador, password, ct);
            var usuario = await repo.ObtenerPorIdAsync(idUsuario)
                ?? throw new KeyNotFoundException("El usuario no existe.");
            await repo.CambiarEstadoAsync(idUsuario, activo: true, ct);
            await correo.EnviarReactivacionAsync(usuario.CorreoLogin, usuario.Nombres, ct);
        }

        public async Task RestablecerPasswordAsync(int idUsuario, int idAdministrador, string password, CancellationToken ct = default)
        {
            await VerificarAdministradorAsync(idAdministrador, password, ct);
            var usuario = await repo.ObtenerPorIdAsync(idUsuario)
                ?? throw new KeyNotFoundException("El usuario no existe.");
            await auth.SolicitarRestablecimientoAsync(usuario.CorreoLogin, ct);
        }

        private async Task VerificarAdministradorAsync(int idAdministrador, string password, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new NoAutorizadoException("La contraseña del administrador es obligatoria.");

            var hash = await repo.ObtenerClavePasswordAsync(idAdministrador);
            if (hash is null || !hasher.Comparar(password, hash))
                throw new NoAutorizadoException("La contraseña del administrador no es válida.");
        }
    }
}
