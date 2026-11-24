namespace ClinicSyncServer.Features.Usuarios.Projections
{
    /// <summary>
    /// UsuarioDetalle es una proyección que contiene los datos del usuario y del empleado
    /// como aparece en la configuración por cada usuario.
    /// </summary>
    public class UsuarioDetalle
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
        /// Gets or sets the identidad.
        /// </summary>
        /// <value>
        /// The identidad.
        /// </value>
        public string Identidad { get; set; } = string.Empty;
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
        /// Gets or sets the fecha nacimiento.
        /// </summary>
        /// <value>
        /// The fecha nacimiento.
        /// </value>
        public DateOnly FechaNacimiento { get; set; }
        /// <summary>
        /// Gets or sets the genero.
        /// </summary>
        /// <value>
        /// The genero.
        /// </value>
        public char Genero { get; set; }
        /// <summary>
        /// Gets or sets the telefono.
        /// </summary>
        /// <value>
        /// The telefono.
        /// </value>
        public string Telefono { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the correo contacto.
        /// </summary>
        /// <value>
        /// The correo contacto.
        /// </value>
        public string? CorreoContacto { get; set; }
        /// <summary>
        /// Gets or sets the imagen perfil.
        /// </summary>
        /// <value>
        /// The imagen perfil.
        /// </value>
        public string? ImagenPerfil { get; set; }
        /// <summary>
        /// Gets or sets the nombre usuario.
        /// </summary>
        /// <value>
        /// The nombre usuario.
        /// </value>
        public string NombreUsuario { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the correo login.
        /// </summary>
        /// <value>
        /// The correo login.
        /// </value>
        public string CorreoLogin { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the identifier rol.
        /// </summary>
        /// <value>
        /// The identifier rol.
        /// </value>
        public int IdRol { get; set; }
        /// <summary>
        /// Gets or sets the estado.
        /// </summary>
        /// <value>
        /// The estado.
        /// </value>
        public string Estado { get; set; } = "PENDIENTE";
        /// <summary>
        /// Gets or sets the fecha registro.
        /// </summary>
        /// <value>
        /// The fecha registro.
        /// </value>
        public DateTime FechaRegistro { get; set; }
    }
}
