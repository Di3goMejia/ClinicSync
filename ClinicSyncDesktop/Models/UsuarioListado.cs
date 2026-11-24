using System;
using System.Collections.Generic;
using System.Text;

namespace ClinicSyncDesktop.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class UsuarioListado
    {
        /// <summary>
        /// Gets or sets the identifier usuario.
        /// </summary>
        /// <value>
        /// The identifier usuario.
        /// </value>
        public int IdUsuario { get; set; }
        /// <summary>
        /// Gets or sets the identifier rol.
        /// </summary>
        /// <value>
        /// The identifier rol.
        /// </value>
        public int IdRol { get; set; }
        /// <summary>
        /// Gets or sets the nombre rol.
        /// </summary>
        /// <value>
        /// The nombre rol.
        /// </value>
        public string NombreRol { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the nombre completo.
        /// </summary>
        /// <value>
        /// The nombre completo.
        /// </value>
        public string NombreCompleto { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the ultimo acceso.
        /// </summary>
        /// <value>
        /// The ultimo acceso.
        /// </value>
        public string? UltimoAcceso { get; set; }
        /// <summary>
        /// Gets or sets the estado.
        /// </summary>
        /// <value>
        /// The estado.
        /// </value>
        public string Estado { get; set; } = "PENDIENTE";
    }
}
