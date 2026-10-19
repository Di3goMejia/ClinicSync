using ClinicSyncServer.Features;
using Microsoft.Data.SqlClient;

namespace ClinicSyncServer.Common.Data
{
    public static class SqlErrorTranslator
    {
        // Los procedimientos almacenados emiten errores en el rango 50000-60000.
        // 547 es un error SQL emitido por violación de la restricción FOREIGN KEY.
        // 2601 y 2627 son errores SQL emitodos por violación de condiciones UNIQUE. 
        public static bool EsTraducible(SqlException ex) =>
            ex.Number is >= 50000 and < 60000 or 547 or 2601 or 2627;

        public static Exception Traducir(SqlException ex) => ex.Number switch
        {
            >= 50000 and < 60000 => new ReglaNegocioException(ex.Message, ex.Number),
            2601 or 2627         => new ConflictoException("El registro ya existe."),
            547                  => new ReglaNegocioException("La referencia indicada no existe.", ex.Number),
            _                    => ex
        };      
    }
}
