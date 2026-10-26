
namespace ClinicSyncServer.Common.Seguridad
{
    public static class Permisos
    {
        // USUARIOS
        public const string UsuariosLeer = "USUARIOS_LEER";
        public const string UsuariosCrear = "USUARIOS_CREAR";

        // TRATAMIENTOS
        public const string TratamientosCrear = "TRATAMIENTOS_CREAR";
        public const string TratamientosLeer = "TRATAMIENTOS_LEER";
        public const string TratamientosModificar = "TRATAMIENTOS_MODIFICAR";
        public const string TratamientosEstado = "TRATAMIENTOS_ESTADO";

        // EXPEDIENTES
        public const string ExpedientesLeer = "EXPEDIENTES_LEER";
        public const string ExpedientesModificar = "EXPEDIENTES_MODIFICAR";

        // ARCHIVOS DEL EXPEDIENTE
        public const string ArchivosLeer = "ARCHIVOS_LEER";
        public const string ArchivosSubir = "ARCHIVOS_SUBIR";
        public const string ArchivosDesactivar = "ARCHIVOS_DESACTIVAR";

        public static readonly IReadOnlyList<string> Todos =
        [
            // USUARIOS
            UsuariosLeer,
            UsuariosCrear,

            // TRATAMIENTOS
            TratamientosLeer,
            TratamientosCrear,
            TratamientosModificar,
            TratamientosEstado,

            // EXPEDIENTES
            ExpedientesLeer,
            ExpedientesModificar,

            // ARCHIVOS
            ArchivosLeer,
            ArchivosSubir,
            ArchivosDesactivar
        ];
    }

    public static class ClaimsPersonalizados
    {
        public const string Rol = "rol";
        public const string Permiso = "permiso";
        public const string Proposito = "proposito";
    }
}
