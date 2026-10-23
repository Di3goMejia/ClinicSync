using System.ComponentModel.DataAnnotations;

namespace ClinicSyncServer.Features.Tratamientos.Requests
{
    public class CambiarEstadoTratamientoRequest
    {
        [Required(ErrorMessage = "El estado es obligatorio.")]
        public bool Activo { get; set; }
    }
}
