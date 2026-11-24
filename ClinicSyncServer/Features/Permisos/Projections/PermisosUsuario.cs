namespace ClinicSyncServer.Features.Permisos.Projections
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.IEquatable&lt;ClinicSyncServer.Features.Permisos.Projections.PermisosUsuario&gt;" />
    public record PermisosUsuario(string Rol, IReadOnlyList<string> Permisos);
}
