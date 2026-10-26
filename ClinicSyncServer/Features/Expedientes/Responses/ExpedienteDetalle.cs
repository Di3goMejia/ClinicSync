
using System;

namespace ClinicSyncServer.Features.Expedientes.Responses
{
    public class ExpedienteDetalle
    {
        public int IdExpediente { get; set; }
        public int IdPaciente { get; set; }
        public string Codigo { get; set; } = "";
        public string? Notas { get; set; }
        public DateTime FechaCreacion { get; set; }
        public bool Activo { get; set; }
    }
}
