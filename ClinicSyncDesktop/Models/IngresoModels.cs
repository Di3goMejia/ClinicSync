
using System;
using System.Collections.Generic;

namespace ClinicSyncDesktop.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class PacienteIngreso
    {
        /// <summary>
        /// Gets or sets the identifier paciente.
        /// </summary>
        /// <value>
        /// The identifier paciente.
        /// </value>
        public int IdPaciente { get; set; }
        /// <summary>
        /// Gets or sets the nombres.
        /// </summary>
        /// <value>
        /// The nombres.
        /// </value>
        public string Nombres { get; set; } = "";
        /// <summary>
        /// Gets or sets the apellidos.
        /// </summary>
        /// <value>
        /// The apellidos.
        /// </value>
        public string Apellidos { get; set; } = "";
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="PacienteIngreso"/> is activo.
        /// </summary>
        /// <value>
        ///   <c>true</c> if activo; otherwise, <c>false</c>.
        /// </value>
        public bool Activo { get; set; }

        /// <summary>
        /// Gets the nombre completo.
        /// </summary>
        /// <value>
        /// The nombre completo.
        /// </value>
        public string NombreCompleto =>
            $"{Nombres} {Apellidos}";
    }

    /// <summary>
    /// 
    /// </summary>
    public class TratamientoIngreso
    {
        /// <summary>
        /// Gets or sets the identifier tratamiento visita.
        /// </summary>
        /// <value>
        /// The identifier tratamiento visita.
        /// </value>
        public int IdTratamientoVisita { get; set; }
        /// <summary>
        /// Gets or sets the identifier tratamiento.
        /// </summary>
        /// <value>
        /// The identifier tratamiento.
        /// </value>
        public int IdTratamiento { get; set; }
        /// <summary>
        /// Gets or sets the nombre.
        /// </summary>
        /// <value>
        /// The nombre.
        /// </value>
        public string Nombre { get; set; } = "";
        /// <summary>
        /// Gets or sets the precio.
        /// </summary>
        /// <value>
        /// The precio.
        /// </value>
        public decimal Precio { get; set; }
        /// <summary>
        /// Gets or sets the cantidad.
        /// </summary>
        /// <value>
        /// The cantidad.
        /// </value>
        public int Cantidad { get; set; }

        /// <summary>
        /// Gets the importe.
        /// </summary>
        /// <value>
        /// The importe.
        /// </value>
        public decimal Importe => Precio * Cantidad;

        /// <summary>
        /// Gets the descripcion.
        /// </summary>
        /// <value>
        /// The descripcion.
        /// </value>
        public string Descripcion =>
            $"{Nombre} - L {Importe:N2}";
    }

    /// <summary>
    /// 
    /// </summary>
    public class MaterialIngreso
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
        /// Gets or sets the stock actual.
        /// </summary>
        /// <value>
        /// The stock actual.
        /// </value>
        public decimal StockActual { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="MaterialIngreso"/> is activo.
        /// </summary>
        /// <value>
        ///   <c>true</c> if activo; otherwise, <c>false</c>.
        /// </value>
        public bool Activo { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class MaterialConsumido
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
    public class RegistrarIngresoRequest
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
        /// Gets or sets the ids tratamientos visita.
        /// </summary>
        /// <value>
        /// The ids tratamientos visita.
        /// </value>
        public List<int> IdsTratamientosVisita
        { get; set; } = new();

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
        /// Gets or sets the materiales.
        /// </summary>
        /// <value>
        /// The materiales.
        /// </value>
        public List<MaterialConsumido> Materiales
        { get; set; } = new();
    }

    /// <summary>
    /// 
    /// </summary>
    public class IngresoListado
    {
        /// <summary>
        /// Gets or sets the identifier ingreso.
        /// </summary>
        /// <value>
        /// The identifier ingreso.
        /// </value>
        public int IdIngreso { get; set; }
        /// <summary>
        /// Gets or sets the fecha.
        /// </summary>
        /// <value>
        /// The fecha.
        /// </value>
        public DateTime Fecha { get; set; }
        /// <summary>
        /// Gets or sets the paciente.
        /// </summary>
        /// <value>
        /// The paciente.
        /// </value>
        public string Paciente { get; set; } = "";
        /// <summary>
        /// Gets or sets the tratamiento.
        /// </summary>
        /// <value>
        /// The tratamiento.
        /// </value>
        public string Tratamiento { get; set; } = "";
        /// <summary>
        /// Gets or sets the medio pago.
        /// </summary>
        /// <value>
        /// The medio pago.
        /// </value>
        public string MedioPago { get; set; } = "";
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
        /// Gets or sets a value indicating whether this <see cref="IngresoListado"/> is activo.
        /// </summary>
        /// <value>
        ///   <c>true</c> if activo; otherwise, <c>false</c>.
        /// </value>
        public bool Activo { get; set; }

    }
}

