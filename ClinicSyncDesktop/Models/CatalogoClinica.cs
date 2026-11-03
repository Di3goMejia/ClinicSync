using System;
using System.Collections.Generic;

namespace ClinicSyncDesktop.Models
{
    public static class CatalogoClinica
    {
        private static readonly Dictionary<string, decimal> _preciosBase = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Endodoncia", 1500m },
            { "Tartrectomías", 800m },
            { "Implantología y Prótesis", 3500m }
        };

        private static readonly List<string> _pacientes = new()
        {
            "Karem N. Sánchez",
            "Diego J. Mejía",
            "María Fernanda Gómez",
            "Carlos Eduardo Rivera",
            "Ana Lucía Martínez"
        };

        private static readonly List<string> _tratamientos = new()
        {
            "Endodoncia",
            "Tartrectomías",
            "Implantología y Prótesis"
        };

        public static IReadOnlyList<string> Pacientes => _pacientes.AsReadOnly();
        public static IReadOnlyList<string> Tratamientos => _tratamientos.AsReadOnly();

        public static decimal ObtenerPrecioBaseTratamiento(string? nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return 0m;
            if (_preciosBase.TryGetValue(nombre.Trim(), out decimal p)) return p;
            return 1200m;
        }

        public static void RegistrarPrecio(string nombre, decimal precio)
        {
            if (!string.IsNullOrWhiteSpace(nombre))
            {
                _preciosBase[nombre.Trim()] = precio;
                if (!_tratamientos.Contains(nombre.Trim(), StringComparer.OrdinalIgnoreCase))
                {
                    _tratamientos.Add(nombre.Trim());
                }
            }
        }

        public static void RegistrarPaciente(string nombre)
        {
            if (!string.IsNullOrWhiteSpace(nombre) && !_pacientes.Contains(nombre.Trim(), StringComparer.OrdinalIgnoreCase))
            {
                _pacientes.Add(nombre.Trim());
            }
        }
    }
}

