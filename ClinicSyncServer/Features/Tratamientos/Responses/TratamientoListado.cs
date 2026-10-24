namespace ClinicSyncServer.Features.Tratamientos.Responses
{
    public class TratamientoListado
    {
        public int IdTratamiento { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public decimal Precio { get; set; }

        public string? Descripcion { get; set; }

        public bool Activo { get; set; }
    }
}
