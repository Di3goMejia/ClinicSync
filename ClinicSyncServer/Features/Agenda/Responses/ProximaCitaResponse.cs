namespace ClinicSyncServer.Features.Agenda.Responses
{
    public sealed class ProximaCitaResponse
    {
        public int IdEvento { get; set; }
        public int IdEmpleado { get; set; }
        public int? IdPaciente { get; set; }
        public string? Paciente { get; set; }

        public string Estado { get; set; } = "";
        public string Titulo { get; set; } = "";

        public DateTime FechaInicio { get; set; }
        public DateTime FechaFinal { get; set; }

        public string? Color { get; set; }
        public string? Direccion { get; set; }
        public bool TodoElDia { get; set; }
    }
}