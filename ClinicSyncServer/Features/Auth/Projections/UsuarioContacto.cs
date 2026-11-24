namespace ClinicSyncServer.Features.Auth.Projections
{
    /// <summary>
    /// 
    /// </summary>
    public class UsuarioContacto
    {
        /// <summary>
        /// Gets or sets the identifier usuario.
        /// </summary>
        /// <value>
        /// The identifier usuario.
        /// </value>
        public int IdUsuario { get; set; }
        /// <summary>
        /// Gets or sets the correo.
        /// </summary>
        /// <value>
        /// The correo.
        /// </value>
        public string Correo { get; set; } = string.Empty;
    }
}
