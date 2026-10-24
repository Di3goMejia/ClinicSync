namespace ClinicSyncServer.Features.Auth.Responses
{
    public record PerfilActualResponse(
        string NombreUsuario,
        string Rol,
        bool TieneFinanzas
    );
}
