using System.ComponentModel.DataAnnotations;

namespace ClinicSyncServer.Features.Tratamientos.Requests
{
    /// <summary>
    /// 
    /// </summary>
    public class RegistrarTratamientoRequest
    {
        /// <summary>
        /// Gets or sets the nombre.
        /// </summary>
        /// <value>
        /// The nombre.
        /// </value>
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the precio.
        /// </summary>
        /// <value>
        /// The precio.
        /// </value>
        [Required(ErrorMessage = "El precio es obligatorio.")]
        [Range(0.0, Double.MaxValue, ErrorMessage = "El precio debe ser mayor a cero.")]
        public decimal Precio { get; set; }
        /// <summary>
        /// Gets or sets the descripcion.
        /// </summary>
        /// <value>
        /// The descripcion.
        /// </value>
        public string? Descripcion { get; set; }
    }
}
