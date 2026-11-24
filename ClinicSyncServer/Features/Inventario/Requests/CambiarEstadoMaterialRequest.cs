using System.ComponentModel.DataAnnotations;

namespace ClinicSyncServer.Features.Inventario.Requests
{
    /// <summary>
    /// 
    /// </summary>
    public class CambiarEstadoMaterialRequest
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CambiarEstadoMaterialRequest"/> is activo.
        /// </summary>
        /// <value>
        ///   <c>true</c> if activo; otherwise, <c>false</c>.
        /// </value>
        [Required(ErrorMessage = "El estado es obligatorio.")]
        public bool Activo { get; set; }
    }
}
