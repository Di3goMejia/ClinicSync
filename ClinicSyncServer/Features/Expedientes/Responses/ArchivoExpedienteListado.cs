namespace ClinicSyncServer.Features.Expedientes.Responses
{
    /* Detalles:
	   - IdExpediente e IdVisita se excluyen; son implícitos.
	   - NombreArchivo aparece solo como Nombre.
	   - HashArchivo se excluye. Se envía mediante header.
    */
    /// <summary>
    /// 
    /// </summary>
    public class ArchivoExpedienteListado
    {

        /// <summary>
        /// Gets or sets the identifier archivo.
        /// </summary>
        /// <value>
        /// The identifier archivo.
        /// </value>
        public int IdArchivo { get; set; }
        /// <summary>
        /// Gets or sets the identifier usuario.
        /// </summary>
        /// <value>
        /// The identifier usuario.
        /// </value>
        public int IdUsuario { get; set; }
        /// <summary>
        /// Gets or sets the nombre.
        /// </summary>
        /// <value>
        /// The nombre.
        /// </value>
        public string Nombre { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the clave.
        /// </summary>
        /// <value>
        /// The clave.
        /// </value>
        public string Clave { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the descripcion.
        /// </summary>
        /// <value>
        /// The descripcion.
        /// </value>
        public string? Descripcion { get; set; }
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
        /// Gets or sets a value indicating whether this <see cref="ArchivoExpedienteListado"/> is activo.
        /// </summary>
        /// <value>
        ///   <c>true</c> if activo; otherwise, <c>false</c>.
        /// </value>
        public bool Activo { get; set; }
    }
}
