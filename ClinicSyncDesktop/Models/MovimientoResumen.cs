using System;

namespace ClinicSyncDesktop.Models
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class MovimientoResumen
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
        /// Gets or sets the tipo.
        /// </summary>
        /// <value>
        /// The tipo.
        /// </value>
        public string Tipo { get; set; } = "";
    }
}
