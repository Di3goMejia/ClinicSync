using System.ComponentModel.DataAnnotations;

namespace ClinicSyncServer.Features.Roles.Requests;

/// <summary>
/// 
/// </summary>
/// <seealso cref="System.IEquatable&lt;ClinicSyncServer.Features.Roles.Requests.ModificarRolRequest&gt;" />
public sealed record ModificarRolRequest(
    [property: Required(ErrorMessage = "El nombre es obligatorio.")]
    [property: StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
    string Nombre,

    [property: StringLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
    string? Descripcion,

    IReadOnlyList<string>? Permisos = null)
{
    /// <summary>
    /// Gets the codigo.
    /// </summary>
    /// <value>
    /// The codigo.
    /// </value>
    public string Codigo { get; init; } = string.Empty;
}
