namespace ClinicSyncServer.Features.Egresos.Requests
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class MaterialEgresoRequest
    {
        /// <summary>
        /// Gets or sets the identifier material.
        /// </summary>
        /// <value>
        /// The identifier material.
        /// </value>
        public int IdMaterial { get; set; }
        /// <summary>
        /// Gets or sets the cantidad.
        /// </summary>
        /// <value>
        /// The cantidad.
        /// </value>
        public decimal Cantidad { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class RegistrarEgresoRequest
    {
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
        /// Gets or sets the materiales.
        /// </summary>
        /// <value>
        /// The materiales.
        /// </value>
        public List<MaterialEgresoRequest>? Materiales { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class ActualizarEgresoRequest
    {
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
        /// Gets or sets the activo.
        /// </summary>
        /// <value>
        /// The activo.
        /// </value>
        public bool? Activo { get; set; }
        /// <summary>
        /// Gets or sets the materiales.
        /// </summary>
        /// <value>
        /// The materiales.
        /// </value>
        public List<MaterialEgresoRequest>? Materiales { get; set; }
    }
}