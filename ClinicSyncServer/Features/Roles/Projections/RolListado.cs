namespace ClinicSyncServer.Features.Roles.Projections;

/// <summary>
/// 
/// </summary>
public sealed class RolListado
{
    /// <summary>
    /// Gets or sets the identifier rol.
    /// </summary>
    /// <value>
    /// The identifier rol.
    /// </value>
    public int IdRol { get; set; }
    /// <summary>
    /// Gets or sets the nombre.
    /// </summary>
    /// <value>
    /// The nombre.
    /// </value>
    public string Nombre { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the codigo.
    /// </summary>
    /// <value>
    /// The codigo.
    /// </value>
    public string Codigo { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the descripcion.
    /// </summary>
    /// <value>
    /// The descripcion.
    /// </value>
    public string? Descripcion { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="RolListado"/> is activo.
    /// </summary>
    /// <value>
    ///   <c>true</c> if activo; otherwise, <c>false</c>.
    /// </value>
    public bool Activo { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="RolListado"/> is protegido.
    /// </summary>
    /// <value>
    ///   <c>true</c> if protegido; otherwise, <c>false</c>.
    /// </value>
    public bool Protegido { get; set; }
}
