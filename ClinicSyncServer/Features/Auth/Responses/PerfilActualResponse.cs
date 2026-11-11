
namespace ClinicSyncServer.Features.Auth.Responses
{
    public record PerfilActualResponse(
        string NombreUsuario,
        string Rol,
        bool TieneFinanzas,
        int IdEmpleado,
        string NombreEmpleado,
        string Identidad,
        string Nombres,
        string Apellidos,
        DateOnly FechaNacimiento,
        char Genero,
        string Telefono,
        string? CorreoContacto,
        string? ImagenPerfil
    );
}
