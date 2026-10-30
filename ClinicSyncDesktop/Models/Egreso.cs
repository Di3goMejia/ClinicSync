using System;
using System.Collections.Generic;
using System.Text;

namespace ClinicSyncDesktop.Models
{
    public class Egreso
    {
        public int IdEgreso { get; set; }

        public string Tipo { get; set; } = string.Empty;

        public DateTime Fecha { get; set; }

        public string Concepto { get; set; } = string.Empty;

        public string MedioPago { get; set; } = string.Empty;

        public decimal Monto { get; set; }

        public bool Activo { get; set; }

        public int? IdCompra { get; set; }

        public string TipoTexto => Capitalizar(Tipo);

        public string MedioPagoTexto => Capitalizar(MedioPago);

        private static string Capitalizar(string texto) =>
            string.IsNullOrEmpty(texto)
                ? ""
                : char.ToUpper(texto[0]) + texto[1..].ToLower();

    }
}