namespace ClinicSyncServer.Features.Roles.Projections;

public sealed class RolListado
{
    public int IdRol { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Activo { get; set; }
    public bool Protegido { get; set; }
}
