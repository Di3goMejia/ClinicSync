namespace ClinicSyncServer.Features.Tratamientos.Requests
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.IEquatable&lt;ClinicSyncServer.Features.Tratamientos.Requests.ListarTratamientoQuery&gt;" />
    public record ListarTratamientoQuery(string? Buscar = null, bool? Activo = null);
}
