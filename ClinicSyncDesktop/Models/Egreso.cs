using System;
using System.Collections.Generic;
using System.Text;

namespace ClinicSyncDesktop.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class Egreso
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
        public string Tipo { get; set; } = string.Empty;

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
        public string Concepto { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the medio pago.
        /// </summary>
        /// <value>
        /// The medio pago.
        /// </value>
        public string MedioPago { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the monto.
        /// </summary>
        /// <value>
        /// The monto.
        /// </value>
        public decimal Monto { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Egreso"/> is activo.
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
        public List<ClinicSyncDesktop.Views.MaterialParaEgreso> Materiales { get; set; } = new();

        /// <summary>
        /// Gets the tipo texto.
        /// </summary>
        /// <value>
        /// The tipo texto.
        /// </value>
        public string TipoTexto => Capitalizar(Tipo);

        /// <summary>
        /// Gets the medio pago texto.
        /// </summary>
        /// <value>
        /// The medio pago texto.
        /// </value>
        public string MedioPagoTexto => Capitalizar(MedioPago);

        /// <summary>
        /// Capitalizars the specified texto.
        /// </summary>
        /// <param name="texto">The texto.</param>
        /// <returns></returns>
        private static string Capitalizar(string texto) =>
            string.IsNullOrEmpty(texto)
                ? ""
                : char.ToUpper(texto[0]) + texto[1..].ToLower();

    }
}