namespace ClinicSyncServer.Features.Expedientes.Projections
{
    /// <summary>
    /// 
    /// </summary>
    public class ArchivoExpediente
    {

        /// <summary>
        /// Gets or sets the identifier archivo.
        /// </summary>
        /// <value>
        /// The identifier archivo.
        /// </value>
        public Guid IdArchivo { get; set; }
        /// <summary>
        /// Gets or sets the identifier expediente.
        /// </summary>
        /// <value>
        /// The identifier expediente.
        /// </value>
        public int IdExpediente { get; set; }
        /// <summary>
        /// Gets or sets the identifier visita.
        /// </summary>
        /// <value>
        /// The identifier visita.
        /// </value>
        public int? IdVisita { get; set; }
        /// <summary>
        /// Gets or sets the identifier usuario.
        /// </summary>
        /// <value>
        /// The identifier usuario.
        /// </value>
        public int IdUsuario { get; set; }
        /// <summary>
        /// Gets or sets the nombre archivo.
        /// </summary>
        /// <value>
        /// The nombre archivo.
        /// </value>
        public string NombreArchivo { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the clave almacen.
        /// </summary>
        /// <value>
        /// The clave almacen.
        /// </value>
        public string ClaveAlmacen { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the descripcion.
        /// </summary>
        /// <value>
        /// The descripcion.
        /// </value>
        public string? Descripcion { get; set; }
        /// <summary>
        /// Gets or sets the hash archivo.
        /// </summary>
        /// <value>
        /// The hash archivo.
        /// </value>
        public string HashArchivo { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the tipo MIME.
        /// </summary>
        /// <value>
        /// The tipo MIME.
        /// </value>
        public string TipoMime { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the tamano bytes.
        /// </summary>
        /// <value>
        /// The tamano bytes.
        /// </value>
        public int TamanoBytes { get; set; }
        /// <summary>
        /// Gets or sets the fecha subida.
        /// </summary>
        /// <value>
        /// The fecha subida.
        /// </value>
        public DateTime FechaSubida { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ArchivoExpediente"/> is activo.
        /// </summary>
        /// <value>
        ///   <c>true</c> if activo; otherwise, <c>false</c>.
        /// </value>
        public bool Activo { get; set; }
    }
}
