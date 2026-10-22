using System.Net;

namespace ClinicSyncDesktop.Api
{
    public record LoginResponse(bool Requiere2fa, string? Desafio, string? AccessToken, string? RefreshToken);

    public record ProblemaApi(string? Title, string? Detail, Dictionary<string, string[]>? Errors);

    public class ApiException(string mensaje, System.Net.HttpStatusCode estado) : Exception(mensaje)
    {
        public HttpStatusCode Estado { get; } = estado;
    }

    public class SesionExpiradaException() : Exception("La sesión expiró. Inicia sesión de nuevo.");
}
