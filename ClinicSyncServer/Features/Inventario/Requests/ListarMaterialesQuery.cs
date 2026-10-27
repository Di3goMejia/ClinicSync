namespace ClinicSyncServer.Features.Inventario.Requests
{
    public record ListarMaterialesQuery(string? Buscar = null, bool? Activo = null);
}
