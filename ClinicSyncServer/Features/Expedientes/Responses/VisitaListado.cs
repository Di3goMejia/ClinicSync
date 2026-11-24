
using System;

namespace ClinicSyncServer.Features.Expedientes.Responses
{
    /// <summary>
    /// 
    /// </summary>
    public class VisitaListado
    {
        /// <summary>
        /// Gets or sets the identifier visita.
        /// </summary>
        /// <value>
        /// The identifier visita.
        /// </value>
        public int IdVisita { get; set; }

        /// <summary>
        /// Gets or sets the identifier expediente.
        /// </summary>
        /// <value>
        /// The identifier expediente.
        /// </value>
        public int IdExpediente { get; set; }

        /// <summary>
        /// Gets or sets the identifier empleado.
        /// </summary>
        /// <value>
        /// The identifier empleado.
        /// </value>
        public int IdEmpleado { get; set; }

        /// <summary>
        /// Gets or sets the fecha.
        /// </summary>
        /// <value>
        /// The fecha.
        /// </value>
        public DateTime Fecha { get; set; }

        /// <summary>
        /// Gets or sets the motivo.
        /// </summary>
        /// <value>
        /// The motivo.
        /// </value>
        public string Motivo { get; set; } = "";

        /// <summary>
        /// Gets or sets the notas.
        /// </summary>
        /// <value>
        /// The notas.
        /// </value>
        public string? Notas { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="VisitaListado" /> is activo.
        /// </summary>
        /// <value>
        ///   <c>true</c> if activo; otherwise, <c>false</c>.
        /// </value>
        public bool Activo { get; set; }
    }
}
