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

    /// <summary>
    /// 
    /// </summary>
    public sealed class ResumenDto
    {
        /// <summary>
        /// Gets or sets the total ingresos.
        /// </summary>
        /// <value>
        /// The total ingresos.
        /// </value>
        public decimal TotalIngresos { get; set; }
        /// <summary>
        /// Gets or sets the total egresos.
        /// </summary>
        /// <value>
        /// The total egresos.
        /// </value>
        public decimal TotalEgresos { get; set; }
        /// <summary>
        /// Gets or sets the saldo neto.
        /// </summary>
        /// <value>
        /// The saldo neto.
        /// </value>
        public decimal SaldoNeto { get; set; }
        /// <summary>
        /// Gets or sets the grafico.
        /// </summary>
        /// <value>
        /// The grafico.
        /// </value>
        public GraficoResumenDto? Grafico { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class GraficoResumenDto
    {
        /// <summary>
        /// Gets or sets the periodo.
        /// </summary>
        /// <value>
        /// The periodo.
        /// </value>
        public string Periodo { get; set; } = "";
        /// <summary>
        /// Gets or sets the agrupacion.
        /// </summary>
        /// <value>
        /// The agrupacion.
        /// </value>
        public string Agrupacion { get; set; } = "";
        /// <summary>
        /// Gets or sets the desde.
        /// </summary>
        /// <value>
        /// The desde.
        /// </value>
        public string Desde { get; set; } = "";
        /// <summary>
        /// Gets or sets the hasta.
        /// </summary>
        /// <value>
        /// The hasta.
        /// </value>
        public string Hasta { get; set; } = "";

        /// <summary>
        /// Gets or sets the etiquetas.
        /// </summary>
        /// <value>
        /// The etiquetas.
        /// </value>
        public IReadOnlyList<string> Etiquetas { get; set; } = [];
        /// <summary>
        /// Gets or sets the ingresos.
        /// </summary>
        /// <value>
        /// The ingresos.
        /// </value>
        public IReadOnlyList<decimal> Ingresos { get; set; } = [];
        /// <summary>
        /// Gets or sets the egresos.
        /// </summary>
        /// <value>
        /// The egresos.
        /// </value>
        public IReadOnlyList<decimal> Egresos { get; set; } = [];
    }

    /// <summary>
    /// 
    /// </summary>
    public class GraficosApiService
    {
        /// <summary>
        /// Obteners the grafico asynchronous.
        /// </summary>
        /// <param name="periodo">The periodo.</param>
        /// <param name="desde">The desde.</param>
        /// <param name="hasta">The hasta.</param>
        /// <returns></returns>
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

