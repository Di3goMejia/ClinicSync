
namespace ClinicSyncServer.Features.Expedientes.Responses
{
    /// <summary>
    /// 
    /// </summary>
    public class ExpedienteDetalle
    {
        /// <summary>
        /// Gets or sets the identifier expediente.
        /// </summary>
        /// <value>
        /// The identifier expediente.
        /// </value>
        public int IdExpediente { get; set; }
        /// <summary>
        /// Gets or sets the identifier paciente.
        /// </summary>
        /// <value>
        /// The identifier paciente.
        /// </value>
        public int IdPaciente { get; set; }
        /// <summary>
        /// Gets or sets the codigo.
        /// </summary>
        /// <value>
        /// The codigo.
        /// </value>
        public string Codigo { get; set; } = "";
        /// <summary>
        /// Gets or sets the notas.
        /// </summary>
        /// <value>
        /// The notas.
        /// </value>
        public string? Notas { get; set; }
        /// <summary>
        /// Gets or sets the fecha creacion.
        /// </summary>
        /// <value>
        /// The fecha creacion.
        /// </value>
        public DateTime FechaCreacion { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ExpedienteDetalle"/> is activo.
        /// </summary>
        /// <value>
        ///   <c>true</c> if activo; otherwise, <c>false</c>.
        /// </value>
        public bool Activo { get; set; }
    }
}
