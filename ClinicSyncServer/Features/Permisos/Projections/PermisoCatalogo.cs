namespace ClinicSyncServer.Features.Permisos.Projections;

/// <summary>
/// 
/// </summary>
/// <seealso cref="System.IEquatable&lt;ClinicSyncServer.Features.Permisos.Projections.PermisoCatalogo&gt;" />
public sealed record PermisoCatalogo(
    int IdPermiso, 
    string Modulo, 
    string Nombre, 
    string Codigo, 
    string Descripcion
);
