using ClinicSyncServer.Common.Abstracciones;
using ClinicSyncServer.Features.Usuarios.Data;
using ClinicSyncServer.Features.Usuarios.Requests;
using ClinicSyncServer.Features.Usuarios.Responses;
using System.Security.Cryptography;

namespace ClinicSyncServer.Features.Usuarios
{
    public class UsuariosService(UsuariosRepository repo, IEmailSender correo) : IUsuariosService
    {
        private static readonly TimeSpan VigenciaActivacion = TimeSpan.FromHours(24); 

        public async Task<CrearUsuarioResponse> CrearAsync(CrearUsuarioRequest req, CancellationToken ct = default)
        {
            // Generar el token.
            var tokenBytes = RandomNumberGenerator.GetBytes(32);
            var token = Convert.ToBase64String(tokenBytes);
            var tokenHash = SHA256.HashData(tokenBytes);
            var expira = DateTime.UtcNow.Add(VigenciaActivacion);

            // Crear el usuario.
            var resp = await repo.CrearConEmpleadoAsync(req, tokenHash, expira);

            // Enviar el correo.
            await correo.EnviarActivacionAsync(req.CorreoLogin, req.Nombres, token, ct);
            return resp;
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
    }
}
