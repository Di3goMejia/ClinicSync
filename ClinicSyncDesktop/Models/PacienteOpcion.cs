namespace ClinicSyncDesktop.Models
{
    // Paciente tal como se muestra en el selector del formulario de citas.
    /// <summary>
    /// 
    /// </summary>
    public class PacienteOpcion
    {
        /// <summary>
        /// Gets or sets the identifier paciente.
        /// </summary>
        /// <value>
        /// The identifier paciente.
        /// </value>
        public int IdPaciente { get; set; }
        /// <summary>
        /// Gets or sets the nombres.
        /// </summary>
        /// <value>
        /// The nombres.
        /// </value>
        public string Nombres { get; set; } = "";
        /// <summary>
        /// Gets or sets the apellidos.
        /// </summary>
        /// <value>
        /// The apellidos.
        /// </value>
        public string Apellidos { get; set; } = "";
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="PacienteOpcion"/> is activo.
        /// </summary>
        /// <value>
        ///   <c>true</c> if activo; otherwise, <c>false</c>.
        /// </value>
        public bool Activo { get; set; }

        // Texto que se ve en el selector.
        /// <summary>
        /// Gets the nombre completo.
        /// </summary>
        /// <value>
        /// The nombre completo.
        /// </value>
        public string NombreCompleto => $"{Nombres} {Apellidos}".Trim();
    }
}