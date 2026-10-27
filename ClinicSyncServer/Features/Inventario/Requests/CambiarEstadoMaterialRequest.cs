using System.ComponentModel.DataAnnotations;

namespace ClinicSyncServer.Features.Inventario.Requests
{
    public class CambiarEstadoMaterialRequest
    {
        [Required(ErrorMessage = "El estado es obligatorio.")]
        public bool Activo { get; set; }
    }
}
