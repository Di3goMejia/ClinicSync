using Org.BouncyCastle.Asn1.Ocsp;
using System.ComponentModel.DataAnnotations;

namespace ClinicSyncServer.Features.Expedientes.Requests
{
    public class TratamientoSeleccionadoRequest
    {
        [Required(ErrorMessage = "El ID de tratamiento es requerido.")]
        public int? IdTratamiento { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero.")]
        public int Cantidad { get; set; } = 1;
    }
}