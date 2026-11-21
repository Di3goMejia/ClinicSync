namespace ClinicSyncServer.Features.Permisos.Projections;

public sealed record PermisoCatalogo(
    int IdPermiso, 
    string Modulo, 
    string Nombre, 
    string Codigo, 
    string Descripcion
);
