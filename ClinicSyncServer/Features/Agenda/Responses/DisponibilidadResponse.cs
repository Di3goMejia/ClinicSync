namespace ClinicSyncServer.Features.Agenda.Responses
{
    // Un evento que choca con el horario consultado.
    public sealed class ConflictoAgendaResponse
    {
        public int IdEvento { get; set; }
        public string Tipo { get; set; } = "";
        public string Estado { get; set; } = "";
        public string Titulo { get; set; } = "";
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFinal { get; set; }
    }

    // Resultado de consultar si un horario está libre.
    public sealed class DisponibilidadResponse
    {
        public bool Disponible { get; set; }
        public IReadOnlyList<ConflictoAgendaResponse> Conflictos { get; set; } = [];
    }
}