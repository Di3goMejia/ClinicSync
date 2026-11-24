namespace ClinicSyncServer.Features.Egresos.Responses
{
    public sealed class EgresoListado
    {
        public int IdEgreso { get; set; }
        public string Tipo { get; set; } = "";
        public DateTime Fecha { get; set; }
        public string Concepto { get; set; } = "";
        public string MedioPago { get; set; } = "";
        public decimal Monto { get; set; }
        public bool Activo { get; set; }
        public int? IdCompra { get; set; }
        public List<MaterialEgresoDetalle> Materiales { get; set; } = new();

    }
    public sealed class MaterialEgresoDetalle
    {
        public int IdMaterial { get; set; }
        public string Nombre { get; set; } = "";
        public string UnidadMedida { get; set; } = "";
        public decimal Cantidad { get; set; }
    }
}
