using System.ComponentModel.DataAnnotations;

namespace ClinicSyncServer.Features.Roles.Requests;

public sealed record CrearRolRequest(
    [property: Required(ErrorMessage = "El nombre es obligatorio.")]
    [property: StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
    string Nombre,

    [property: StringLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
    string? Descripcion,
    IReadOnlyList<string>? Permisos = null);
