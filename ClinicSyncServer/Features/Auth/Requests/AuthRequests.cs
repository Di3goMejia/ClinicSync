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

    public record OlvideContrasenaRequest(
    [property: Required, EmailAddress]
    string Identificador
);

    public record RestablecerContrasenaRequest(
        [property: Required, EmailAddress]
    string Identificador,

        [property: Required]
    [property: RegularExpression(
        @"^[0-9]{6}$",
        ErrorMessage = "El código debe tener 6 dígitos.")]
    string Codigo,

        [property: Required]
    [property: MinLength(8)]
    string NuevaPassword,

        [property: Required]
    string ConfirmarPassword
    );
}
