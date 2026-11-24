using System.ComponentModel.DataAnnotations;

namespace ClinicSyncServer.Features.Inventario.Requests
{
    /// <summary>
    /// 
    /// </summary>
    public class MaterialRequest
    {
        /// <summary>
        /// Gets or sets the codigo.
        /// </summary>
        /// <value>
        /// The codigo.
        /// </value>
        [Required(ErrorMessage = "El código es obligatorio.")]
        [StringLength(30, ErrorMessage = "El código no puede superar los 30 caracteres.")]
        public string Codigo { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the nombre.
        /// </summary>
        /// <value>
        /// The nombre.
        /// </value>
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the unidad medida.
        /// </summary>
        /// <value>
        /// The unidad medida.
        /// </value>
        [Required(ErrorMessage = "La unidad de medida es obligatoria.")]
        [StringLength(20, ErrorMessage = "La unidad de medida no puede superar los 20 caracteres.")]
        public string UnidadMedida { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the stock minimo.
        /// </summary>
        /// <value>
        /// The stock minimo.
        /// </value>
        [Range(typeof(decimal), "0", "9999999999.99",
            ErrorMessage = "El stock mínimo no puede ser negativo.")]
        public decimal StockMinimo { get; set; }

        /// <summary>
        /// Gets or sets the stock actual.
        /// </summary>
        /// <value>
        /// The stock actual.
        /// </value>
        [Range(typeof(decimal), "0", "9999999999.99",
            ErrorMessage = "El stock actual no puede ser negativo.")]
        public decimal? StockActual { get; set; }

        /// <summary>
        /// Gets or sets the stock esperado.
        /// </summary>
        /// <value>
        /// The stock esperado.
        /// </value>
        public decimal? StockEsperado { get; set; }

        /// <summary>
        /// Gets or sets the motivo.
        /// </summary>
        /// <value>
        /// The motivo.
        /// </value>
        [StringLength(200,
            ErrorMessage = "El motivo no puede superar los 200 caracteres.")]
        public string? Motivo { get; set; }

        /// <summary>
        /// Gets or sets the precio compra.
        /// </summary>
        /// <value>
        /// The precio compra.
        /// </value>
        [Range(typeof(decimal), "0", "99999999.99",
            ErrorMessage = "El precio de compra no puede ser negativo.")]
        public decimal PrecioCompra { get; set; }

        /// <summary>
        /// Gets or sets the imagen.
        /// </summary>
        /// <value>
        /// The imagen.
        /// </value>
        [StringLength(255,
            ErrorMessage = "La ruta de la imagen no puede superar los 255 caracteres.")]
        public string? Imagen { get; set; }

        /// <summary>
        /// Gets or sets the descripcion.
        /// </summary>
        /// <value>
        /// The descripcion.
        /// </value>
        [StringLength(200,
            ErrorMessage = "La descripción no puede superar los 200 caracteres.")]
        public string? Descripcion { get; set; }
    }
}
