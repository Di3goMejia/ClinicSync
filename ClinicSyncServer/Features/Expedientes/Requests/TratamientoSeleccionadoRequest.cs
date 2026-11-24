using Org.BouncyCastle.Asn1.Ocsp;
using System.ComponentModel.DataAnnotations;

namespace ClinicSyncServer.Features.Expedientes.Requests
{
    /// <summary>
    /// 
    /// </summary>
    public class TratamientoSeleccionadoRequest
    {
        /// <summary>
        /// Gets or sets the identifier tratamiento.
        /// </summary>
        /// <value>
        /// The identifier tratamiento.
        /// </value>
        [Required(ErrorMessage = "El ID de tratamiento es requerido.")]
        public int? IdTratamiento { get; set; }

        /// <summary>
        /// Gets or sets the cantidad.
        /// </summary>
        /// <value>
        /// The cantidad.
        /// </value>
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero.")]
        public int Cantidad { get; set; } = 1;
    }
}