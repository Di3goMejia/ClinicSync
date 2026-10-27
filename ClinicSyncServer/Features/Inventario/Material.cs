namespace ClinicSyncServer.Features.Inventario
{
    public class Material
    {
        public int IdMaterial { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string UnidadMedida { get; set; } = string.Empty;
        public decimal StockActual { get; set; }
        public decimal StockMinimo { get; set; }
        public decimal PrecioCompra { get; set; }
        public string? Imagen { get; set; }
        public string? Descripcion { get; set; }
        public bool Activo { get; set; }
    }
}
