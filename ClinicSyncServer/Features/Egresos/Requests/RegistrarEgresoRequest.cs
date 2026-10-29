namespace ClinicSyncServer.Features.Egresos.Requests
{
    public sealed class RegistrarEgresoRequest
    {
        public string Tipo { get; set; } = "";
        public DateTime Fecha { get; set; }
        public string Concepto { get; set; } = "";
        public string MedioPago { get; set; } = "";
        public decimal Monto { get; set; }

    }


}

