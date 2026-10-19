using System.ComponentModel.DataAnnotations;

namespace ClinicSyncServer.Features.Auth.Responses
{

    public record LoginResponse(
        bool Requiere2fa,
        string? Desafio,
        string? AccessToken,
        string? RefreshToken);
}
