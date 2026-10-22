namespace ClinicSyncServer.Features.Usuarios.Responses
{
    public record CrearUsuarioResponse(int IdUsuario, int IdEmpleado);

    public record UsuarioDetalleResponse(
        int IdUsuario, int IdEmpleado, string Identidad, string Nombres, string Apellidos,
        DateOnly FechaNacimiento, char Genero, string Telefono, string? CorreoContacto,
        string? ImagenPerfil, string NombreUsuario, string CorreoLogin, int IdRol,
        string Estado, DateTime FechaRegistro
    );

    public record UsuarioListadoResponse(
        int IdUsuario, int IdEmpleado, int IdRol, string Nombres, 
        string Apellidos, string? ImagenPerfil, DateTime UltimoAcceso, 
        string Estado
    );
}
