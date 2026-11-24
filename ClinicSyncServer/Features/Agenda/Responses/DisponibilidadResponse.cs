namespace ClinicSyncServer.Features.Agenda.Responses
{
    // Un evento que choca con el horario consultado.
    /// <summary>
    /// 
    /// </summary>
    public sealed class ConflictoAgendaResponse
    {
        /// <summary>
        /// Gets or sets the identifier evento.
        /// </summary>
        /// <value>
        /// The identifier evento.
        /// </value>
        public int IdEvento { get; set; }
        /// <summary>
        /// Gets or sets the tipo.
        /// </summary>
        /// <value>
        /// The tipo.
        /// </value>
        public string Tipo { get; set; } = "";
        /// <summary>
        /// Gets or sets the estado.
        /// </summary>
        /// <value>
        /// The estado.
        /// </value>
        public string Estado { get; set; } = "";
        /// <summary>
        /// Gets or sets the titulo.
        /// </summary>
        /// <value>
        /// The titulo.
        /// </value>
        public string Titulo { get; set; } = "";
        /// <summary>
        /// Gets or sets the fecha inicio.
        /// </summary>
        /// <value>
        /// The fecha inicio.
        /// </value>
        public DateTime FechaInicio { get; set; }
        /// <summary>
        /// Gets or sets the fecha final.
        /// </summary>
        /// <value>
        /// The fecha final.
        /// </value>
        public DateTime FechaFinal { get; set; }
    }

    // Resultado de consultar si un horario está libre.
    /// <summary>
    /// 
    /// </summary>
    public sealed class DisponibilidadResponse
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DisponibilidadResponse"/> is disponible.
        /// </summary>
        /// <value>
        ///   <c>true</c> if disponible; otherwise, <c>false</c>.
        /// </value>
        public bool Disponible { get; set; }
        /// <summary>
        /// Gets or sets the conflictos.
        /// </summary>
        /// <value>
        /// The conflictos.
        /// </value>
        public IReadOnlyList<ConflictoAgendaResponse> Conflictos { get; set; } = [];
    }
}