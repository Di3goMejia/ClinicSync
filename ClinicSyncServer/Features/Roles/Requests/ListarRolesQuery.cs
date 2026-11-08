using System.ComponentModel.DataAnnotations;

namespace ClinicSyncServer.Features.Roles.Requests;

public sealed record ListarRolesQuery(
    string? Nombre = null,
    bool? Activo = null);
