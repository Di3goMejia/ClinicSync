using System.ComponentModel.DataAnnotations;

namespace ClinicSyncServer.Features.Expedientes.Requests
{
    /// <summary>
    /// 
    /// </summary>
    public class CrearVisitaCompletaRequest
    {
        /// <summary>
        /// Gets or sets the identifier paciente.
        /// </summary>
        /// <value>
        /// The identifier paciente.
        /// </value>
        [Required(ErrorMessage = "El ID del paciente es obligatorio.")]
        public int? IdPaciente { get; set; }

        /// <summary>
        /// Gets or sets the identifier empleado.
        /// </summary>
        /// <value>
        /// The identifier empleado.
        /// </value>
        [Required(ErrorMessage = "El ID del empleado es obligatorio.")]
        public int? IdEmpleado { get; set; }

        /// <summary>
        /// Gets or sets the fecha.
        /// </summary>
        /// <value>
        /// The fecha.
        /// </value>
        [Required(ErrorMessage = "La fecha es obligatoria.")]
        public DateTime Fecha { get; set; }

        /// <summary>
        /// Gets or sets the motivo.
        /// </summary>
        /// <value>
        /// The motivo.
        /// </value>
        [Required(ErrorMessage = "El motivo es obligatorio.")]
        [StringLength(255, ErrorMessage = "El motivo no debe superar los 255 caracteres.")]
        public string Motivo { get; set; } = "";

        /// <summary>
        /// Gets or sets the notas.
        /// </summary>
        /// <value>
        /// The notas.
        /// </value>
        [StringLength(500, ErrorMessage = "Las notas no deben superar los 500 caracteres.")]
        public string? Notas { get; set; }

        /// <summary>
        /// Gets or sets the tratamientos.
        /// </summary>
        /// <value>
        /// The tratamientos.
        /// </value>
        [Required(ErrorMessage = "El listado de tratamientos es obligatorio.")]
        [MinLength(1, ErrorMessage = "El listado debe incluir al menos un tratamiento.")]
        public List<TratamientoSeleccionadoRequest> Tratamientos { get; set; } = [];
    }
}