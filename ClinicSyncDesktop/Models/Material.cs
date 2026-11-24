namespace ClinicSyncDesktop.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class Material
    {
        /// <summary>
        /// Gets or sets the identifier material.
        /// </summary>
        /// <value>
        /// The identifier material.
        /// </value>
        public int IdMaterial { get; set; }

        /// <summary>
        /// Gets or sets the codigo.
        /// </summary>
        /// <value>
        /// The codigo.
        /// </value>
        public string Codigo { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the nombre.
        /// </summary>
        /// <value>
        /// The nombre.
        /// </value>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the unidad medida.
        /// </summary>
        /// <value>
        /// The unidad medida.
        /// </value>
        public string UnidadMedida { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the stock actual.
        /// </summary>
        /// <value>
        /// The stock actual.
        /// </value>
        public decimal StockActual { get; set; }

        /// <summary>
        /// Gets or sets the stock minimo.
        /// </summary>
        /// <value>
        /// The stock minimo.
        /// </value>
        public decimal StockMinimo { get; set; }

        /// <summary>
        /// Gets or sets the precio compra.
        /// </summary>
        /// <value>
        /// The precio compra.
        /// </value>
        public decimal PrecioCompra { get; set; }

        /// <summary>
        /// Gets or sets the imagen.
        /// </summary>
        /// <value>
        /// The imagen.
        /// </value>
        public string? Imagen { get; set; }

        /// <summary>
        /// Gets or sets the descripcion.
        /// </summary>
        /// <value>
        /// The descripcion.
        /// </value>
        public string? Descripcion { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Material"/> is activo.
        /// </summary>
        /// <value>
        ///   <c>true</c> if activo; otherwise, <c>false</c>.
        /// </value>
        public bool Activo { get; set; }
    }
}
