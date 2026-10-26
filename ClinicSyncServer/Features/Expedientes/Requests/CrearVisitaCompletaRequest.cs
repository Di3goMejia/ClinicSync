using System;
using System.Collections.Generic;

namespace ClinicSyncServer.Features.Expedientes.Requests
{
    public class CrearVisitaCompletaRequest
    {
        public int IdPaciente { get; set; }

        public int IdEmpleado { get; set; }

        public DateTime Fecha { get; set; }

        public string Motivo { get; set; } = "";

        public string? Notas { get; set; }

        public List<TratamientoSeleccionadoRequest> Tratamientos { get; set; } = new();
    }
}