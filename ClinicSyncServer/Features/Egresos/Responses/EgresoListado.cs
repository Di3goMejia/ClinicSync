namespace ClinicSyncServer.Features.Egresos.Responses
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class EgresoListado
    {
        /// <summary>
        /// Gets or sets the identifier egreso.
        /// </summary>
        /// <value>
        /// The identifier egreso.
        /// </value>
        public int IdEgreso { get; set; }
        /// <summary>
        /// Gets or sets the tipo.
        /// </summary>
        /// <value>
        /// The tipo.
        /// </value>
        public string Tipo { get; set; } = "";
        /// <summary>
        /// Gets or sets the fecha.
        /// </summary>
        /// <value>
        /// The fecha.
        /// </value>
        public DateTime Fecha { get; set; }
        /// <summary>
        /// Gets or sets the concepto.
        /// </summary>
        /// <value>
        /// The concepto.
        /// </value>
        public string Concepto { get; set; } = "";
        /// <summary>
        /// Gets or sets the medio pago.
        /// </summary>
        /// <value>
        /// The medio pago.
        /// </value>
        public string MedioPago { get; set; } = "";
        /// <summary>
        /// Gets or sets the monto.
        /// </summary>
        /// <value>
        /// The monto.
        /// </value>
        public decimal Monto { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="EgresoListado"/> is activo.
        /// </summary>
        /// <value>
        ///   <c>true</c> if activo; otherwise, <c>false</c>.
        /// </value>
        public bool Activo { get; set; }
        /// <summary>
        /// Gets or sets the identifier compra.
        /// </summary>
        /// <value>
        /// The identifier compra.
        /// </value>
        public int? IdCompra { get; set; }
        /// <summary>
        /// Gets or sets the materiales.
        /// </summary>
        /// <value>
        /// The materiales.
        /// </value>
        public List<MaterialEgresoDetalle> Materiales { get; set; } = new();

    }
    /// <summary>
    /// 
    /// </summary>
    public sealed class MaterialEgresoDetalle
    {
        /// <summary>
        /// Gets or sets the identifier material.
        /// </summary>
        /// <value>
        /// The identifier material.
        /// </value>
        public int IdMaterial { get; set; }
        /// <summary>
        /// Gets or sets the nombre.
        /// </summary>
        /// <value>
        /// The nombre.
        /// </value>
        public string Nombre { get; set; } = "";
        /// <summary>
        /// Gets or sets the unidad medida.
        /// </summary>
        /// <value>
        /// The unidad medida.
        /// </value>
        public string UnidadMedida { get; set; } = "";
        /// <summary>
        /// Gets or sets the cantidad.
        /// </summary>
        /// <value>
        /// The cantidad.
        /// </value>
        public decimal Cantidad { get; set; }
    }
}
