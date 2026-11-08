using System.ComponentModel.DataAnnotations;

namespace ClinicSyncServer.Features.Roles.Requests;

public sealed record CambiarEstadoRolRequest(
    [property: Required(ErrorMessage = "El estado activo es obligatorio.")]
    bool? Activo);
