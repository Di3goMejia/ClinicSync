using System.ComponentModel.DataAnnotations;

namespace ClinicSyncServer.Features.Auth.Responses
{

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.IEquatable&lt;ClinicSyncServer.Features.Auth.Responses.LoginResponse&gt;" />
    public record LoginResponse(
        bool Requiere2fa,
        string? Desafio,
        string? AccessToken,
        string? RefreshToken);
}
