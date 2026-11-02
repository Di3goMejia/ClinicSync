using ClinicSyncDesktop.Data;
using ClinicSyncDesktop.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading.Tasks;


namespace ClinicSyncDesktop.Services
{

    public sealed class ResumenDto
    {
        public decimal TotalIngresos { get; set; }
        public decimal TotalEgresos { get; set; }
        public decimal SaldoNeto { get; set; }
        public GraficoResumenDto? Grafico { get; set; }
    }

    public sealed class GraficoResumenDto
    {
        public string Periodo { get; set; } = "";
        public string Agrupacion { get; set; } = "";
        public string Desde { get; set; } = "";
        public string Hasta { get; set; } = "";

        public IReadOnlyList<string> Etiquetas { get; set; } = [];
        public IReadOnlyList<decimal> Ingresos { get; set; } = [];
        public IReadOnlyList<decimal> Egresos { get; set; } = [];
    }

    public class GraficosApiService
    {
        public async Task<ResumenDto> ObtenerGraficoAsync(
            string periodo, DateOnly? desde = null, DateOnly? hasta = null)
        {
            string ruta =
                "api/finanzas/graficos" +
                $"?periodo={Uri.EscapeDataString(periodo)}";

            if (desde is not null)
                ruta += $"&desde={desde.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}";
               
            if (hasta is not null)
                ruta += $"&hasta={hasta.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}";

            return await ClienteApi.ObtenerAutenticadoAsync<ResumenDto>(ruta);
        }
    }
}

