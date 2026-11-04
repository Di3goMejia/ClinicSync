using System.ComponentModel.DataAnnotations;

namespace ClinicSyncServer.Features.Agenda.Requests
{
    public sealed class CitaRequest
    {
        [Required(ErrorMessage = "Debe seleccionar el paciente.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar el paciente.")]
        public int? IdPaciente { get; set; }

        [Required(ErrorMessage = "Debe ingresar el título de la cita.")]
        [StringLength(100, ErrorMessage = "El título no puede superar los 100 caracteres.")]
        public string Titulo { get; set; } = "";

        [Required(ErrorMessage = "Debe seleccionar la fecha y hora de inicio.")]
        public DateTime? FechaInicio { get; set; }

        [Required(ErrorMessage = "Debe seleccionar la fecha y hora final.")]
        public DateTime? FechaFinal { get; set; }

        [RegularExpression("^#[0-9A-Fa-f]{6}$",
            ErrorMessage = "El color debe tener el formato #RRGGBB.")]
        public string? Color { get; set; }

        [StringLength(200, ErrorMessage = "La dirección no puede superar los 200 caracteres.")]
        public string? Direccion { get; set; }

        [StringLength(500, ErrorMessage = "Las notas no pueden superar los 500 caracteres.")]
        public string? Notas { get; set; }

        public bool TodoElDia { get; set; }
    }
}