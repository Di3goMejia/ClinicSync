using System;
using System.Collections.Generic;

namespace ClinicSyncDesktop.Models
{
    // Una cita o bloqueo del calendario (GET /api/agenda).
    public class EventoAgenda
    {
        public int IdEvento { get; set; }
        public int IdEmpleado { get; set; }
        public string Empleado { get; set; } = "";
        public int? IdPaciente { get; set; }
        public string? Paciente { get; set; }
        public string Tipo { get; set; } = "";
        public string Estado { get; set; } = "";
        public string Titulo { get; set; } = "";
        public string? Notas { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFinal { get; set; }
        public string? Color { get; set; }
        public string? Direccion { get; set; }
        public bool TodoElDia { get; set; }
    }

    // Una fila del panel "Próximas citas" (GET /api/agenda/proximas).
    public class ProximaCita
    {
        public int IdEvento { get; set; }
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

    // Respuesta de GET /api/agenda/disponibilidad.
    public class DisponibilidadAgenda
    {
        public bool Disponible { get; set; }
        public List<ConflictoAgenda> Conflictos { get; set; } = new();
    }

    public class ConflictoAgenda
    {
        public int IdEvento { get; set; }
        public string Titulo { get; set; } = "";
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFinal { get; set; }
    }
}