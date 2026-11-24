
using System;
using System.Collections.Generic;

namespace ClinicSyncDesktop.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class EditarIngresoDetalle
    {
        /// <summary>
        /// Gets or sets the identifier ingreso.
        /// </summary>
        /// <value>
        /// The identifier ingreso.
        /// </value>
        public int IdIngreso { get; set; }

        /// <summary>
        /// Gets or sets the identifier paciente.
        /// </summary>
        /// <value>
        /// The identifier paciente.
        /// </value>
        public int IdPaciente { get; set; }

        /// <summary>
        /// Gets or sets the paciente.
        /// </summary>
        /// <value>
        /// The paciente.
        /// </value>
        public string Paciente { get; set; } = "";

        /// <summary>
        /// Gets or sets the fecha.
        /// </summary>
        /// <value>
        /// The fecha.
        /// </value>
        public DateTime Fecha { get; set; }

        /// <summary>
        /// Gets or sets the subtotal.
        /// </summary>
        /// <value>
        /// The subtotal.
        /// </value>
        public decimal Subtotal { get; set; }

        /// <summary>
        /// Gets or sets the descuento.
        /// </summary>
        /// <value>
        /// The descuento.
        /// </value>
        public decimal Descuento { get; set; }

        /// <summary>
        /// Gets or sets the total.
        /// </summary>
        /// <value>
        /// The total.
        /// </value>
        public decimal Total { get; set; }

        /// <summary>
        /// Gets or sets the medio pago.
        /// </summary>
        /// <value>
        /// The medio pago.
        /// </value>
        public string MedioPago { get; set; } = "";

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="EditarIngresoDetalle"/> is activo.
        /// </summary>
        /// <value>
        ///   <c>true</c> if activo; otherwise, <c>false</c>.
        /// </value>
        public bool Activo { get; set; }

        /// <summary>
        /// Gets or sets the materiales.
        /// </summary>
        /// <value>
        /// The materiales.
        /// </value>
        public List<MaterialConsumido> Materiales { get; set; } = new();

        /// <summary>
        /// Gets or sets the tratamientos.
        /// </summary>
        /// <value>
        /// The tratamientos.
        /// </value>
        public List<TratamientoIngreso> Tratamientos
        { get; set; } = new();
    }

    /// <summary>
    /// 
    /// </summary>
    public class EditarIngresoRequest
    {
        /// <summary>
        /// Gets or sets the identifier paciente.
        /// </summary>
        /// <value>
        /// The identifier paciente.
        /// </value>
        public int IdPaciente { get; set; }

        /// <summary>
        /// Gets or sets the fecha.
        /// </summary>
        /// <value>
        /// The fecha.
        /// </value>
        public DateTime Fecha { get; set; }

        /// <summary>
        /// Gets or sets the descuento.
        /// </summary>
        /// <value>
        /// The descuento.
        /// </value>
        public decimal Descuento { get; set; }

        /// <summary>
        /// Gets or sets the medio pago.
        /// </summary>
        /// <value>
        /// The medio pago.
        /// </value>
        public string MedioPago { get; set; } = "";

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="EditarIngresoRequest"/> is activo.
        /// </summary>
        /// <value>
        ///   <c>true</c> if activo; otherwise, <c>false</c>.
        /// </value>
        public bool Activo { get; set; }

        /// <summary>
        /// Gets or sets the materiales.
        /// </summary>
        /// <value>
        /// The materiales.
        /// </value>
        public List<MaterialConsumido> Materiales { get; set; } = new();

        /// <summary>
        /// Gets or sets the ids tratamientos.
        /// </summary>
        /// <value>
        /// The ids tratamientos.
        /// </value>
        public List<int> IdsTratamientos
        { get; set; } = new();
    }
}
