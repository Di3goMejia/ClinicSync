
using System;

namespace ClinicSyncServer.Features.Expedientes.Requests
{
    public class ModificarVisitaRequest
    {
        public int IdEmpleado { get; set; }

        public DateTime Fecha { get; set; }

        public string Motivo { get; set; } = "";

        public string? Notas { get; set; }
    }
}
