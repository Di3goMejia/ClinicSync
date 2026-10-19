using ClinicSyncServer.Common.Opciones;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace ClinicSyncServer.Common.Data
{
    public class SqlConnectionFactory(IOptions<OpcionesConexion> opciones)
    {
        private readonly string _cadena = opciones.Value.Clinica;

        // Crea la conexión a la base de datos.
        public SqlConnection Crear() => new(_cadena);
    }
}
