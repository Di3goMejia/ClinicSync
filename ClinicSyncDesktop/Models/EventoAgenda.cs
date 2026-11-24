using System;
using System.Collections.Generic;

namespace ClinicSyncDesktop.Models
{
    // Una cita o bloqueo del calendario (GET /api/agenda).
    /// <summary>
    /// 
    /// </summary>
    public class EventoAgenda
    {
        /// <summary>
        /// Gets or sets the identifier evento.
        /// </summary>
        /// <value>
        /// The identifier evento.
        /// </value>
        public int IdEvento { get; set; }
        /// <summary>
        /// Gets or sets the identifier empleado.
        /// </summary>
        /// <value>
        /// The identifier empleado.
        /// </value>
        public int IdEmpleado { get; set; }
        /// <summary>
        /// Gets or sets the empleado.
        /// </summary>
        /// <value>
        /// The empleado.
        /// </value>
        public string Empleado { get; set; } = "";
        /// <summary>
        /// Gets or sets the identifier paciente.
        /// </summary>
        /// <value>
        /// The identifier paciente.
        /// </value>
        public int? IdPaciente { get; set; }
        /// <summary>
        /// Gets or sets the paciente.
        /// </summary>
        /// <value>
        /// The paciente.
        /// </value>
        public string? Paciente { get; set; }
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
        /// Gets or sets the notas.
        /// </summary>
        /// <value>
        /// The notas.
        /// </value>
        public string? Notas { get; set; }
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
        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        /// <value>
        /// The color.
        /// </value>
        public string? Color { get; set; }
        /// <summary>
        /// Gets or sets the direccion.
        /// </summary>
        /// <value>
        /// The direccion.
        /// </value>
        public string? Direccion { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [todo el dia].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [todo el dia]; otherwise, <c>false</c>.
        /// </value>
        public bool TodoElDia { get; set; }
    }

    // Una fila del panel "Próximas citas" (GET /api/agenda/proximas).
    /// <summary>
    /// 
    /// </summary>
    public class ProximaCita
    {
        /// <summary>
        /// Gets or sets the identifier evento.
        /// </summary>
        /// <value>
        /// The identifier evento.
        /// </value>
        public int IdEvento { get; set; }
        /// <summary>
        /// Gets or sets the identifier paciente.
        /// </summary>
        /// <value>
        /// The identifier paciente.
        /// </value>
        public int? IdPaciente { get; set; }
        /// <summary>
        /// Gets or sets the paciente.
        /// </summary>
        /// <value>
        /// The paciente.
        /// </value>
        public string? Paciente { get; set; }
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
        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        /// <value>
        /// The color.
        /// </value>
        public string? Color { get; set; }
        /// <summary>
        /// Gets or sets the direccion.
        /// </summary>
        /// <value>
        /// The direccion.
        /// </value>
        public string? Direccion { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [todo el dia].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [todo el dia]; otherwise, <c>false</c>.
        /// </value>
        public bool TodoElDia { get; set; }
    }

    // Respuesta de GET /api/agenda/disponibilidad.
    /// <summary>
    /// 
    /// </summary>
    public class DisponibilidadAgenda
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DisponibilidadAgenda"/> is disponible.
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
        public List<ConflictoAgenda> Conflictos { get; set; } = new();
    }

    /// <summary>
    /// 
    /// </summary>
    public class ConflictoAgenda
    {
        /// <summary>
        /// Gets or sets the identifier evento.
        /// </summary>
        /// <value>
        /// The identifier evento.
        /// </value>
        public int IdEvento { get; set; }
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
}