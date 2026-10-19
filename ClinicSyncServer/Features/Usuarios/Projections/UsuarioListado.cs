namespace ClinicSyncServer.Features.Usuarios.Projections
{
    /// <summary>
    /// UsuarioListado es una proyección que contiene los datos del usuario y del empleado
    /// como aparecen en el DataGrid del cliente.
    /// </summary>
    public class UsuarioListado
    {
        public int IdUsuario { get; set; }
        public int IdEmpleado { get; set; }
        public int IdRol { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string? ImagenPerfil { get; set; }
        public DateTime UltimoAcceso { get; set; }
        public string Estado { get; set; } = "PENDIENTE";
    }
}
