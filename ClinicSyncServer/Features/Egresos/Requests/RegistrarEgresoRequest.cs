namespace ClinicSyncServer.Features.Egresos.Requests
{
    public sealed class MaterialEgresoRequest
    {
        public int IdMaterial { get; set; }
        public decimal Cantidad { get; set; }
    }

    public sealed class RegistrarEgresoRequest
    {
        public string Tipo { get; set; } = "";
        public DateTime Fecha { get; set; }
        public string Concepto { get; set; } = "";
        public string MedioPago { get; set; } = "";
        public decimal Monto { get; set; }
        public List<MaterialEgresoRequest>? Materiales { get; set; }
    }

    public sealed class ActualizarEgresoRequest
    {
        public DateTime Fecha { get; set; }
        public string Concepto { get; set; } = "";
        public string MedioPago { get; set; } = "";
        public decimal Monto { get; set; }
        public bool? Activo { get; set; }
        public List<MaterialEgresoRequest>? Materiales { get; set; }
    }
}