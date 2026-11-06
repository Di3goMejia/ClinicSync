using ClinicSyncDesktop.Data;
using ClinicSyncDesktop.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace ClinicSyncDesktop.Services
{
    public sealed class ResumenFinanzasApiService
    {
        public async Task<IReadOnlyList<MovimientoResumen>> ObtenerResumenAsync(
            string periodo,
            DateOnly? desde = null,
            DateOnly? hasta = null)
        {
            string ruta =
                "api/finanzas/resumen" +
                $"?periodo={Uri.EscapeDataString(periodo)}";

            if (desde is not null)
                ruta += $"&desde={desde.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}";

            if (hasta is not null)
                ruta += $"&hasta={hasta.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}";

            return await ClienteApi.ObtenerAutenticadoAsync<List<MovimientoResumen>>(ruta);
        }
    }
}
