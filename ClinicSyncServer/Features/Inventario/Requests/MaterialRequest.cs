using System.ComponentModel.DataAnnotations;

namespace ClinicSyncServer.Features.Inventario.Requests
{
    public class MaterialRequest
    {
        [Required(ErrorMessage = "El código es obligatorio.")]
        [StringLength(30, ErrorMessage = "El código no puede superar los 30 caracteres.")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La unidad de medida es obligatoria.")]
        [StringLength(20, ErrorMessage = "La unidad de medida no puede superar los 20 caracteres.")]
        public string UnidadMedida { get; set; } = string.Empty;

        [Range(typeof(decimal), "0", "9999999999.99", ErrorMessage = "El stock mínimo no puede ser negativo.")]
        public decimal StockMinimo { get; set; }

        [Range(typeof(decimal), "0", "99999999.99", ErrorMessage = "El precio de compra no puede ser negativo.")]
        public decimal PrecioCompra { get; set; }

        [StringLength(255, ErrorMessage = "La ruta de la imagen no puede superar los 255 caracteres.")]
        public string? Imagen { get; set; }

        [StringLength(200, ErrorMessage = "La descripción no puede superar los 200 caracteres.")]
        public string? Descripcion { get; set; }
    }
}
