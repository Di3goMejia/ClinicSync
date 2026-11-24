namespace ClinicSyncServer.Features.Tratamientos
{
    /// <summary>
    /// 
    /// </summary>
    public class Tratamiento
    {
        /// <summary>
        /// Gets or sets the identifier tratamiento.
        /// </summary>
        /// <value>
        /// The identifier tratamiento.
        /// </value>
        public int IdTratamiento { get; set; }

        /// <summary>
        /// Gets or sets the nombre.
        /// </summary>
        /// <value>
        /// The nombre.
        /// </value>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the precio.
        /// </summary>
        /// <value>
        /// The precio.
        /// </value>
        public decimal Precio { get; set; }

        /// <summary>
        /// Gets or sets the descripcion.
        /// </summary>
        /// <value>
        /// The descripcion.
        /// </value>
        public string? Descripcion { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Tratamiento"/> is activo.
        /// </summary>
        /// <value>
        ///   <c>true</c> if activo; otherwise, <c>false</c>.
        /// </value>
        public bool Activo { get; set; }
    }
}
