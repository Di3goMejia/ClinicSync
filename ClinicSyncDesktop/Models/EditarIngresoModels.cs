
using System;
using System.Collections.Generic;

namespace ClinicSyncDesktop.Models
{
    public class EditarIngresoDetalle
    {
        public int IdIngreso { get; set; }

        public int IdPaciente { get; set; }

        public string Paciente { get; set; } = "";

        public DateTime Fecha { get; set; }

        public decimal Subtotal { get; set; }

        public decimal Descuento { get; set; }

        public decimal Total { get; set; }

        public string MedioPago { get; set; } = "";

        public bool Activo { get; set; }

        public List<MaterialConsumido> Materiales { get; set; } = new();

        public List<TratamientoIngreso> Tratamientos
        { get; set; } = new();
    }

    public class EditarIngresoRequest
    {
        public int IdPaciente { get; set; }

        public DateTime Fecha { get; set; }

        public decimal Descuento { get; set; }

        public string MedioPago { get; set; } = "";

        public bool Activo { get; set; }

        public List<MaterialConsumido> Materiales { get; set; } = new();

        public List<int> IdsTratamientos
        { get; set; } = new();
    }
}
