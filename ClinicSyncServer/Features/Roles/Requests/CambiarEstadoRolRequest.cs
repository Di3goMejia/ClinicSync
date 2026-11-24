using System.ComponentModel.DataAnnotations;

namespace ClinicSyncServer.Features.Roles.Requests;

/// <summary>
/// 
/// </summary>
/// <seealso cref="System.IEquatable&lt;ClinicSyncServer.Features.Roles.Requests.CambiarEstadoRolRequest&gt;" />
public sealed record CambiarEstadoRolRequest(
    [property: Required(ErrorMessage = "El estado activo es obligatorio.")]
    bool? Activo);
