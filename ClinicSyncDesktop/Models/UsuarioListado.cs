using System;
using System.Collections.Generic;
using System.Text;

namespace ClinicSyncDesktop.Models
{
    public class UsuarioListado
    {
        public int IdUsuario { get; set; }
        public int IdRol { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string? UltimoAcceso { get; set; }
        public string Estado { get; set; } = "PENDIENTE";
    }
}
