using System;
using System.Collections.Generic;
using System.Text;

namespace ClinicSyncDesktop.Api
{
    public static class QueryString
    {
        public static string Crear(string ruta,
            IEnumerable<KeyValuePair<string, string?>> parametros)
        {
            var partes = parametros
                .Where(p => !string.IsNullOrWhiteSpace(p.Value))
                .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value!)}");

            var query = string.Join("&", partes);
            return query.Length == 0 ? ruta : $"{ruta}?{query}";
        }
    }
}
