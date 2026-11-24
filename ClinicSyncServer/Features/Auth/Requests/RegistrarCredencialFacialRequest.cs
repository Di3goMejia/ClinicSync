
using System.ComponentModel.DataAnnotations;

namespace ClinicSyncServer.Features.Auth.Requests
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.IEquatable&lt;ClinicSyncServer.Features.Auth.Requests.RegistrarCredencialFacialRequest&gt;" />
    public record RegistrarCredencialFacialRequest(
        [property: Required]
        [property: MinLength(5)]
        [property: MaxLength(10)]
        List<string> Muestras
    );
}
