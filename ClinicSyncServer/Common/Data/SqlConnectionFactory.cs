using ClinicSyncServer.Common.Opciones;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace ClinicSyncServer.Common.Data
{
    /// <summary>
    /// 
    /// </summary>
    public class SqlConnectionFactory(IOptions<OpcionesConexion> opciones)
    {
        /// <summary>
        /// The cadena
        /// </summary>
        private readonly string _cadena = opciones.Value.Clinica;

        // Crea la conexión a la base de datos.
        /// <summary>
        /// Crears this instance.
        /// </summary>
        /// <returns></returns>
        public SqlConnection Crear() => new(_cadena);
    }
}
