using System.ComponentModel.DataAnnotations;

namespace ClinicSyncServer.Features.Roles.Requests;

/// <summary>
/// 
/// </summary>
/// <seealso cref="System.IEquatable&lt;ClinicSyncServer.Features.Roles.Requests.PermisosRolRequest&gt;" />
public sealed record PermisosRolRequest(
    [property: Required(ErrorMessage = "La lista de permisos es obligatoria.")]
    IReadOnlyList<string>? Permisos);
