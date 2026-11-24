
namespace ClinicSyncServer.Features.Pacientes.Responses
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class PacienteListado
    {
        /// <summary>
        /// Gets or sets the identifier paciente.
        /// </summary>
        /// <value>
        /// The identifier paciente.
        /// </value>
        public int IdPaciente { get; set; }

        /// <summary>
        /// Gets or sets the identidad.
        /// </summary>
        /// <value>
        /// The identidad.
        /// </value>
        public string Identidad { get; set; } = "";

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
        /// Gets or sets the fecha nacimiento.
        /// </summary>
        /// <value>
        /// The fecha nacimiento.
        /// </value>
        public DateTime FechaNacimiento { get; set; }

        /// <summary>
        /// Gets or sets the genero.
        /// </summary>
        /// <value>
        /// The genero.
        /// </value>
        public string Genero { get; set; } = "";

        /// <summary>
        /// Gets or sets the telefono.
        /// </summary>
        /// <value>
        /// The telefono.
        /// </value>
        public string Telefono { get; set; } = "";

        /// <summary>
        /// Gets or sets the fecha registro.
        /// </summary>
        /// <value>
        /// The fecha registro.
        /// </value>
        public DateTime FechaRegistro { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="PacienteListado"/> is activo.
        /// </summary>
        /// <value>
        ///   <c>true</c> if activo; otherwise, <c>false</c>.
        /// </value>
        public bool Activo { get; set; }
    }
}
