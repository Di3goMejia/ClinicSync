using ClinicSyncServer.Features.Permisos.Projections;

namespace ClinicSyncServer.Common.Abstracciones
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.IEquatable&lt;ClinicSyncServer.Common.Abstracciones.RefreshTokenGenerado&gt;" />
    public record RefreshTokenGenerado(string Token, byte[] Hash);

    /// <summary>
    /// ITokenService define la interfaz de la clase encargada del manejo de JSON Web Tokens (JWT).
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Generars the acceso JWT.
        /// </summary>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="idSesion">The identifier sesion.</param>
        /// <param name="perfil">The perfil.</param>
        /// <returns></returns>
        string GenerarAccesoJwt(int idUsuario, Guid idSesion, PermisosUsuario perfil);
        /// <summary>
        /// Generars the desafio2fa.
        /// </summary>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <returns></returns>
        string GenerarDesafio2fa(int idUsuario);
        /// <summary>
        /// Leers the desafio2fa.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        int? LeerDesafio2fa(string token);
        /// <summary>
        /// Generars the refresh token.
        /// </summary>
        /// <returns></returns>
        RefreshTokenGenerado GenerarRefreshToken();
        /// <summary>
        /// Hashes the refresh token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        byte[]? HashRefreshToken(string token);
    }
}
