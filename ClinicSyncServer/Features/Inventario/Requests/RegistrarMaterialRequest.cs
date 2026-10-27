using System.ComponentModel.DataAnnotations;

namespace ClinicSyncServer.Features.Inventario.Requests
{
    public class RegistrarMaterialRequest : MaterialRequest
    {
        [Range(typeof(decimal), "0", "9999999999.99", ErrorMessage = "La cantidad inicial no puede ser negativa.")]
        public decimal CantidadInicial { get; set; }
    }
}
