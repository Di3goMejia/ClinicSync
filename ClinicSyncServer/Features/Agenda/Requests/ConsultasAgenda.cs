using System.ComponentModel.DataAnnotations;

namespace ClinicSyncServer.Features.Agenda.Requests
{
    // GET /api/agenda  (calendario Día / Semana / Mes)
    /// <summary>
    /// 
    /// </summary>
    public sealed class ListarAgendaQuery
    {
        /// <summary>
        /// Gets or sets the desde.
        /// </summary>
        /// <value>
        /// The desde.
        /// </value>
        [Required(ErrorMessage = "Debe indicar la fecha de inicio del rango.")]
        public DateTime? Desde { get; set; }

        /// <summary>
        /// Gets or sets the hasta.
        /// </summary>
        /// <value>
        /// The hasta.
        /// </value>
        [Required(ErrorMessage = "Debe indicar la fecha final del rango.")]
        public DateTime? Hasta { get; set; }

        // Opcionales: si no se envían, se muestran todos.
        /// <summary>
        /// Gets or sets the identifier empleado.
        /// </summary>
        /// <value>
        /// The identifier empleado.
        /// </value>
        public int? IdEmpleado { get; set; }
        /// <summary>
        /// Gets or sets the identifier paciente.
        /// </summary>
        /// <value>
        /// The identifier paciente.
        /// </value>
        public int? IdPaciente { get; set; }
    }

    // GET /api/agenda/proximas  (panel "Próximas citas" y "Ver más...")
    /// <summary>
    /// 
    /// </summary>
    public sealed class ProximasCitasQuery
    {
        // Hora actual en la misma zona horaria que usa el front.
        /// <summary>
        /// Gets or sets the desde.
        /// </summary>
        /// <value>
        /// The desde.
        /// </value>
        [Required(ErrorMessage = "Debe indicar la fecha y hora actual.")]
        public DateTime? Desde { get; set; }

        /// <summary>
        /// Gets or sets the cantidad.
        /// </summary>
        /// <value>
        /// The cantidad.
        /// </value>
        [Range(1, 50, ErrorMessage = "La cantidad debe estar entre 1 y 50.")]
        public int Cantidad { get; set; } = 4;

        /// <summary>
        /// Gets or sets the identifier empleado.
        /// </summary>
        /// <value>
        /// The identifier empleado.
        /// </value>
        public int? IdEmpleado { get; set; }
    }

    // GET /api/agenda/disponibilidad  (aviso "Esta hora no está disponible")
    /// <summary>
    /// 
    /// </summary>
    public sealed class DisponibilidadQuery
    {
        /// <summary>
        /// Gets or sets the fecha inicio.
        /// </summary>
        /// <value>
        /// The fecha inicio.
        /// </value>
        [Required(ErrorMessage = "Debe seleccionar la fecha y hora de inicio.")]
        public DateTime? FechaInicio { get; set; }

        /// <summary>
        /// Gets or sets the fecha final.
        /// </summary>
        /// <value>
        /// The fecha final.
        /// </value>
        [Required(ErrorMessage = "Debe seleccionar la fecha y hora final.")]
        public DateTime? FechaFinal { get; set; }

        // Al editar una cita, se envía su id para que no choque consigo misma.
        /// <summary>
        /// Gets or sets the identifier evento excluir.
        /// </summary>
        /// <value>
        /// The identifier evento excluir.
        /// </value>
        public int? IdEventoExcluir { get; set; }
    }
}