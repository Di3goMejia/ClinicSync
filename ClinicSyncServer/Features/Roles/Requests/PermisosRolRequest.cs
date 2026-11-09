using System.ComponentModel.DataAnnotations;

namespace ClinicSyncServer.Features.Roles.Requests;

public sealed record PermisosRolRequest(
    [property: Required(ErrorMessage = "La lista de permisos es obligatoria.")]
    IReadOnlyList<string>? Permisos);
