using ClinicSyncServer.Features.Auth.Requests;
using ClinicSyncServer.Features.Auth.Responses;

namespace ClinicSyncServer.Features.Auth
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.IEquatable&lt;ClinicSyncServer.Features.Auth.DatosCliente&gt;" />
    public record DatosCliente(string? Ip, string Equipo);

    /// <summary>
    /// 
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Logins the asynchronous.
        /// </summary>
        /// <param name="req">The req.</param>
        /// <param name="cliente">The cliente.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<LoginResponse> LoginAsync(LoginRequest req, DatosCliente cliente, CancellationToken ct = default);
        /// <summary>
        /// Verificars the segundo factor asynchronous.
        /// </summary>
        /// <param name="req">The req.</param>
        /// <param name="cliente">The cliente.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<LoginResponse> VerificarSegundoFactorAsync(VerificarSegundoFactorRequest req, DatosCliente cliente, CancellationToken ct = default);
        /// <summary>
        /// Renovars the asynchronous.
        /// </summary>
        /// <param name="refreshToken">The refresh token.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<LoginResponse> RenovarAsync(string refreshToken, CancellationToken ct = default);
        /// <summary>
        /// Cerrars the sesion asynchronous.
        /// </summary>
        /// <param name="idSesion">The identifier sesion.</param>
        /// <returns></returns>
        Task CerrarSesionAsync(Guid idSesion);
        /// <summary>
        /// Validars the token activacion asynchronous.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task ValidarTokenActivacionAsync(string token, CancellationToken ct = default);
        /// <summary>
        /// Activars the cuenta asynchronous.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="nuevaPassword">The nueva password.</param>
        /// <param name="confirmarPassword">The confirmar password.</param>
        /// <returns></returns>
        Task ActivarCuentaAsync(string token, string nuevaPassword, string? confirmarPassword = null);
        /// <summary>
        /// Solicitars the restablecimiento asynchronous.
        /// </summary>
        /// <param name="identificador">The identificador.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task SolicitarRestablecimientoAsync(string identificador, CancellationToken ct = default);
        /// <summary>
        /// Restablecers the contrasena asynchronous.
        /// </summary>
        /// <param name="identificador">The identificador.</param>
        /// <param name="codigo">The codigo.</param>
        /// <param name="nuevaPassword">The nueva password.</param>
        /// <param name="confirmarPassword">The confirmar password.</param>
        /// <returns></returns>
        Task RestablecerContrasenaAsync(
        string identificador,
        string codigo,
        string nuevaPassword,
        string confirmarPassword);

        /// <summary>
        /// Cambiars the mi contrasena asynchronous.
        /// </summary>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="idSesionActual">The identifier sesion actual.</param>
        /// <param name="actual">The actual.</param>
        /// <param name="nueva">The nueva.</param>
        /// <param name="confirmacion">The confirmacion.</param>
        /// <returns></returns>
        Task CambiarMiContrasenaAsync(
            int idUsuario,
            Guid idSesionActual,
            string actual,
            string nueva,
            string confirmacion);
    }
}
