using System.ComponentModel.DataAnnotations;

namespace ClinicSyncServer.Features.Roles.Requests;

/// <summary>
/// 
/// </summary>
/// <seealso cref="System.IEquatable&lt;ClinicSyncServer.Features.Roles.Requests.ListarRolesQuery&gt;" />
public sealed record ListarRolesQuery(
    string? Nombre = null,
    bool? Activo = null);
