
using System;
using System.Collections.Generic;

namespace ClinicSyncDesktop.Models
{
    public class PacienteIngreso
    {
        public int IdPaciente { get; set; }
        public string Nombres { get; set; } = "";
        public string Apellidos { get; set; } = "";
        public bool Activo { get; set; }

        public string NombreCompleto =>
            $"{Nombres} {Apellidos}";
    }

    public class TratamientoIngreso
    {
        public int IdTratamientoVisita { get; set; }
        public int IdTratamiento { get; set; }
        public string Nombre { get; set; } = "";
        public decimal Precio { get; set; }
        public int Cantidad { get; set; }

        public decimal Importe => Precio * Cantidad;

        public string Descripcion =>
            $"{Nombre} - L {Importe:N2}";
    }

    public class MaterialIngreso
    {
        public int IdMaterial { get; set; }
        public string Nombre { get; set; } = "";
        public decimal StockActual { get; set; }
        public bool Activo { get; set; }
    }

    public class MaterialConsumido
    {
        public int IdMaterial { get; set; }
        public string Nombre { get; set; } = "";
        public decimal Cantidad { get; set; }
    }

    public class RegistrarIngresoRequest
    {
        public int IdPaciente { get; set; }
        public DateTime Fecha { get; set; }

        public List<int> IdsTratamientosVisita
        { get; set; } = new();

        public decimal Descuento { get; set; }
        public string MedioPago { get; set; } = "";

        public List<MaterialConsumido> Materiales
        { get; set; } = new();
    }

    public class IngresoListado
    {
        public int IdIngreso { get; set; }
        public DateTime Fecha { get; set; }
        public string Paciente { get; set; } = "";
        public string Tratamiento { get; set; } = "";
        public string MedioPago { get; set; } = "";
        public decimal Subtotal { get; set; }
        public decimal Descuento { get; set; }
        public decimal Total { get; set; }
        public bool Activo { get; set; }

    }
}

