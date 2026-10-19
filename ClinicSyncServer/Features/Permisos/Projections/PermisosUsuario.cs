namespace ClinicSyncServer.Features.Permisos.Projections
{
    public record PermisosUsuario(string Rol, IReadOnlyList<string> Permisos);
}
