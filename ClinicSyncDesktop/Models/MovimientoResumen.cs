using System;

namespace ClinicSyncDesktop.Models
{
    public sealed class MovimientoResumen
    {
        public DateTime Fecha { get; set; }
        public string Concepto { get; set; } = "";
        public string MedioPago { get; set; } = "";
        public decimal Monto { get; set; }
        public string Tipo { get; set; } = "";
    }
}
