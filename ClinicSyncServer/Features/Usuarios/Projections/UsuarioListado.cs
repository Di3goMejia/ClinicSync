namespace ClinicSyncServer.Features.Usuarios.Projections
{
    /// <summary>
    /// UsuarioListado es una proyección que contiene los datos del usuario y del empleado
    /// como aparecen en el DataGrid del cliente.
    /// </summary>
    public class UsuarioListado
    {
        /// <summary>
        /// Gets or sets the identifier usuario.
        /// </summary>
        /// <value>
        /// The identifier usuario.
        /// </value>
        public int IdUsuario { get; set; }
        /// <summary>
        /// Gets or sets the identifier empleado.
        /// </summary>
        /// <value>
        /// The identifier empleado.
        /// </value>
        public int IdEmpleado { get; set; }
        /// <summary>
        /// Gets or sets the identifier rol.
        /// </summary>
        /// <value>
        /// The identifier rol.
        /// </value>
        public int IdRol { get; set; }
        /// <summary>
        /// Gets or sets the nombre rol.
        /// </summary>
        /// <value>
        /// The nombre rol.
        /// </value>
        public string NombreRol { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the nombres.
        /// </summary>
        /// <value>
        /// The nombres.
        /// </value>
        public string Nombres { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the apellidos.
        /// </summary>
        /// <value>
        /// The apellidos.
        /// </value>
        public string Apellidos { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the imagen perfil.
        /// </summary>
        /// <value>
        /// The imagen perfil.
        /// </value>
        public string? ImagenPerfil { get; set; }
        /// <summary>
        /// Gets or sets the ultimo acceso.
        /// </summary>
        /// <value>
        /// The ultimo acceso.
        /// </value>
        public DateTime? UltimoAcceso { get; set; }
        /// <summary>
        /// Gets or sets the ultimo login.
        /// </summary>
        /// <value>
        /// The ultimo login.
        /// </value>
        public DateTime? UltimoLogin { get => UltimoAcceso; set => UltimoAcceso = value; }
        /// <summary>
        /// Gets or sets the estado.
        /// </summary>
        /// <value>
        /// The estado.
        /// </value>
        public string Estado { get; set; } = "PENDIENTE";
    }
}
