namespace ClinicSyncServer.Features.Expedientes.Requests
{
    public class TratamientoSeleccionadoRequest
    {
        public int IdTratamiento { get; set; }

        public int Cantidad { get; set; } = 1;
    }
}