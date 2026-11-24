namespace ClinicSyncServer.Features.Sesiones.Projections
{
    /// <summary>
    /// 
    /// </summary>
    public class SesionRotada
    {
        /// <summary>
        /// Gets or sets the identifier sesion.
        /// </summary>
        /// <value>
        /// The identifier sesion.
        /// </value>
        public Guid IdSesion { get; set; }
        /// <summary>
        /// Gets or sets the identifier usuario.
        /// </summary>
        /// <value>
        /// The identifier usuario.
        /// </value>
        public int IdUsuario { get; set; }
    }
}
