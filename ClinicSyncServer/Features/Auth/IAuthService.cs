using ClinicSyncServer.Features.Auth.Requests;
using ClinicSyncServer.Features.Auth.Responses;

namespace ClinicSyncServer.Features.Auth
{
    public record DatosCliente(string? Ip, string Equipo);

    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest req, DatosCliente cliente, CancellationToken ct = default);
        Task<LoginResponse> VerificarSegundoFactorAsync(VerificarSegundoFactorRequest req, DatosCliente cliente, CancellationToken ct = default);
        Task<LoginResponse> RenovarAsync(string refreshToken, CancellationToken ct = default);
        Task CerrarSesionAsync(Guid idSesion);
        Task ActivarCuentaAsync(string token, string nuevaPassword);
        Task SolicitarRestablecimientoAsync(string identificador, CancellationToken ct = default);
        Task RestablecerContrasenaAsync(
        string identificador,
        string codigo,
        string nuevaPassword,
        string confirmarPassword);
    }
}
