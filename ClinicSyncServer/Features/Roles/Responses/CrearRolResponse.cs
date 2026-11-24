namespace ClinicSyncServer.Features.Roles.Responses;

/// <summary>
/// 
/// </summary>
public sealed class CrearRolResponse
{
    /// <summary>
    /// Gets or sets the identifier rol.
    /// </summary>
    /// <value>
    /// The identifier rol.
    /// </value>
    public int IdRol { get; set; }
    /// <summary>
    /// Gets or sets the codigo.
    /// </summary>
    /// <value>
    /// The codigo.
    /// </value>
    public string Codigo { get; set; } = string.Empty;
}
