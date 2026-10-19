using System.ComponentModel.DataAnnotations;

namespace ClinicSyncServer.Features.Auth.Requests
{
    public record LoginRequest(
        [property: Required] string Identificador,
        [property: Required] string Password);

    public record VerificarSegundoFactorRequest(
        [property: Required] string Desafio,
        [property: Required] string Prueba);

    public record RefreshRequest([property: Required] string RefreshToken);

    public record ActivarCuentaRequest(
        [property: Required] string Token,
        [property: Required] string NuevaPassword);

    public record OlvideContrasenaRequest([property: Required] string Identificador);

    public record RestablecerContrasenaRequest(
        [property: Required] string Token,
        [property: Required] string NuevaPassword);
}
