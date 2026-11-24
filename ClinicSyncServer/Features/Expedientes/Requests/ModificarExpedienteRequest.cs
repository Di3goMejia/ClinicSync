using System.ComponentModel.DataAnnotations;

namespace ClinicSyncServer.Features.Expedientes.Requests
{
    /// <summary>
    /// 
    /// </summary>
    public class ModificarExpedienteRequest
    {
        /// <summary>
        /// Gets or sets the notas.
        /// </summary>
        /// <value>
        /// The notas.
        /// </value>
        [StringLength(500, ErrorMessage = "Las notas no deben superar los 500 caracteres.")]
        public string? Notas { get; set; }
    }
}
