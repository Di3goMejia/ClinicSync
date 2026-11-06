using System.ComponentModel.DataAnnotations;

namespace ClinicSyncServer.Features.Agenda.Requests
{
    // GET /api/agenda  (calendario Día / Semana / Mes)
    public sealed class ListarAgendaQuery
    {
        [Required(ErrorMessage = "Debe indicar la fecha de inicio del rango.")]
        public DateTime? Desde { get; set; }

        [Required(ErrorMessage = "Debe indicar la fecha final del rango.")]
        public DateTime? Hasta { get; set; }

        // Opcionales: si no se envían, se muestran todos.
        public int? IdEmpleado { get; set; }
        public int? IdPaciente { get; set; }
    }

    // GET /api/agenda/proximas  (panel "Próximas citas" y "Ver más...")
    public sealed class ProximasCitasQuery
    {
        // Hora actual en la misma zona horaria que usa el front.
        [Required(ErrorMessage = "Debe indicar la fecha y hora actual.")]
        public DateTime? Desde { get; set; }

        [Range(1, 50, ErrorMessage = "La cantidad debe estar entre 1 y 50.")]
        public int Cantidad { get; set; } = 4;

        public int? IdEmpleado { get; set; }
    }

    // GET /api/agenda/disponibilidad  (aviso "Esta hora no está disponible")
    public sealed class DisponibilidadQuery
    {
        [Required(ErrorMessage = "Debe seleccionar la fecha y hora de inicio.")]
        public DateTime? FechaInicio { get; set; }

        [Required(ErrorMessage = "Debe seleccionar la fecha y hora final.")]
        public DateTime? FechaFinal { get; set; }

        // Al editar una cita, se envía su id para que no choque consigo misma.
        public int? IdEventoExcluir { get; set; }
    }
}