
using System;

namespace ClinicSyncServer.Features.Expedientes.Responses
{
    public class VisitaListado
    {
        public int IdVisita { get; set; }

        public int IdExpediente { get; set; }

        public int IdEmpleado { get; set; }

        public DateTime Fecha { get; set; }

        public string Motivo { get; set; } = "";

        public string? Notas { get; set; }

        public bool Activo { get; set; }
    }
}
