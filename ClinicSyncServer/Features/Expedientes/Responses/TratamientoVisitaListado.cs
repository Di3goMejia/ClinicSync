
namespace ClinicSyncServer.Features.Expedientes.Responses
{
    public class TratamientoVisitaListado
    {
        public int IdTratamientoVisita { get; set; }
        public int IdVisita { get; set; }
        public int IdTratamiento { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Descuento { get; set; }
        public string? Notas { get; set; }
    }
}
