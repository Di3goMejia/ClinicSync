
using System.ComponentModel.DataAnnotations;

namespace ClinicSyncServer.Features.Auth.Requests
{
    public record RegistrarCredencialFacialRequest(
        [property: Required]
        [property: MinLength(5)]
        [property: MaxLength(10)]
        List<string> Muestras
    );
}
