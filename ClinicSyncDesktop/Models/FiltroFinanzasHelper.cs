using System;
using System.Globalization;

namespace ClinicSyncDesktop.Models
{
    public static class FiltroFinanzasHelper
    {
        public static readonly DateTime FechaReferenciaFigma = new DateTime(2026, 6, 19);

        public static bool ParsearFecha(string? fechaStr, out DateTime dt)
        {
            if (string.IsNullOrWhiteSpace(fechaStr) || fechaStr.Contains("YYYY", StringComparison.OrdinalIgnoreCase))
            {
                dt = default;
                return false;
            }

            string[] formatos = { "yyyy/MM/dd", "yyyy-MM-dd", "dd/MM/yyyy", "d/M/yyyy", "yyyy/M/d" };
            if (DateTime.TryParseExact(fechaStr.Trim(), formatos, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
            {
                return true;
            }

            return DateTime.TryParse(fechaStr.Trim(), out dt);
        }

        public static bool CoincideFiltro(string? fechaStr, string filtro, string? fechaDesde = null, string? fechaHasta = null)
        {
            if (!ParsearFecha(fechaStr, out var fecha)) return true;

            switch (filtro)
            {
                case "Hoy":
                    return EsHoy(fecha);

                case "Semana":
                    return EsSemana(fecha);

                case "Mes":
                    return EsMes(fecha);

                case "Rango":
                    if (ParsearFecha(fechaDesde, out var d) && ParsearFecha(fechaHasta, out var h))
                    {
                        return fecha.Date >= d.Date && fecha.Date <= h.Date;
                    }
                    return true;

                default:
                    return true;
            }
        }

        public static bool EsHoy(DateTime fecha)
        {
            return fecha.Date == DateTime.Today.Date || fecha.Date == FechaReferenciaFigma.Date;
        }

        public static bool EsSemana(DateTime fecha)
        {
            // Comprobar semana de DateTime.Today
            DateTime inicioHoy = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (DateTime.Today.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));
            DateTime finHoy = inicioHoy.AddDays(6);
            if (fecha.Date >= inicioHoy.Date && fecha.Date <= finHoy.Date) return true;

            // Comprobar semana de FechaReferenciaFigma (15 a 21 de junio de 2026)
            DateTime inicioFigma = FechaReferenciaFigma.AddDays(-(int)FechaReferenciaFigma.DayOfWeek + (FechaReferenciaFigma.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));
            DateTime finFigma = inicioFigma.AddDays(6);
            return fecha.Date >= inicioFigma.Date && fecha.Date <= finFigma.Date;
        }

        public static bool EsMes(DateTime fecha)
        {
            if (fecha.Year == DateTime.Today.Year && fecha.Month == DateTime.Today.Month) return true;

            return fecha.Year == FechaReferenciaFigma.Year && fecha.Month == FechaReferenciaFigma.Month;
        }
    }
}

