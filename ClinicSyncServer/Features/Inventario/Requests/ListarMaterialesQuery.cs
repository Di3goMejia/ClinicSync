namespace ClinicSyncServer.Features.Inventario.Requests
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.IEquatable&lt;ClinicSyncServer.Features.Inventario.Requests.ListarMaterialesQuery&gt;" />
    public record ListarMaterialesQuery(string? Buscar = null, bool? Activo = null);
}
