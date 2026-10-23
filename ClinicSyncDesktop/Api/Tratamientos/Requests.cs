namespace ClinicSyncDesktop.Api.Tratamientos
{
    public record ListarTratamientoQuery(string? Buscar = null, bool? Activo = null);

    public class RegistrarTratamientoRequest
    {
        public string Nombre { get; set; } = string.Empty;
        public double Precio { get; set; }
        public string? Descripcion { get; set; }
    }

    public record CambiarEstadoTratamientoRequest(bool Activo);
}
