namespace ClinicSyncServer.Features.Auth.Projections
{
    /// <summary>
    /// 
    /// </summary>
    public class UsuarioAuth
    {
        /// <summary>
        /// Gets or sets the identifier usuario.
        /// </summary>
        /// <value>
        /// The identifier usuario.
        /// </value>
        public int IdUsuario { get; set; }
        /// <summary>
        /// Gets or sets the identifier rol.
        /// </summary>
        /// <value>
        /// The identifier rol.
        /// </value>
        public int IdRol { get; set; }
        /// <summary>
        /// Gets or sets the clave hash.
        /// </summary>
        /// <value>
        /// The clave hash.
        /// </value>
        public string ClaveHash { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the estado.
        /// </summary>
        /// <value>
        /// The estado.
        /// </value>
        public string Estado { get; set; } = "PENDIENTE";
        /// <summary>
        /// Gets or sets a value indicating whether [requiere2 fa].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [requiere2 fa]; otherwise, <c>false</c>.
        /// </value>
        public bool Requiere2FA { get; set; }
    }
}
