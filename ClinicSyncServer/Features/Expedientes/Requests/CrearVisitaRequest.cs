
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicSyncServer.Features.Expedientes.Requests
{
    public class CrearVisitaRequest
    {
        [Required(ErrorMessage = "El ID de paciente es obligatorio.")]
        public int? IdPaciente { get; set; }

        [Required(ErrorMessage = "El ID de empleado es obligatorio.")]
        public int? IdEmpleado { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria.")]
        public DateTime Fecha { get; set; }

        [Required(ErrorMessage = "El motivo es obligatorio.")]
        [StringLength(255, ErrorMessage = "El motivo no debe superar los 255 caracteres.")]
        public string Motivo { get; set; } = "";

        [StringLength(500, ErrorMessage = "Las notas no deben superar los 500 caracteres.")]
        public string? Notas { get; set; }
    }
}
