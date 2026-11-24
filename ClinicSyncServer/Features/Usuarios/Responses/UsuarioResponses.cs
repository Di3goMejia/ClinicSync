namespace ClinicSyncServer.Features.Usuarios.Responses
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.IEquatable&lt;ClinicSyncServer.Features.Usuarios.Responses.CrearUsuarioResponse&gt;" />
    public record CrearUsuarioResponse(int IdUsuario, int IdEmpleado);

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.IEquatable&lt;ClinicSyncServer.Features.Usuarios.Responses.UsuarioDetalleResponse&gt;" />
    public record UsuarioDetalleResponse(
        int IdUsuario, int IdEmpleado, string Identidad, string Nombres, string Apellidos,
        DateOnly FechaNacimiento, char Genero, string Telefono, string? CorreoContacto,
        string? ImagenPerfil, string NombreUsuario, string CorreoLogin, int IdRol,
        string Estado, DateTime FechaRegistro
    );
}
