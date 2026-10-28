using System.ComponentModel.DataAnnotations;

namespace ClinicSyncServer.Features.Expedientes.Requests
{
    public class ModificarExpedienteRequest
    {
        [StringLength(500, ErrorMessage = "Las notas no deben superar los 500 caracteres.")]
        public string? Notas { get; set; }
    }
}
