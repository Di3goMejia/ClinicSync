using System.ComponentModel.DataAnnotations;

namespace ClinicSyncServer.Features.Agenda.Requests
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class CitaRequest
    {
        /// <summary>
        /// Gets or sets the identifier paciente.
        /// </summary>
        /// <value>
        /// The identifier paciente.
        /// </value>
        [Required(ErrorMessage = "Debe seleccionar el paciente.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar el paciente.")]
        public int? IdPaciente { get; set; }

        /// <summary>
        /// Gets or sets the titulo.
        /// </summary>
        /// <value>
        /// The titulo.
        /// </value>
        [Required(ErrorMessage = "Debe ingresar el título de la cita.")]
        [StringLength(100, ErrorMessage = "El título no puede superar los 100 caracteres.")]
        public string Titulo { get; set; } = "";

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

        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        /// <value>
        /// The color.
        /// </value>
        [RegularExpression("^#[0-9A-Fa-f]{6}$",
            ErrorMessage = "El color debe tener el formato #RRGGBB.")]
        public string? Color { get; set; }

        /// <summary>
        /// Gets or sets the direccion.
        /// </summary>
        /// <value>
        /// The direccion.
        /// </value>
        [StringLength(200, ErrorMessage = "La dirección no puede superar los 200 caracteres.")]
        public string? Direccion { get; set; }

        /// <summary>
        /// Gets or sets the notas.
        /// </summary>
        /// <value>
        /// The notas.
        /// </value>
        [StringLength(500, ErrorMessage = "Las notas no pueden superar los 500 caracteres.")]
        public string? Notas { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [todo el dia].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [todo el dia]; otherwise, <c>false</c>.
        /// </value>
        public bool TodoElDia { get; set; }
    }
}