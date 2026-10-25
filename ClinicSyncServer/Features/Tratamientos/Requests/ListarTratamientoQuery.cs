namespace ClinicSyncServer.Features.Tratamientos.Requests
{
    public record ListarTratamientoQuery(string? Buscar = null, bool? Activo = null);
}
