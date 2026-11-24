using System.ComponentModel.DataAnnotations;

namespace ClinicSyncServer.Features.Tratamientos.Requests
{
    /// <summary>
    /// 
    /// </summary>
    public class CambiarEstadoTratamientoRequest
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CambiarEstadoTratamientoRequest"/> is activo.
        /// </summary>
        /// <value>
        ///   <c>true</c> if activo; otherwise, <c>false</c>.
        /// </value>
        [Required(ErrorMessage = "El estado es obligatorio.")]
        public bool Activo { get; set; }
    }
}
