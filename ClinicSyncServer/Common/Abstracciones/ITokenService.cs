using ClinicSyncServer.Features.Permisos.Projections;

namespace ClinicSyncServer.Common.Abstracciones
{
    public record RefreshTokenGenerado(string Token, byte[] Hash);

    /// <summary>
    /// ITokenService define la interfaz de la clase encargada del manejo de JSON Web Tokens (JWT).
    /// </summary>
    public interface ITokenService
    {
        string GenerarAccesoJwt(int idUsuario, Guid idSesion, PermisosUsuario perfil);
        string GenerarDesafio2fa(int idUsuario);
        int? LeerDesafio2fa(string token);
        RefreshTokenGenerado GenerarRefreshToken();
        byte[]? HashRefreshToken(string token);
    }
}
