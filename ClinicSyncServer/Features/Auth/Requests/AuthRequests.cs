using System.ComponentModel.DataAnnotations;

namespace ClinicSyncServer.Features.Auth.Requests
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.IEquatable&lt;ClinicSyncServer.Features.Auth.Requests.LoginRequest&gt;" />
    public record LoginRequest(
        [property: Required] string Identificador,
        [property: Required] string Password);

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.IEquatable&lt;ClinicSyncServer.Features.Auth.Requests.VerificarSegundoFactorRequest&gt;" />
    public record VerificarSegundoFactorRequest(
        [property: Required] string Desafio,
        [property: Required] string Prueba);

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.IEquatable&lt;ClinicSyncServer.Features.Auth.Requests.RefreshRequest&gt;" />
    public record RefreshRequest([property: Required] string RefreshToken);

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.IEquatable&lt;ClinicSyncServer.Features.Auth.Requests.ValidarTokenRequest&gt;" />
    public record ValidarTokenRequest([property: Required] string Token);

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.IEquatable&lt;ClinicSyncServer.Features.Auth.Requests.ActivarCuentaRequest&gt;" />
    public record ActivarCuentaRequest(
        [property: Required] string Token,
        [property: Required] string NuevaPassword,
        string? ConfirmarPassword = null);

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.IEquatable&lt;ClinicSyncServer.Features.Auth.Requests.OlvideContrasenaRequest&gt;" />
    public record OlvideContrasenaRequest(
    [property: Required, EmailAddress]
    string Identificador
);

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.IEquatable&lt;ClinicSyncServer.Features.Auth.Requests.RestablecerContrasenaRequest&gt;" />
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

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.IEquatable&lt;ClinicSyncServer.Features.Auth.Requests.CambiarMiContrasenaRequest&gt;" />
    public record CambiarMiContrasenaRequest(
        string ContrasenaActual,
        string NuevaContrasena,
        string ConfirmarContrasena
    );
}
