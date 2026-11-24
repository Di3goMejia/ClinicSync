using System.ComponentModel.DataAnnotations;

namespace ClinicSyncServer.Features.Inventario.Requests
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="ClinicSyncServer.Features.Inventario.Requests.MaterialRequest" />
    public class RegistrarMaterialRequest : MaterialRequest
    {
        /// <summary>
        /// Gets or sets the cantidad inicial.
        /// </summary>
        /// <value>
        /// The cantidad inicial.
        /// </value>
        [Range(typeof(decimal), "0", "9999999999.99", ErrorMessage = "La cantidad inicial no puede ser negativa.")]
        public decimal CantidadInicial { get; set; }
    }
}
