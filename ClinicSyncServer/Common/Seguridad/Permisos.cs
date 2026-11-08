
namespace ClinicSyncServer.Common.Seguridad
{
    public static class Permisos
    {
        // USUARIOS
        public const string UsuariosLeer = "USUARIOS_LEER";
        public const string UsuariosCrear = "USUARIOS_CREAR";
        public const string UsuariosModificar = "USUARIOS_MODIFICAR";
        public const string UsuariosEstado = "USUARIOS_ESTADO";

        // TRATAMIENTOS
        public const string TratamientosCrear = "TRATAMIENTOS_CREAR";
        public const string TratamientosLeer = "TRATAMIENTOS_LEER";
        public const string TratamientosModificar = "TRATAMIENTOS_MODIFICAR";
        public const string TratamientosEstado = "TRATAMIENTOS_ESTADO";

        // INVENTARIO
        public const string InventarioCrear = "INVENTARIO_CREAR";
        public const string InventarioLeer = "INVENTARIO_LEER";
        public const string InventarioModificar = "INVENTARIO_MODIFICAR";
        public const string InventarioEstado = "INVENTARIO_ESTADO";
        public const string InventarioConsultarMovimientos = "INVENTARIO_CONSULTAR_MOVIMIENTOS";
        public const string InventarioRegistrarMovimientos = "INVENTARIO_REGISTRAR_MOVIMIENTOS";

        // EXPEDIENTES
        public const string ExpedientesLeer = "EXPEDIENTES_LEER";
        public const string ExpedientesModificar = "EXPEDIENTES_MODIFICAR";

        // ARCHIVOS DEL EXPEDIENTE
        public const string ArchivosLeer = "ARCHIVOS_LEER";
        public const string ArchivosSubir = "ARCHIVOS_SUBIR";
        public const string ArchivosDesactivar = "ARCHIVOS_DESACTIVAR";


        // EGRESOS
        public const string EgresosCrear = "EGRESOS_CREAR";
        public const string EgresosLeer = "EGRESOS_LEER";
        public const string EgresosModificar = "EGRESOS_MODIFICAR";


        // AGENDA
        public const string AgendaCrear = "AGENDA_CREAR";
        public const string AgendaLeer = "AGENDA_LEER";
        public const string AgendaModificar = "AGENDA_MODIFICAR";

        // ROLES
        public const string RolesLeer = "ROLES_LEER";
        public const string RolesCrear = "ROLES_CREAR";
        public const string RolesModificar = "ROLES_MODIFICAR";
        public const string RolesEstado = "ROLES_ESTADO";

        public static readonly IReadOnlyList<string> Todos =
        [
            // USUARIOS
            UsuariosLeer,
            UsuariosCrear,
            UsuariosModificar,
            UsuariosEstado,

            // ROLES
            RolesLeer,
            RolesCrear,
            RolesModificar,
            RolesEstado,

            // TRATAMIENTOS
            TratamientosLeer,
            TratamientosCrear,
            TratamientosModificar,
            TratamientosEstado,

            // INVENTARIO
            InventarioCrear,
            InventarioLeer,
            InventarioModificar,
            InventarioEstado,
            InventarioConsultarMovimientos,
            InventarioRegistrarMovimientos,

            // EXPEDIENTES
            ExpedientesLeer,
            ExpedientesModificar,

            // ARCHIVOS
            ArchivosLeer,
            ArchivosSubir,
            ArchivosDesactivar,

            // EGRESOS
            EgresosCrear,
            EgresosLeer,
            EgresosModificar,

            // AGENDA
            AgendaCrear,
            AgendaLeer,
            AgendaModificar
        
        ];
    } 

    public static class ClaimsPersonalizados
    {
        public const string Rol = "rol";
        public const string Permiso = "permiso";
        public const string Proposito = "proposito";
    }
}
