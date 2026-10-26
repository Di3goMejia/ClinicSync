using System.ComponentModel.DataAnnotations;

namespace ClinicSyncServer.Features.Tratamientos.Requests
{
    public class RegistrarTratamientoRequest
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El precio es obligatorio.")]
        [Range(0.0, Double.MaxValue, ErrorMessage = "El precio debe ser mayor a cero.")]
        public decimal Precio { get; set; }
        public string? Descripcion { get; set; }
    }
}
